# Innkeeper

Innkeeper is a 3/4 top-down cozy fantasy game about renovating a run-down inn.
The player walks around a tile-based world, repairs and decorates the inn, hires
staff, and serves guests with needs (food, drink, lodging, atmosphere) while a
small surrounding village offers shops and exploration. It is single-player,
desktop-first, non-violent, and built solo with AI assistance. A farming supply
chain is a deliberately deferred late-phase feature, not the core.

> **AI assistants & new contributors: read this file first.** It exists to keep
> you on the project's actual APIs and folder layout. The deeper docs live in
> [`docs/`](docs/README.md).

## Stack & versions

Pinning exact versions here so tooling (and AI assistants) don't default to
older APIs.

| Tool | Version | Notes |
|------|---------|-------|
| Unity Editor | **6000.3.16f1** | Marketed as Unity 6.3. |
| URP (Universal Render Pipeline) | **17.3.0** | Configured with the **2D Renderer** (`Assets/Settings/Renderer2D.asset`). |
| New Input System | **1.19.0** | Not the legacy `Input.GetKey` API. |
| TextMeshPro (via `com.unity.ugui`) | **2.0.0** | In Unity 6, TMP ships **inside** `com.unity.ugui` — there is no standalone `com.unity.textmeshpro` package. |
| Cinemachine | **3.1.6** | Cinemachine 3.x uses the `CinemachineCamera` component (not the pre-3.x `CinemachineVirtualCamera`). |
| 2D feature set | — | Tilemap (+ extras), 2D Animation, Aseprite, PSD Importer, SpriteShape. |

Version control: **Git + Git LFS**. `.gitattributes` routes images, audio,
video, and 3D/binary assets through LFS; Unity YAML assets (`.unity`, `.prefab`,
`.asset`, …) use `unityyamlmerge`.

## Folder conventions

```
Assets/
├── _Project/          ← ALL of our own content lives here
│   ├── Scripts/       Core, Player, Interactions, UI (+ planned World, Items, Time)
│   ├── Art/           Sprites, tile assets, tile palettes
│   ├── Scenes/        Inn.unity (the only scene so far)
│   ├── Input/         PlayerControls.inputactions + generated PlayerControls.cs
│   ├── Prefabs/       (empty for now)
│   ├── ScriptableObjects/  (empty — will hold item/NPC/dialogue data)
│   └── Audio/         (empty for now)
├── Settings/          ← URP config: Renderer2D, UniversalRP, volume profile (root, NOT _Project)
└── TextMesh Pro/      ← Unity-installed TMP resources
```

- **`Assets/_Project/`** holds everything we author. Keeping it isolated makes a
  future audit or migration clean.
- **Third-party assets** go in their own folder at the `Assets/` root (e.g.
  `Assets/Plugins/`, `Assets/CozyFantasyPack/`), never inside `_Project/`.
- **URP / render-pipeline assets stay at `Assets/Settings/`** (the Unity
  default location). `Assets/_Project/Settings/` exists but is intentionally
  empty.
- **Namespaces mirror script folders:** `Scripts/Core/` → `Innkeeper.Core`,
  `Scripts/Player/` → `Innkeeper.Player`, etc.

## Core architectural patterns

Each is one component or class; see [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
for detail.

- **`GridActor`** (`Assets/_Project/Scripts/Core/GridActor.cs`) — abstract base
  for anything that moves on the tile grid. Snaps to grid, steps one tile at a
  time, tracks `Facing`, exposes `CurrentTile` / `TileInFront`, and rejects moves
  into `blockingLayers`. Subclasses (e.g. `PlayerMovement`) call `TryStep(dir)`.
- **`Interactable` + `InteractionRegistry`**
  (`Assets/_Project/Scripts/Interactions/`) — interactables self-register in a
  **static `InteractionRegistry` keyed by tile coordinate (`Vector2Int`)** on
  `OnEnable` and unregister on `OnDisable` (one interactable per tile).
- **`PlayerInteraction`** (`Assets/_Project/Scripts/Player/PlayerInteraction.cs`)
  — each frame queries the registry for the tile in front of the player and
  exposes `CurrentTarget`; pressing **E** calls `CurrentTarget.OnInteract()`.
- **`InteractionPromptUI`** (`Assets/_Project/Scripts/UI/InteractionPromptUI.cs`)
  — world-space TextMeshPro prompt that reads `CurrentTarget` in `LateUpdate`
  and floats above the targeted object.
- **Camera** — `CM_PlayerFollow` (a Cinemachine 3.x `CinemachineCamera`) follows
  the player; the `Main Camera` carries the `CinemachineBrain`.

## Conventions & pitfalls

- **Use `rb.MovePosition` for kinematic `Rigidbody2D` movement, not
  `transform.position`.** Grid actors use a Kinematic `Rigidbody2D` so overlap
  checks stay accurate and movement doesn't tunnel through walls.
- **A script's filename must match its class name exactly** (Unity requirement
  for `MonoBehaviour`s).
- **Namespaces match folders** (`Scripts/<Folder>` → `Innkeeper.<Folder>`).
- **Tunable values are `[SerializeField] private`**, not `public` — exposed to
  the Inspector without widening the public API. Use `[Tooltip]` for non-obvious
  fields.
- **Don't hand-edit `PlayerControls.cs`** — it's generated from
  `PlayerControls.inputactions`. Re-toggle "Generate C# Class" on the asset to
  resync if it drifts.
- **Use `Assets/_Project/Input/PlayerControls.inputactions`** — there is also a
  leftover default `Assets/InputSystem_Actions.inputactions` from project
  creation; it is **not** the authoritative input asset.
- **Don't edit the `Main Camera` transform** — it's driven by Cinemachine at
  runtime. Change camera behavior on `CM_PlayerFollow`.
- **Movement-blocking objects use the `Walls` layer** (layer index 6), which
  `GridActor.blockingLayers` references.

### Unity 6 API notes

- Tilemap colliders use **Composite Operation: Merge**, not the older "Used By
  Composite" checkbox.
- Cinemachine 3.x uses **`CinemachineCamera`**, not `CinemachineVirtualCamera`.
- Build settings live under **File → Build Profiles**, not "Build Settings".

## Documentation

| Doc | What's in it |
|-----|--------------|
| [`docs/README.md`](docs/README.md) | Documentation index / where to start. |
| [`docs/GAME_DESIGN.md`](docs/GAME_DESIGN.md) | What the game *is* — concept, core loop, vibe. |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | How the code is organized and the patterns to follow. |
| [`docs/CONVENTIONS.md`](docs/CONVENTIONS.md) | Naming, folder, and code-style rules + pitfalls. |
| [`docs/DECISIONS.md`](docs/DECISIONS.md) | Architecture Decision Records (ADRs). |
| [`docs/ROADMAP.md`](docs/ROADMAP.md) | What's built and what's next, by phase. |
