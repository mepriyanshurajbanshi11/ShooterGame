# Photon Fusion 2 Networking Plan: Solo to Multiplayer Migration

## Executive Summary
This document provides the definitive architectural and implementation plan for integrating **Photon Fusion 2 (Host Mode)** into the Unity 6 LTS (6000.6.8f1) shooter project. It details installation procedures, free tier limits, networking topology decisions, cross-continental region routing (Bangalore, India ↔ Australia), and a granular, script-by-script code conversion blueprint from solo gameplay to networked multiplayer.

---

## 1. Exact Installation Steps for Photon Fusion 2 in Unity 6

### 1.1 Download and SDK Package
* **Official SDK Download Page:** [https://www.photonengine.com/fusion/sdk](https://www.photonengine.com/fusion/sdk)
* **SDK Version:** **Photon Fusion 2** (current release: Fusion 2.0+). *Note: Do not install Fusion 1.x; Fusion 1 does not natively support Unity 6.*
* **Distribution Format:** Fusion 2 is primarily distributed as a standalone `.unitypackage` from the Photon website or the Unity Asset Store. Addons (such as *Simple KCC* and *Fusion Menu*) can be brought in via the Unity Package Manager (UPM) via Git URLs.

### 1.2 Import Process
1. Ensure your Unity 6 project is closed or in an idle state.
2. In Unity 6, navigate to **Assets > Import Package > Custom Package...**.
3. Select the downloaded `PhotonFusion_v2.x.x.unitypackage` file.
4. In the package import dialog, ensure all assets, plugins, and assemblies are checked, then click **Import**.
5. Once imported, the Fusion Hub / Setup Wizard window will appear automatically.

### 1.3 Creating a Photon App ID
1. Navigate to the Photon Engine Dashboard: [https://dashboard.photonengine.com/](https://dashboard.photonengine.com/).
2. Log in or create an account.
3. On the Applications page, click **Create a New App**.
4. Configure the form:
   * **Photon SDK:** Select **Fusion**.
   * **SDK Version:** Select **Fusion 2**.
   * **Name:** `ShooterGame` (or your project working title).
5. Click **Create**.
6. On the dashboard, locate the newly created app and copy the **App ID** (a UUID string formatted like `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`).

### 1.4 Pasting the App ID in Unity
1. In the Unity Editor, open the top menu: **Tools > Fusion > Realtime Settings** (or select the `PhotonAppSettings` asset in `Assets/Photon/Fusion/Resources/`).
2. In the Inspector, locate the **App Id Fusion** input field.
3. Paste your copied App ID into this field.
4. ⚠️ **Security & Git Hygiene:** The `PhotonAppSettings.asset` file holds your App ID. If your repository is public or shared broadly, ensure you either keep the repository private or avoid committing your live production App ID (e.g. by adding the settings asset to `.gitignore` or injecting it at build time).

### 1.5 Unity 6 Compatibility Notes
* **Native Unity 6 Support:** Fusion 2 officially supports Unity 6 (6000.x LTS).
* **Missing Namespace / Assembly References on First Import (CS0234):** ⚠️ On fresh Unity 6 imports with sample packages (e.g. Fusion Menu), Unity 6's asset compilation pipeline can occasionally throw compile errors (`CS0234: The type or namespace name 'Menu' does not exist in the namespace 'Fusion'`). If this occurs, restart Unity or right-click `Assets/Photon` and select **Reimport All**.
* **Roslyn IL Weaver & Code Generation:** Fusion 2 uses IL post-processing / Roslyn source generators to compile `[Networked]` properties. Unity 6's Roslyn compiler handles this seamlessly, but all custom networked scripts must be part of an Assembly Definition (`.asmdef`) or compile under the main `Assembly-CSharp.dll` without syntax errors for the weaver to succeed.
* **Asset Serialization:** Ensure **Edit > Project Settings > Editor > Asset Serialization Mode** is set to **Force Text**.

---

## 2. Free Plan Limits (Current as of 2026)

| Parameter | Specification | Details |
|---|---|---|
| **CCU Limit** | **100 CCU** (Evergreen Free Tier) | Concurrently Connected Users across all rooms and regions globally. |
| **Fallback CCU** | **20 CCU** ⚠️ | Accounts that do not specifically initialize the Fusion Free 100 CCU tier default to 20 CCU evaluation mode. |
| **App Limit** | **1 App Rule** | Exactly one free 100 CCU application per Photon account. |
| **Hard Cap Behavior** | **Strict Rejection** | Any connection exceeding the limit is instantly rejected. No queueing; no automatic CCU burst on free tiers. |
| **Disconnect Code** | `ShutdownReason.MaxCcuReached` | Triggered in `INetworkRunnerCallbacks.OnShutdown` when cap is hit. |
| **Bandwidth / Traffic** | **3 GB / CCU / month** | Pooled: ~300 GB / month included for 100 CCU. |
| **Message Frequency** | **500 msg/sec/room** | Soft guideline. Recommended tick rate: 30–60 Hz for Host Mode. |
| **Pricing Source** | [Photon Fusion Pricing](https://www.photonengine.com/fusion/pricing) | Official pricing and subscription tier breakdown. |

### Handling `MaxCcuReached` Gracefully in Code
```csharp
// Example pseudocode: handling CCU limit shutdown in Fusion 2
public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
{
    if (shutdownReason == ShutdownReason.MaxCcuReached)
    {
        Debug.LogWarning("[Network] Server is at maximum capacity (100 CCU).");
        // Notify player via UI overlay
        UIManager.ShowError("Server capacity is currently full. Please try again shortly.");
    }
}
```

---

## 3. Host Mode vs Shared Mode

### 3.1 Overview of Both Modes
* **Shared Mode:**
  * Decentralized authority. Every client has State Authority over the objects they spawn.
  * Photon Cloud relays data packets directly between players.
  * No central simulation tick; clients interpolate other players' states.
  * **No server-side lag compensation.** Hit validation must happen client-side on the shooter's machine and is trusted by everyone else.
* **Host Mode (Selected Architecture):**
  * Client/Server topology. One player's game instance runs both the **authoritative server simulation** (State Authority) and a local client player.
  * Remote players have **Input Authority** over their avatars; they sample local input and send it to the Host.
  * The Host executes simulation ticks deterministically in `FixedUpdateNetwork()` and replicates state down to all clients.
  * Clients run **Client-Side Prediction and Rollback**: their own movement feels zero-ping and responsive, while the Host retains ultimate authority over physics, health, ammo, and positions.
  * **Server-side Lag Compensation:** The Host maintains a historical buffer of player hitboxes (typically 200 ms). When a client fires, the Host rewinds time to match the shooter's perspective and verifies the hit.

### 3.2 Why Host Mode is the Superior Choice for 1v1 to 4v4 Shooters
1. **Authoritative Hit Registration & Anti-Cheat:** In Shared Mode, any modified client could broadcast "I hit player X for 100 damage" and kill players across the map. In Host Mode, damage is strictly calculated by the Host (`TakeDamage` can only be invoked by State Authority).
2. **Lag-Compensated Raycasts:** For cross-continental play (e.g. Bangalore to Australia with ~90–150 ms latency), players cannot lead hitscan shots unnaturally. Host Mode's `Runner.LagCompensatedRaycast` automatically compensates for the client's round-trip time.
3. **Consistent Match State:** Match timer, kill counts, team scores, and respawn logic reside on a single authoritative machine (`MatchState`), avoiding state conflicts or desynchronization between clients.

### 3.3 Pros & Cons for This Specific Project
* **Pros:**
  * **Zero Server Hosting Costs:** Fully operational on Photon Cloud's free tier without renting cloud Linux VMs.
  * **High-Paced FPS Feel:** Client prediction guarantees snappy movement on both PC and mobile touch controls.
  * **Strict Anti-Cheat / Authority:** Essential if expanding beyond private matches to strangers.
* **Cons & Mitigations:**
  * **Host Advantage:** The host player has 0 ms ping. *Mitigation:* In friendly 1v1 / team matches, take turns hosting or rotate hosts between rounds.
  * **Host Disconnect:** If the host exits or crashes, the session terminates unless complex host migration is coded. *Mitigation:* For a friends-first game, simple room re-creation is acceptable; host migration can be added in later phases.
  * **Host CPU Overhead on Mobile:** Running the simulation for 8 players and rendering the game simultaneously is demanding. *Mitigation:* Recommend PC players host matches when cross-playing with mobile players.

---

## 4. Regions to Test (Bangalore, India ↔ Australia Cross-Play)

### 4.1 Photon Cloud Region Tokens

| Region Name | Photon Region Token | Data Center Location | Note |
|---|---|---|---|
| **Singapore** | `asia` | Singapore | ⚠️ Often mistakenly referenced as `sg`. The official token is `asia`. |
| **India** | `in` | Chennai, India | Primary cluster for South Asia. |
| **Australia** | `au` | Sydney, Australia | Primary cluster for Australasia. |

### 4.2 Recommended Default Region: Singapore (`asia`)
For cross-play between **Bangalore, India** and **Australia (Sydney/Melbourne/Perth)**, **Singapore (`asia`) must be the default region**:
* **Submarine Cable Topology:** The primary subsea fiber routes between India and Australia route through Singapore (e.g., SEA-ME-WE series, INDIGO-Central, Australia-Singapore Cable / ASC).
* **Latency Balance:**
  * **Bangalore ↔ Singapore (`asia`):** ~30–45 ms.
  * **Australia (Perth) ↔ Singapore (`asia`):** ~50–60 ms.
  * **Australia (Sydney/Melbourne) ↔ Singapore (`asia`):** ~85–110 ms.
  * **Bangalore ↔ Australia (`au` direct):** ~150–210 ms.
  * **Australia ↔ India (`in` direct):** ~160–220 ms.
* By hosting or relaying in Singapore, both players experience consistent, sub-120 ms ping with symmetrical jitter, preventing one player from suffering an unplayable 200+ ms connection.

### 4.3 Setting Region Programmatically in Fusion 2
```csharp
// Pseudocode: Starting a session with a designated region
using Fusion;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkConnectHandler : MonoBehaviour
{
    public async Task StartGameWithRegion(NetworkRunner runner, GameMode mode, string sessionCode, string regionToken)
    {
        // 1. Clone global app settings
        var customSettings = PhotonAppSettings.Global.AppSettings.GetCopy();
        
        // 2. Set the fixed region token ("asia", "in", or "au")
        customSettings.FixedRegion = regionToken.ToLower();

        // 3. Launch the session
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode, // GameMode.Host or GameMode.Client
            SessionName = sessionCode,
            CustomPhotonAppSettings = customSettings
        });

        if (!result.Ok)
        {
            Debug.LogError($"[Fusion] Failed to start: {result.ShutdownReason}");
        }
    }
}
```

### 4.4 Measuring and Displaying Ping in the UI
Fusion 2 provides real-time Round Trip Time (RTT) in seconds via `NetworkRunner.GetPlayerRtt`:
* **Client to Host Ping:** `runner.GetPlayerRtt(PlayerRef.None)`
* **Host to Specific Client:** `runner.GetPlayerRtt(targetPlayerRef)`

```csharp
// Pseudocode: HUD Ping & Network stats display
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PingMonitor : SimulationBehaviour
{
    public Text pingLabel;
    private float updateTimer;

    public void Update()
    {
        if (Runner == null || !Runner.IsRunning) return;

        updateTimer += Time.unscaledDeltaTime;
        if (updateTimer >= 0.5f) // Update every 500ms
        {
            updateTimer = 0f;
            // RTT is in seconds; multiply by 1000 for ms
            int pingMs = Mathf.RoundToInt((float)(Runner.GetPlayerRtt(PlayerRef.None) * 1000.0));
            string role = Runner.IsServer ? "HOST" : "CLIENT";
            
            pingLabel.text = $"{role} | Ping: {pingMs} ms";
            pingLabel.color = pingMs > 200 ? Color.red : (pingMs > 100 ? Color.yellow : Color.green);
        }
    }
}
```

---

## 5. Step-by-Step Script Conversion Plan: Solo Player → Networked Player

### 5.1 Architecture Overview: Simulation Loop vs. Unity Render Loop
In Photon Fusion 2:
1. **`Update()` / `LateUpdate()`:** Runs locally on every rendered frame. Used strictly for polling raw hardware input, UI updates, and camera position adjustments.
2. **`FixedUpdateNetwork()` (FUN):** Runs at fixed network tick rates (e.g., 60 Hz). Rewinds and replays during prediction rollback. **All gameplay state changes, character movement, firing, and damage validation must occur inside `FixedUpdateNetwork()`.**
3. **`Render()`:** Runs every frame after FUN. Used to apply smooth visual interpolation, mesh animations, and cosmetic effects.

---

### 5.2 `PlayerInputRouter` Conversion

#### Responsibilities
* Sample local keyboard, mouse, and touch inputs during Unity's `Update()`.
* Accumulate button presses between network ticks so quick taps (e.g., jump, shoot) are never missed.
* Implement `INetworkRunnerCallbacks.OnInput` to populate an `INetworkInput` struct and deliver it to Fusion.

#### Network Data Struct
```csharp
// Pseudocode: Network Input Definition
using Fusion;
using UnityEngine;

public enum PlayerButtons
{
    Jump = 0,
    Crouch = 1,
    Prone = 2,
    Fire = 3,
    Aim = 4,
    Reload = 5
}

public struct PlayerNetworkInput : INetworkInput
{
    public Vector2 Move;
    public Vector2 Look;
    public NetworkButtons Buttons;
}
```

#### Code Changes Required
* Keep existing input reading logic (touch + PC), but store button events as bit flags using Fusion's `NetworkButtons`.
* Register `PlayerInputRouter` with `NetworkRunner` to supply data during `OnInput`:
```csharp
// Pseudocode: Polling and providing input to Fusion
public void OnInput(NetworkRunner runner, NetworkInput input)
{
    var netInput = new PlayerNetworkInput();
    netInput.Move = Move;
    netInput.Look = Look;
    netInput.Buttons = accumulatedButtons;

    input.Set(netInput);

    // Reset accumulated one-shot button presses for next tick
    accumulatedButtons = default;
    MobileInput.ClearOneShots();
}
```

---

### 5.3 `PlayerController` Conversion

#### Responsibilities
* Inherit from `NetworkBehaviour` instead of `MonoBehaviour`.
* Replace raw `CharacterController.Move()` with `NetworkCharacterController` (or Fusion's kinematic controller addon ⚠️).
* Predict movement on the local client with Input Authority; simulate authoritatively on Host.
* Synchronize character stance and camera pitch across remote proxies.

#### Component Requirements
* Add `NetworkObject` to the root prefab.
* Add `NetworkCharacterController` (wraps Unity's `CharacterController` and supports state rollback).

#### Networked Properties
```csharp
// Pseudocode: Networked Player State
[Networked] public Stance CurrentStance { get; set; } = Stance.Stand;
[Networked] public float AimPitch { get; set; } // Tilt angle for remote player models
```

#### Code Changes Required
1. **Movement in `FixedUpdateNetwork()`:**
   * Call `GetInput(out PlayerNetworkInput input)` to retrieve the current tick's verified input.
   * Calculate move vector using character orientation.
   * Call `networkCharacterController.Move(moveVector * speed * Runner.DeltaTime)`.
   * Check `input.Buttons.WasPressed(previousButtons, PlayerButtons.Jump)` to trigger jumps through the NCC.
2. **Stance Changes:**
   * Stance toggling is executed inside `FixedUpdateNetwork()`.
   * When `CurrentStance` updates, the Host and predicting clients update the `CharacterController.height` and center.
3. **Smooth Visual Interpolation in `Render()`:**
   * Stance height blending (`curHeight = Mathf.Lerp(...)`) and local `CamPivot` positioning move to `Render()` to ensure butter-smooth camera movement independent of network tick rate.

---

### 5.4 `Gun` Conversion

#### Responsibilities
* Inherit from `NetworkBehaviour`.
* Replace `Physics.Raycast` with `Runner.LagCompensatedRaycast` on the Host.
* Synchronize ammunition state across all clients via `[Networked]`.
* Provide immediate local audio/visual feedback while deferring damage application to the Host.

#### Networked Properties
```csharp
// Pseudocode: Networked Gun State
[Networked] public int Ammo { get; set; }
[Networked] public int FireCounter { get; set; } // Incremented to trigger remote cosmetic effects
```

#### Lag Compensation Setup
* Attach `HitboxRoot` to the root of every damageable entity (players, targets).
* Attach `Hitbox` components to colliders representing head, torso, and limbs.
* Configure `HitboxBufferLengthInMs = 200` in Fusion's Network Project Config.

#### Code Changes Required
```csharp
// Pseudocode: Authoritative lag-compensated firing
public override void FixedUpdateNetwork()
{
    if (GetInput(out PlayerNetworkInput input))
    {
        // Reload handling
        if (input.Buttons.IsSet(PlayerButtons.Reload) && HasStateAuthority)
        {
            Ammo = magSize;
        }

        // Fire handling
        if (input.Buttons.IsSet(PlayerButtons.Fire) && Ammo > 0 && Runner.SimulationTime >= nextFireTime)
        {
            nextFireTime = Runner.SimulationTime + fireRate;

            // Decrement ammo on Host and Predicted Client
            Ammo--;
            FireCounter++;

            // Only the Host performs the authoritative hit registration
            if (HasStateAuthority)
            {
                if (Runner.LagCompensatedRaycast(
                    camPivot.position,
                    camPivot.forward,
                    range,
                    Object.InputAuthority, // Attacker ref used to rewind hitboxes to shooter's tick
                    out var hit,
                    layerMask: default,
                    HitOptions.IncludePhysX | HitOptions.SubTickAccuracy))
                {
                    if (hit.Hitbox != null)
                    {
                        var target = hit.Hitbox.Root.GetComponent<IDamageable>();
                        if (target != null)
                        {
                            bool isHeadshot = hit.Hitbox.name.Contains("Head");
                            float finalDamage = damage * (isHeadshot ? 2f : 1f);
                            target.TakeDamage(finalDamage);
                        }
                    }
                }
            }
        }
    }
}
```

* **Client Feedback:** In `Render()`, check if `FireCounter` changed or if the local player fired to spawn muzzle flash and play gunshot sounds without network latency.

---

### 5.5 `Target` Conversion

#### Responsibilities
* Inherit from `NetworkBehaviour, IDamageable`.
* Store `Health` as a `[Networked]` property.
* Enforce that only the Host can apply damage and despawn the entity.
* Replicate visual damage feedback (turning red) to all clients using Fusion's `ChangeDetector` or `[OnChangedRender]`.

#### Code Changes Required
```csharp
// Pseudocode: Networked Target
using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Target : NetworkBehaviour, IDamageable
{
    [Networked] public float Health { get; set; } = 100f;

    private ChangeDetector _changes;
    private Renderer _rend;
    private Color _baseColor;

    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        _rend = GetComponentInChildren<Renderer>();
        _baseColor = _rend.material.color;
    }

    public void TakeDamage(float amount)
    {
        // Host Authority: Only the host mutates health
        if (!HasStateAuthority) return;

        Health -= amount;
        if (Health <= 0)
        {
            Runner.Despawn(Object); // Never call Destroy() on a NetworkObject!
        }
    }

    public override void Render()
    {
        // Detects when Health changes on any client and triggers cosmetic flash
        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(Health))
            {
                FlashRed();
            }
        }
    }

    private void FlashRed()
    {
        _rend.material.color = Color.red;
        Invoke(nameof(ResetColor), 0.1f);
    }

    private void ResetColor()
    {
        if (_rend != null) _rend.material.color = _baseColor;
    }
}
```

---

### 5.6 `CameraSwitch` Conversion

#### Responsibilities
* Operates as a purely **client-local** component.
* **Does NOT need `[Networked]` state.** Remote clients never care whether another player is looking in FPP or TPP.
* **Critical Rule:** In multiplayer, every spawned player prefab contains camera objects and `AudioListener` components. **These must be deactivated on all remote proxies**, active ONLY on the local avatar (`Object.HasInputAuthority == true`).

#### Code Changes Required
```csharp
// Pseudocode: Disabling remote cameras
public override void Spawned()
{
    bool isLocalPlayer = Object.HasInputAuthority;

    tppCam.gameObject.SetActive(isLocalPlayer);
    fppCam.gameObject.SetActive(isLocalPlayer);

    var listener = GetComponentInChildren<AudioListener>();
    if (listener != null) listener.enabled = isLocalPlayer;

    if (!isLocalPlayer)
    {
        // Remote proxies always render the full body mesh
        bodyModel.SetActive(true);
        this.enabled = false; // Disable update loop on remote proxies
    }
}
```

---

### 5.7 `SimpleHUD` Conversion

#### Responsibilities
* Remains a local UI MonoBehaviour on the UI canvas.
* Binds to the local player's `Gun` once spawned.
* Displays ammo from `[Networked] gun.Ammo`, ping from `Runner.GetPlayerRtt(PlayerRef.None)`, and local host status.

#### Code Changes Required
```csharp
// Pseudocode: HUD Binding
void Update()
{
    if (localGun == null)
    {
        // Find local player gun instance once spawned
        FindLocalPlayerGun();
        return;
    }

    ammoText.text = $"{localGun.Ammo} / {localGun.magSize}";
    ammoText.color = localGun.Ammo == 0 ? Color.red : Color.white;
}
```

---

## 6. Uncertainties and Watch-Outs (Marked with ⚠️)

1. ⚠️ **Photon Cloud Region Token for Singapore (`asia` vs `sg`):**
   In earlier documentation and general conversation, developers often write `sg`. However, Photon's real-time region token for Singapore is `asia`. Using `"sg"` in `FixedRegion` will cause matchmaking errors or fall back to best-ping resolution.
2. ⚠️ **India Cluster Location (`in`):**
   Photon's India region (`in`) is hosted in **Chennai**, whereas AWS's standard Indian region is Mumbai (`ap-south-1`). Latency from Bangalore to Chennai is exceptionally low (~15–25 ms), making it an ideal test bed for domestic India play.
3. ⚠️ **Free Tier CCU Allocation (100 CCU vs 20 CCU):**
   While Fusion's evergreen indie tier permits 100 CCU free forever on one app, accounts created without explicitly selecting the 100 CCU Fusion plan on the dashboard may default to a 20 CCU evaluation ceiling. Always verify on `dashboard.photonengine.com` that the App type is set to **Fusion 2** with the 100 CCU license active.
4. ⚠️ **`NetworkCharacterController` vs. `Simple KCC` Addon:**
   Fusion 2 includes `NetworkCharacterController` out-of-the-box as a wrapper around Unity's `CharacterController`. While sufficient for the prototype, Photon now recommends the **Simple KCC** addon for production shooters to handle complex capsule-ground snapping, step detection, and head-pitch synchronization without PhysX jitter.
5. ⚠️ **Unity 6 Roslyn Code Weaver and Assembly Definitions:**
   When moving player scripts into `_Project/Scripts/Net/`, ensure that if Assembly Definitions (`.asmdef`) are used, they reference `Fusion.Runtime.dll`. If asmdefs are not configured, all scripts must compile without errors, or Fusion's IL Weaver will fail silently during domain reload.

---

## 7. Migration Checklist & Action Plan

- [ ] **Step 1:** Download `PhotonFusion_v2.x.x.unitypackage` from Photon Engine SDK portal and import into Unity 6.
- [ ] **Step 2:** Create Fusion 2 App ID on `dashboard.photonengine.com` and paste into `Tools > Fusion > Realtime Settings`.
- [ ] **Step 3:** Create `PlayerNetworkInput` struct with `NetworkButtons` (Move, Look, Fire, Jump, Crouch, Prone, Reload).
- [ ] **Step 4:** Implement `INetworkRunnerCallbacks` in `PlayerInputRouter` / `NetworkRunnerHandler` to feed inputs to Fusion via `OnInput`.
- [ ] **Step 5:** Convert `PlayerController` to `NetworkBehaviour`, attach `NetworkObject` and `NetworkCharacterController`, and migrate movement logic to `FixedUpdateNetwork()`.
- [ ] **Step 6:** Convert `Gun` to `NetworkBehaviour`, replace `Physics.Raycast` with `Runner.LagCompensatedRaycast` on the Host, and add `HitboxRoot`/`Hitbox` components to player and target colliders.
- [ ] **Step 7:** Convert `Target` to `NetworkBehaviour` with `[Networked] Health` and `Runner.Despawn()`.
- [ ] **Step 8:** Update `CameraSwitch` to deactivate cameras and `AudioListener` on remote proxy objects (`!HasInputAuthority`).
- [ ] **Step 9:** Build `NetworkRunnerHandler` to launch Host/Client sessions with `FixedRegion = "asia"`.
- [ ] **Step 10:** Test 2 instances on one PC (using Unity Multiplayer Play Mode or standalone build), then test Bangalore ↔ Australia connection.

