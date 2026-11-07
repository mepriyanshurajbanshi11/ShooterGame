# Rules

For contributors and AI assistants (Claude, Antigravity, etc.).

**Read this file first.** Then read [PRD.md](PRD.md), [ARCHITECTURE.md](ARCHITECTURE.md), [ROADMAP.md](ROADMAP.md), [DESIGN.md](DESIGN.md), and [MEMORY.md](MEMORY.md).

## Project rules

1. Engine is Unity 6 LTS with C#. Do not switch engines or major frameworks without asking the owner.
2. Free tools and free tiers only. Never add a paid or metered service without asking.
3. Target Android and Windows from one codebase. Every feature must work with both touch and keyboard/mouse.
4. Gameplay code reads input only through `PlayerInputRouter`, never raw `Input.GetKey`. Prototype scripts may use raw input until the router exists.
5. Networking: health and damage are host-authoritative. Clients send input, never results.
6. Follow [ROADMAP.md](ROADMAP.md) order. Do not start a later phase before the current one works and is tested.

## Owner profile (for AI assistants)

- Needs beginner-friendly explanations: short, step-by-step, simple words.
- Always give exact Unity Editor steps: what to click, what to drag where, which component to add.
- Give complete files, not fragments, unless the change is tiny and the location is obvious.

## Code rules

1. One class per file. File name equals class name.
2. PascalCase for classes and methods, camelCase for fields. Use `_privateField` for private serialized fields if using the underscore style. Stay consistent with existing code.
3. Use namespace `ShooterGame.<Area>` once the project grows past the prototype.
4. Prefer `[SerializeField] private` over public fields.
5. Cache component references in `Awake` or `Start`. No `Find*` or `GetComponent` in `Update`.
6. Use ScriptableObjects for data (weapons, modes, maps). No magic numbers in logic.
7. Keep scripts small and single-purpose. Prefer composition over big manager classes.
8. Comment the why, not the what. Keep comments short.
9. No dead code, no commented-out blocks.
10. Handle null and missing references with clear `Debug.LogError` messages.

## Workflow rules

1. Before changing files, say which files you will create or change, and why.
2. After changing, say how to test it: what to press, what to expect.
3. One feature per change. Small steps. Do not refactor unrelated code.
4. Never delete or rewrite working code without saying so and getting a yes.
5. If requirements are unclear, ask one short question. Do not guess on big decisions.
6. Warn about anything that may break multiplayer later (static state, singletons holding player data, local-only logic).
7. When a phase or task finishes, update [MEMORY.md](MEMORY.md) (status, decisions, known bugs).

## Git rules

- Commit small. Message format: `feat: ...`, `fix: ...`, `refactor: ...`, `docs: ...`.
- `.gitignore` must exclude `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`, `*.csproj`, `*.sln`, and build outputs.
- Never commit API keys, production Photon App IDs, or account tokens. Keep them in a local, git-ignored config file.

## Testing rules

- Test every feature in the editor first, then in a built PC version, then on an Android phone.
- Multiplayer: test with two game instances (editor + build) on one PC before testing over the internet.
- Report known bugs in [MEMORY.md](MEMORY.md). Do not hide them.

## Do not

- Do not copy copyrighted assets, names, logos, or characters from Free Fire, PUBG, or others. Use original or properly licensed free assets only.
- Do not add ads, analytics, or tracking.
- Do not add features outside the current phase "because it is easy".
