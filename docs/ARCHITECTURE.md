# Architecture

## 1. Stack (all free tier)

| Area | Choice | Note |
|---|---|---|
| Engine | Unity 6 LTS, C# | Android + Windows from one project |
| Render pipeline | URP | Light enough for mobile |
| Multiplayer | Photon Fusion 2 (Photon Cloud) | Free 100 CCU plan, one app only. Hard cap at limit |
| Network mode | Host Mode (one player is host) | No rented servers. See [3.6](#36-networking) |
| Voice | Unity Vivox (Unity Gaming Services) | Free tier 5,000 PCU/month. No hard cap, do not exceed |
| Auth | Unity Authentication (anonymous / Google) | Or simple guest ID first |
| Assets | Blender, Mixamo, Kenney, free Unity Asset Store | Check licenses |
| VCS | Git + GitHub | Do not commit `Library/`, `Temp/`, or secrets |
| IDE | VS Code or Visual Studio Community | |

> Before adding any new paid or metered service, ask the owner.

## 2. Folder structure

```text
Assets/
  _Project/
    Scenes/            Boot, MainMenu, Lobby, Map_Training, Map_Warehouse, Map_Desert
    Scripts/
      Player/          PlayerController, CameraSwitch, PlayerHealth, PlayerInputRouter
      Weapons/         Gun, WeaponData (ScriptableObject), Recoil, Scope
      Combat/          Damageable, Target, HitMarker
      Net/             NetworkRunnerHandler, RoomManager, TeamManager, MatchState
      Voice/           VoiceManager (Vivox wrapper)
      UI/              HUD, MainMenuUI, LobbyUI, Scoreboard, MobileControls
      Core/            GameSettings, Constants, Utils
    Prefabs/
    ScriptableObjects/ Weapons, Maps, Modes
    Art/ Audio/ UI/
```

## 3. Key systems

### 3.1 Input abstraction

- `PlayerInputRouter` reads input and exposes: Move, Look, Fire, Aim, Jump, Crouch, Prone, Reload, SwitchWeapon, SwitchCamera, Grenade.
- PC source: keyboard and mouse. Mobile source: on-screen joystick and buttons (Unity Input System + on-screen controls).
- Gameplay scripts read only the router, never raw `Input` calls. PC and mobile share one code path, which networking also requires.
- During the prototype, Active Input Handling is set to **Both**. Migrate to the new Input System later.

### 3.2 Camera system

- Player has a `CamPivot` with two cameras: `FppCam` (eyes) and `TppCam` (behind, over shoulder).
- `CameraSwitch` toggles any time (`V` key or on-screen button). The choice is saved in `PlayerPrefs`.
- ADS changes FOV by scope (red dot 50, 2x 30, 4x 15, 8x 8), smoothed with `Lerp`.
- Hide the own body mesh in FPP for the local player only. Remote players always see the body.
- Only the local player's camera is enabled. Disable cameras and `AudioListener` on remote players.

### 3.3 Weapons

- Data-driven: `WeaponData` ScriptableObject holds damage, headshot multiplier, fire rate, magazine, reload time, range, recoil pattern, allowed scopes, and spread.
- Hitscan via raycast from the camera center. Shotgun uses multiple rays.
- The host validates hits. The client shows instant local feedback (muzzle flash, sound). The host applies damage.

### 3.4 Health and damage

- `Damageable` component: health, armor, `ApplyDamage(amount, hitZone, attacker)`.
- Hit zones: head x2, body x1, limbs x0.75 (tunable in `WeaponData`).
- Health is a networked property. Only the host changes it.

### 3.5 Match flow

```text
Boot -> MainMenu -> Lobby (room code, team select) -> Countdown -> Playing -> RoundEnd/MatchEnd -> Results -> Lobby
```

- `MatchState` is a networked object holding state, timer, and scores.
- `TeamManager` assigns teams. Team sizes 1-4 each, max 8 total.
- Modes are ScriptableObjects (`ModeData`: kill limit, respawn on/off, rounds, time limit).

### 3.6 Networking

- Photon Fusion **Host Mode**. One player hosts. Others connect through the Photon Cloud relay. The session name is the room code.
- Network objects: Player, Projectile (grenade), Pickups, MatchState.
- Player movement: client-side prediction with host-authoritative state (Fusion `NetworkCharacterController`).
- Region: try Singapore (`sg`) first, compare with `in` and `au` using the ping display.
- Show ping on the HUD. Warn above 200 ms.
- Host advantage: the host has zero latency. Mitigation: rotate host each match and show a host indicator. Accepted for friends play.
- The free Photon plan hard-caps CCU. Handle `MaxCcuReached` with a friendly message.

### 3.7 Voice

- `VoiceManager` wraps Vivox. Join a channel by room code. Team channel by default, optional all-chat.
- Push-to-talk and open mic options. Mute player button.
- Works on Android (mic permission required) and PC.

### 3.8 Mobile vs PC fairness

- Room filter: All / PC only / Mobile only.
- Optional light aim assist for touch (setting, off for PC).
- Quality presets: Low / Medium / High. Default is Low on Android.

### 3.9 Anti-cheat (basic)

- Host authority for damage and health. Clients never send "I hit for X". They send "I fired from here, aiming there".
- Host Mode can be cheated by the host. Fine for friends. A dedicated server is a later phase.

## 4. Build targets

- **Windows:** x64 (IL2CPP or Mono)
- **Android:** IL2CPP, ARM64, minimum API per Unity 6 defaults. Share the APK directly for testing.

## 5. Performance rules

- Object pooling for bullet effects and decals
- No per-frame `Find` calls. Cache references.
- Low-poly models, baked lighting where possible, limited real-time lights
- Profile on a real phone, not only in the editor
