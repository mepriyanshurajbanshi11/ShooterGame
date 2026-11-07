# Phases & Roadmap

Status key: `[x]` done, `[~]` in progress, `[ ]` not started.

Update this file and [MEMORY.md](MEMORY.md) when a task finishes. Follow the phases in order.

## Phase 0: Setup `[x]`

- [x] Install Unity Hub, Unity 6 LTS, Android module
- [x] Create project `ShooterGame` (Universal 3D)
- [x] Git repo and `.gitignore`
- [x] Folder structure from [ARCHITECTURE.md](ARCHITECTURE.md#2-folder-structure)

## Phase 1: Solo prototype `[x]`

**Goal:** one player can move, look, switch camera, and shoot a target.

- [x] Floor, Player (`CharacterController`), `CamPivot`, `FppCam`, `TppCam`
- [x] Movement: move, run, jump, look (`PlayerController`)
- [x] Camera: TPP/FPP toggle (`V`) and scope zoom (right mouse / aim) (`CameraSwitch`)
- [x] Gun: hitscan gun, ammo, reload, fire rate (`Gun`)
- [x] Target: hit feedback, headshot bonus (2x), destruction (`Target`)
- [x] Input router: unified PC + touch input (`PlayerInputRouter`)
- [x] Crosshair, ammo text, health bar HUD (`SimpleHUD`: dynamic stance/move spread, green-to-red bar, scope hiding)
- [x] Hit marker and damage numbers (`HitMarkerUI` center X, `DamageNumberManager` pooled 3D floating text, `CombatEvents` decoupled event bus)

**Acceptance:** everything above works in editor Play mode with no console errors.

## Phase 2: Weapons and combat feel `[~]`

- [x] `WeaponData` ScriptableObject, weapon slots, weapon switch (`WeaponHolder`)
- [~] Guns: AR, SMG, sniper (starter weapons generated), shotgun, pistol
- [ ] Recoil pattern and spread, muzzle flash, sound
- [x] Scope types per weapon (red dot 50, 2x 30, 4x 15, 8x 8)
- [ ] Grenade, medkit, armor
- [ ] Player health, death, respawn

**Acceptance:** all 5 guns feel different and are tuned through data only.

## Phase 3: Mobile input `[~]`

- [x] `PlayerInputRouter` abstraction
- [x] On-screen joystick, look area, fire, aim, jump, reload, crouch, prone, TPP/FPP (`VirtualJoystick`, `TouchLookArea`, `HoldButton`)
- [x] Touch UI auto-builder tool (`MobileUIBuilder` under `Tools > Build Mobile Controls`)
- [ ] Android build runs, controls usable on a real phone
- [ ] Quality presets (Low / Medium / High)

**Acceptance:** same gameplay playable on PC and on a real Android phone.

## Phase 4: Maps

- [ ] Training Ground (open arena)
- [ ] Warehouse (close combat)
- [ ] Desert Village (cover, long range)
- [ ] Spawn points per team

**Acceptance:** each map is playable solo with bots or targets.

## Phase 5: Multiplayer core `[~]`

- [x] Networking research & architecture plan completed (`docs/networking-plan.md`)
- [ ] Photon Fusion installed, App ID set (local config, not committed)
- [ ] Host / join with session name (room code)
- [ ] Networked player, movement prediction, camera only for local player
- [ ] Networked shooting, host-authoritative damage and health
- [ ] Test: 2 instances on one PC
- [ ] Test: 2 players in different countries. Compare regions `sg` / `in` / `au`, show ping

**Acceptance:** two real players in different countries can shoot each other.

## Phase 6: Match system

- [ ] Main menu, lobby, room code UI
- [ ] Team select, team sizes 1-4 vs 1-4, max 8
- [ ] Team Deathmatch and Elimination (`ModeData`)
- [ ] Timer, kill counter, scoreboard, win/lose screen
- [ ] Back to lobby after match

**Acceptance:** full match loop from lobby to results, repeatable.

## Phase 7: Voice

- [ ] Unity Gaming Services project, Vivox enabled
- [ ] Team voice channel by room code, push-to-talk / open mic, mute
- [ ] Android mic permission

**Acceptance:** two players talk in-game on PC and Android.

## Phase 8: Polish and extras

- [ ] Minimap, pings / quick chat
- [ ] Knocked state and revive
- [ ] Skins, emotes
- [ ] Room filter: All / PC only / Mobile only, optional aim assist
- [ ] Audio pass, UI pass, performance pass on a low-end phone

## Phase 9: Release prep (optional)

- [ ] Reconnect handling, report and mute
- [ ] Decide: dedicated server for strangers
- [ ] Play Store (one-time fee) or share APK / itch.io

