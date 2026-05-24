# Innkeeper — Architecture

This document describes how the codebase is organized, the responsibilities
of each system, and the patterns to follow when extending it.

For *why* specific choices were made, see [`DECISIONS.md`](DECISIONS.md).

## Tech stack

- **Unity** `6000.3.16f1` (Unity 6.3).
- **URP** `17.3.0`, configured with the **2D Renderer**
  (`Assets/Settings/Renderer2D.asset`).
- **New Input System** `1.19.0` (not the legacy `Input` API).
- **TextMeshPro** via `com.unity.ugui` `2.0.0` (TMP is bundled inside ugui in
  Unity 6 — there is no standalone `com.unity.textmeshpro` package).
- **Cinemachine** `3.1.6`.
- **2D feature set**: Tilemap (+ extras), 2D Animation, Aseprite, PSD Importer,
  SpriteShape.
- **Git + Git LFS** (LFS for images/audio/video/3D; Unity YAML via
  `unityyamlmerge`).

## Project structure

Game code lives under `Assets/_Project/`. Third-party assets, when added,
should go in their own folders at the `Assets/` root (e.g. `Assets/Plugins/`,
`Assets/CozyFantasyPack/`), keeping `_Project/` clean for a possible future
migration or audit.

```
Assets/_Project/
├── Scripts/
│   ├── Core/           Shared base classes, used by multiple systems
│   ├── Player/         Input-driven, player-only logic
│   ├── Interactions/   The interaction framework + concrete interactables
│   ├── UI/             Canvas-based prompts, HUD, menus
│   ├── World/          (planned) Tile / world / building logic
│   ├── Items/          (planned) Inventory and item data
│   ├── Time/           (planned) Day/night, clock, scheduling
│   └── ...
├── Scenes/             Currently: Inn.unity
├── Prefabs/            (empty for now)
├── ScriptableObjects/  (empty for now — will hold item, NPC, dialogue data)
├── Art/                Sprites, tile assets, tile palettes
├── Audio/              (empty for now)
├── Input/              PlayerControls.inputactions + generated C# class
└── Settings/           (empty placeholder — see note below)
```

> **Where the URP config actually lives:** the render-pipeline assets are at the
> **`Assets/Settings/`** root (Unity's default location), not under `_Project/`:
> `Renderer2D.asset`, `UniversalRP.asset`,
> `UniversalRenderPipelineGlobalSettings.asset`, and `DefaultVolumeProfile.asset`.
> The `Assets/_Project/Settings/` folder exists but is currently empty. Don't
> move pipeline assets into `_Project/`.

Each script folder roughly maps to a C# namespace: `Innkeeper.Core`,
`Innkeeper.Player`, `Innkeeper.Interactions`, `Innkeeper.UI`. Keep them
in sync as folders are added.

## Layers, tags, and physics

| Layer    | Index | Purpose                                       |
|----------|-------|-----------------------------------------------|
| Default  | 0     | Most GameObjects (player, interactables, UI). |
| Walls    | 6     | Tilemap layer that blocks `GridActor` movement. |

No custom tags are defined. `GridActor.blockingLayers` references the Walls
layer (used by `Physics2D.OverlapCircle` before committing a step). New
"physically blocking" things should either be painted on the Walls tilemap or
assigned to the Walls layer themselves.

## Core systems

### GridActor (`Scripts/Core/GridActor.cs`)

Abstract base class for any entity that moves on the tile grid.

Responsibilities:
- Snap to grid on spawn.
- Move smoothly between tile centers via `Rigidbody2D.MovePosition`
  (`Vector2.MoveTowards` toward the target each `FixedUpdate`).
- Enforce one-tile-at-a-time movement (no diagonals, no mid-step direction
  changes).
- Track `Facing` direction even when a move is blocked.
- Expose `CurrentTile`, `TileInFront`, `PositionInFront` for systems that
  need to reason about grid position.
- Reject moves that overlap a `blockingLayers` collider at the target tile.

Subclasses decide *when* and *where* to move by calling `TryStep(direction)`.
They do not implement movement themselves.

Concrete subclasses (current and planned):
- `PlayerMovement` — input-driven
- `StaffActor` (planned) — task-queue driven
- `GuestActor` (planned) — need-driven AI

### Input (`Scripts/Player/`, `Assets/_Project/Input/`)

Uses Unity's New Input System (not the legacy `Input.GetKey` API).

`PlayerControls.inputactions` defines the bindings; `PlayerControls.cs` is
auto-generated and must not be edited by hand. Re-toggle "Generate C# Class"
on the asset if it falls out of sync.

Current action map: `Player`. Actions: `Move` (Vector2 composite, WASD),
`Interact` (Button, E).

Scripts subscribe to actions by instantiating `new PlayerControls()`,
enabling the map in `OnEnable`, and disabling in `OnDisable`.

> **Note:** a leftover default `Assets/InputSystem_Actions.inputactions` exists
> from project creation. It is **not** the authoritative input asset — use
> `Assets/_Project/Input/PlayerControls.inputactions`.

### Interactions (`Scripts/Interactions/`)

Three components form the interaction framework:

1. **`Interactable`** (abstract MonoBehaviour) — base class for anything
   the player can interact with. Defines `Prompt` (string for UI),
   `OccupiedTile` (grid coordinate), `CanInteract` (override to gate by
   state), and `OnInteract()` (override to implement behavior).

2. **`InteractionRegistry`** (static class) — dictionary mapping tile
   coordinate (`Vector2Int`) to the `Interactable` on that tile. Interactables
   register themselves in `OnEnable` and unregister in `OnDisable`. Enforces one
   interactable per tile (last write wins, with a warning).

3. **`PlayerInteraction`** (on Player) — every frame, queries the
   registry for the tile in front of the player's `GridActor`. Exposes
   `CurrentTarget`. On Interact key press, calls `CurrentTarget.OnInteract()`.

Adding a new interactable type = create a subclass of `Interactable`, override
`OnInteract()`, attach to a GameObject at a tile center with a SpriteRenderer.
If you override `OnEnable`/`OnDisable`, call `base` so registration still runs.

Current concrete interactables:
- `BrokenChair` — placeholder; changes color from dull to warm on interact,
  then reports `CanInteract => false` so the prompt disappears.

### UI (`Scripts/UI/`)

Uses Unity's uGUI system (Canvas + RectTransform + Image + TextMeshPro),
not UI Toolkit. World-space and screen-space UI live on separate Canvases.

Current canvases:
- `WorldUI` (world-space) — holds the floating `InteractionPrompt`.

`InteractionPromptUI` reads `PlayerInteraction.CurrentTarget` in `LateUpdate`
(after the player updates its target for the frame) and positions a small
panel above the targeted Interactable.

### Camera (Cinemachine 3.1.6)

`Main Camera` carries a `Cinemachine Brain`; the actual logic lives on
`CM_PlayerFollow`, a `CinemachineCamera` that follows the `Player` GameObject.

The Main Camera's transform is driven by Cinemachine at runtime — do not
edit it directly. Change camera behavior on `CM_PlayerFollow` instead.

## Patterns and conventions

### Component composition over inheritance (mostly)

Inheritance is used where there's a clear "is-a" relationship and shared
mechanics (e.g. `PlayerMovement is a GridActor`). For everything else,
prefer composition: small components that do one thing, combined on
GameObjects. Avoid deep inheritance trees.

### Serialized private fields

Tunable values are `[SerializeField] private` (not `public`). This keeps
the public API minimal while exposing values to the Inspector for designer-
style tweaking. Use `[Tooltip]` to document non-obvious fields.

### Namespaces match folders

`Scripts/Core/X.cs` → `namespace Innkeeper.Core`. Helps IDE auto-imports and
makes grep across the codebase predictable.

### Kinematic Rigidbody2D for grid actors

All `GridActor` subclasses use `Rigidbody2D` in Kinematic mode. Movement
goes through `rb.MovePosition`, not `transform.position`. This keeps
physics queries (collision overlap checks) accurate and avoids tunneling.

Body Type = Kinematic is set on the prefab/Inspector — the movement code
assumes it but does not enforce it in `Awake`. (See
[ADR-004](DECISIONS.md#adr-004-kinematic-rigidbody2d--moveposition-for-grid-movement).)

### Static registries for spatial lookups

When many objects need to ask "what's at tile X?", a static dictionary
keyed by `Vector2Int` (like `InteractionRegistry`) is preferred over
`FindObjectsOfType` or per-frame physics queries. Future systems
(tilled-soil registry, NPC schedule lookup, etc.) should follow the
same pattern.

### Unity 6 API notes

- Tilemap colliders use `Composite Operation: Merge`, not the older
  "Used By Composite" checkbox.
- Cinemachine 3.x uses the `CinemachineCamera` component, not the
  pre-3.x `CinemachineVirtualCamera`.
- TextMeshPro ships inside `com.unity.ugui` — there's no separate
  `com.unity.textmeshpro` package to add.
- Build settings live under **File → Build Profiles**, not "Build Settings."

## Out of scope (current phase)

- Farming, crops, weather, seasons — planned for later phases, not now.
- Multiplayer / networking — not planned.
- Mobile or console builds — desktop-only until the vertical slice is done.
- Real art assets — placeholder shapes only. Art comes after core systems
  are stable.
