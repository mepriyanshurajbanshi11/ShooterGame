# Project Memory

Living file. AI assistants: read this at the start of every session and update it at the end. Keep it short and current. Newest info wins.

## Project

- **Name:** ShooterGame (working title)
- **One-liner:** Free Fire / PUBG-style team shooter, 1v1 to 4v4, TPP + FPP with scopes, online with voice, Android + PC.
- **Players:** the main co-players are on different continents (India and Australia), so latency is high. Design for it.

## Decisions

- Team matches only, 1v1 up to 4v4 (16 combos, max 8 players). No battle royale.
- Player can switch TPP/FPP anytime. Scope zoom on aim.
- Online, cross-platform (Android + PC), voice chat required.
- Free to build and run.
- Engine: Unity 6 LTS, C#, URP. Chosen over Unreal because Android is a target and Unity is lighter on phones.
- Multiplayer: Photon Fusion, Host Mode, free 100 CCU plan.
- Voice: Unity Vivox free tier.
- Modes: Team Deathmatch and Elimination.

## Current status

- Phase 0 complete: Git initialized, remote connected to GitHub, project folder structure scaffolded.
- Phase 1 & 3 Core Gameplay Done:
  - **Movement** (`PlayerController`): Move, sprint, jump, ground check, unified input handling.
  - **Camera** (`CameraSwitch`): TPP/FPP toggle (`V`), scope zoom (ADS), body model toggle.
  - **Gun** (`Gun`): Hitscan raycast from camera pivot, fire rate, ammo & reload, headshot calculation, data-driven via `WeaponData`.
  - **Target** (`Target`): Health, hit flash feedback, headshot damage multiplier (2x), destruction.
  - **Input Router** (`PlayerInputRouter`): Full abstraction combining PC (keyboard/mouse) and mobile touch inputs (`VirtualJoystick`, `TouchLookArea`, `HoldButton`).
  - **Mobile UI Builder** (`MobileUIBuilder`): Auto-builds touch controls layout under Canvas via `Tools > Build Mobile Controls`.
- Phase 2 Step 1 Done:
  - `WeaponData`: ScriptableObject data for AR, SMG, Sniper.
  - `WeaponHolder`: 3-slot weapon inventory with keys 1/2/3 and mouse wheel switching.
  - `CreateStarterWeapons`: Editor menu tool under `Tools > Create Starter Weapons`.
- Research Done:
  - `docs/networking-plan.md`: Photon Fusion 2 Host Mode setup, limits, Bangalore ↔ Australia region routing (`asia`).
- Next: In-editor verification of weapons & mobile controls, HUD crosshair/health bar, Phase 5 networking start.

## Open questions

- Final game name
- Art style (default: stylized low-poly)
- Host advantage and lag for India-Australia (plan: test regions `sg` / `in` / `au`, rotate host)
- Move to a dedicated server later for strangers?

## Known bugs

- None recorded yet.

## Setup notes

- Project Settings > Player > Active Input Handling = **Both** (prototype uses legacy `Input`).
- Photon App ID and Vivox keys are kept in a local, git-ignored config. Not in git.
- Test Android by sharing the APK directly. No Play Store needed yet.

## How to update this file

- After each finished task: move the item to status, note decisions with a date, list new bugs.
- Keep each section under about 15 lines. Delete outdated info instead of piling up.

## Change log

- 2026-11-07: Completed Tasks A (Git push), B (WeaponData, Gun update, WeaponHolder, Starter Weapons generator), C (MobileUIBuilder), and D (Networking Plan).
- 2026-11-07: Marked movement, camera, gun, target, and PlayerInputRouter as done. Unity prototype scene with mobile controls verified.
