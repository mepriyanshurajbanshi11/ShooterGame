# Product Requirements (PRD)

**Project:** ShooterGame (working title)

## 1. Vision

A small, polished 3D online team shooter inspired by Free Fire and PUBG Mobile. Not a battle royale. Short team matches from 1v1 up to 4v4, playable on Android and Windows PC, with voice chat. Built for friends in different countries first (for example India and Australia), strangers later.

## 2. Goals

1. Two friends in different countries can create a room, join with a code, and play together or against each other.
2. Same game runs on Android and Windows, with cross-platform play.
3. In-game voice chat.
4. Shooting feels good: aim, recoil, scopes, hit feedback.
5. Built with free tools and free service tiers only.

## 3. Non-goals (for now)

- Battle royale, big open map, vehicles, parachute, 50-player matches
- Paid shop, ads, real-money items
- iOS and console
- Paid dedicated servers

## 4. Players and modes

- Team A (1-4) vs Team B (1-4). All 16 size combinations allowed. Max 8 players per match.
- Room creator picks team sizes, mode, map, and time limit.

| Mode | Rule |
|---|---|
| Team Deathmatch | First team to the kill limit wins (default 20). |
| Elimination | No respawn. Last team alive wins the round. Best of N rounds. |

Teams can be uneven (for example 1v3). An optional damage or health handicap may be added later.

## 5. Features

### MVP (must have)

- TPP and FPP camera, switchable at any time
- Scope / ADS with zoom. Optics: red dot, 2x, 4x, 8x
- Move, run, jump, crouch (prone later)
- Hitscan shooting, body and headshot damage, reload, ammo
- Health, death, respawn (Deathmatch)
- Online multiplayer: 2 players first, then up to 8
- Room code create / join
- Team select, match timer, kill counter, scoreboard, win/lose screen
- Team voice chat
- Android and PC builds

### v1 (after MVP)

- Weapons: AR, SMG, sniper, shotgun, pistol
- Grenades, medkits, armor
- Maps: Training Ground, Warehouse, Desert Village
- Minimap, hit markers, damage numbers, recoil patterns
- Knocked state and revive (team modes)
- Character skins, emotes
- Room filter: mobile-only / PC-only / all-platform

### Later

- Ranks, levels, missions, stats history
- Dedicated servers, stronger anti-cheat
- Matchmaking with strangers, report and mute

## 6. Platforms

- **Windows PC:** keyboard and mouse
- **Android:** touch controls (joystick, fire, aim, jump, crouch, reload, weapon slots, TPP/FPP, grenade)
- One Unity project, two build targets

## 7. Constraints

- Free tools and free tiers only (see [ARCHITECTURE.md](ARCHITECTURE.md)).
- Solo, beginner-friendly developer, AI-assisted. Prefer simple code over clever code.
- Players on different continents means high ping. Design for latency.
- Low-end Android phones must run the game, so quality settings are required.

## 8. Success criteria

- Two friends complete a full 1v1 match over the internet with voice chat.
- 30+ FPS on a mid-range Android phone, 60+ FPS on PC.
- A match can start in under 1 minute from app launch.

## 9. Open questions

- Final game name
- Art style: stylized (Free Fire-like) or semi-realistic. Default: stylized low-poly.
- Host-based or dedicated server once strangers join
