# Game Design

## 1. Pillars

1. Fast, readable team fights. Matches last about 5 to 10 minutes.
2. Free Fire / PUBG Mobile style aiming: scope view, headshots matter.
3. Same game on phone and PC.
4. Small maps, always action.

## 2. Camera

- **TPP:** behind and slightly over the shoulder, about 3.5 m back and 0.8 m up.
- **FPP:** eye height, own body hidden.
- **Switch:** `V` (PC) or on-screen button (mobile), any time.
- **ADS:** hold right mouse (PC) or aim button (mobile). Camera zooms to scope FOV. Release returns to the chosen camera.
- **Default FOV:** 60.

## 3. Scopes

| Scope | FOV | Notes |
|---|---|---|
| Hip fire | 60 | Wide spread |
| Red dot | 50 | Mild zoom |
| 2x | 30 | |
| 4x | 15 | |
| 8x | 8 | Sniper range |
| Sniper | 8 | Dark scope overlay, sway |

## 4. Controls

| Action | PC | Mobile |
|---|---|---|
| Move | `WASD` | Left joystick |
| Look | Mouse | Drag right side of screen |
| Fire | Left mouse | Fire button |
| Aim / scope | Right mouse | Aim button |
| Jump | `Space` | Jump button |
| Crouch | `C` / `Ctrl` | Crouch button |
| Prone | `Z` | Prone button |
| Run | `Shift` | Auto-run on joystick edge |
| Reload | `R` | Reload button |
| Switch weapon | `1-4` / wheel | Weapon slot buttons |
| Grenade | `G` | Grenade button |
| TPP / FPP | `V` | Camera button |
| Voice | Hold `T` (push to talk) | Mic button |
| Scoreboard | `Tab` | Scoreboard button |

## 5. Starter weapons

All values are tuning data stored in `WeaponData`.

| Weapon | Damage | Fire rate (s) | Mag | Range | Notes |
|---|---|---|---|---|---|
| Pistol | 15 | 0.25 | 12 | 60 | Backup |
| SMG | 12 | 0.07 | 30 | 50 | Close range, low recoil |
| Assault rifle | 20 | 0.10 | 30 | 100 | All-rounder |
| Shotgun | 10 x 8 pellets | 0.9 | 6 | 25 | Close range burst |
| Sniper | 85 | 1.4 | 5 | 250 | One-shot headshot |

**Damage rules**
- Hit zones: head x2, body x1, limbs x0.75
- Player health: 100. Armor absorbs damage by level.
- Medkit heals over time. Grenade damage falls with distance.

## 6. Modes

- **Team Deathmatch:** kill limit 20 (room setting). Respawn after 3 s at team spawn, with brief spawn protection.
- **Elimination:** no respawn. Round ends when one team is dead. Best of 3 or 5.
- **Team sizes:** 1-4 vs 1-4, max 8.

## 7. Maps

| Map | Style | Best for |
|---|---|---|
| Training Ground | Open arena, few covers | 1v1 |
| Warehouse | Indoor, tight corridors | Shotgun and SMG |
| Desert Village | Buildings, rooftops, long lanes | Sniper |

Each map has spawn points per team, balanced cover, and no spawn-camping lines.

## 8. UI screens

1. **Main menu:** Play, Create room, Join room (code), Settings
2. **Lobby:** room code, player list, team slots, mode, map, time, ready, start
3. **HUD:** crosshair, health, armor, ammo, weapon slots, minimap (later), kill feed, timer, team score, ping, mic status
4. **Scoreboard:** kills, deaths, assists per player and team
5. **Results:** win/lose, stats, back to lobby
6. **Settings:** sensitivity, graphics quality, default camera (TPP/FPP), voice mode, mobile control layout

## 9. Look and feel

- Stylized low-poly, bright colors, readable silhouettes. Original characters and designs only.
- Hit marker and short sound on hit, stronger on headshot. Kill banner.
- Clear team colors (blue vs red), outlines on teammates.

## 10. Audio

- Gun sounds per weapon, footsteps (direction matters), reload, hit, kill
- Separate volume slider for voice chat

## 11. Accessibility and comfort

- Adjustable sensitivity, FOV slider (PC)
- Later: left-handed mobile layout, colorblind-safe team colors
