# Innkeeper — Conventions

The authoritative reference for naming, folder placement, and code style, plus
the Unity-6-specific pitfalls that cost the most time. The repo-root
[`README.md`](../README.md) has the short version; this is the detail. For the
*reasoning* behind the bigger choices, see [DECISIONS.md](DECISIONS.md).

## Naming

- **Types use PascalCase**: `GridActor`, `InteractionRegistry`,
  `PlayerInteraction`.
- **A script's filename must match its class name exactly.** Unity requires this
  for `MonoBehaviour`s, and we apply it to all scripts. `GridActor.cs` contains
  `class GridActor` — nothing else.
- **Namespaces mirror script folders.** `Scripts/<Folder>/X.cs` →
  `namespace Innkeeper.<Folder>`. Current namespaces: `Innkeeper.Core`,
  `Innkeeper.Player`, `Innkeeper.Interactions`, `Innkeeper.UI`. Keep folder and
  namespace in sync as new folders are added — it makes auto-imports and
  cross-codebase grep predictable.
- **Assets use descriptive, prefixed names** following the patterns already in
  the project:
  - Tiles: `Tile_<Type>_<Material>` — e.g. `Tile_Floor_Wood`,
    `Tile_Floor_Stone`, `Tile_Wall_Stone`.
  - Cinemachine cameras: `CM_<Purpose>` — e.g. `CM_PlayerFollow`.
  - Canvases / UI roots: descriptive names — e.g. `WorldUI`.
  - Input: `PlayerControls.inputactions` (+ generated `PlayerControls.cs`).

## Folders

- **`Assets/_Project/` holds everything we author.** Keeping our content
  isolated from Unity-generated and third-party files keeps a future audit or
  migration clean.
  - `Scripts/` — split by domain: `Core/`, `Player/`, `Interactions/`, `UI/`,
    and the planned `World/`, `Items/`, `Time/`.
  - `Art/` — sprites, tile assets, tile palettes (e.g. `InnPalette.prefab`).
  - `Scenes/` — game scenes (currently `Inn.unity`).
  - `Input/` — the input actions asset and its generated C# class.
  - `Prefabs/`, `ScriptableObjects/`, `Audio/` — present, currently empty;
    item/NPC/dialogue data will live in `ScriptableObjects/`.
- **Third-party assets** go in their own folder at the **`Assets/` root** (e.g.
  `Assets/Plugins/`, `Assets/CozyFantasyPack/`) — never inside `_Project/`.
- **URP / render-pipeline config stays at `Assets/Settings/`** (the Unity
  default): `Renderer2D.asset`, `UniversalRP.asset`,
  `UniversalRenderPipelineGlobalSettings.asset`, `DefaultVolumeProfile.asset`.
  Note: `Assets/_Project/Settings/` exists but is intentionally empty — don't
  put pipeline assets there.

## Code style

- **Tunable values are `[SerializeField] private`, not `public`.** This keeps
  the public API minimal while exposing values to the Inspector for
  designer-style tweaking. Add `[Tooltip("…")]` to non-obvious fields (as
  `GridActor.blockingLayers` does).
- **Prefer composition over inheritance** — except where there's a genuine
  "is-a" with shared mechanics. `PlayerMovement is a GridActor` is justified;
  most other behavior should be small single-purpose components combined on a
  GameObject. Avoid deep inheritance trees.
- **Grid actors use a Kinematic `Rigidbody2D` and move via `rb.MovePosition`** —
  never `transform.position`. This keeps `Physics2D` overlap queries accurate
  and prevents tunneling through walls. (See
  [ADR-004](DECISIONS.md#adr-004-kinematic-rigidbody2d--moveposition-for-grid-movement).)
  Body Type = Kinematic is set on the prefab/Inspector; the script assumes it.
- **Use static registries for spatial lookups.** When many objects ask "what's
  at tile X?", a static `Dictionary<Vector2Int, …>` (like `InteractionRegistry`)
  beats `FindObjectsOfType` or per-frame physics queries. Future systems
  (tilled-soil registry, NPC schedule lookup) should follow the same pattern.
  (See [ADR-002](DECISIONS.md#adr-002-static-spatial-registry-keyed-by-tile-coordinate).)
- **UI is uGUI (Canvas + TextMeshPro), not UI Toolkit.** World-space and
  screen-space UI live on separate Canvases. (See
  [ADR-003](DECISIONS.md#adr-003-ugui-canvas--textmeshpro-over-ui-toolkit).)

## Pitfalls

These are the recurring traps — check here before debugging "why doesn't this
work."

- **`rb.MovePosition`, not `transform.position`**, for kinematic Rigidbody2D
  movement (see above).
- **Don't hand-edit `PlayerControls.cs`.** It's generated from
  `PlayerControls.inputactions`. If it falls out of sync, re-toggle "Generate C#
  Class" on the `.inputactions` asset to regenerate it.
- **Use `Assets/_Project/Input/PlayerControls.inputactions`.** A leftover default
  `Assets/InputSystem_Actions.inputactions` exists from project creation — it is
  **not** the authoritative input asset and should be ignored (or removed).
- **Don't edit the `Main Camera` transform.** Cinemachine drives it at runtime;
  your manual changes get overwritten. Change camera behavior on
  `CM_PlayerFollow` instead.
- **Movement blockers belong on the `Walls` layer** (layer index 6).
  `GridActor.blockingLayers` references it. New blocking things should be painted
  on the Walls tilemap or assigned to the Walls layer.

### Unity 6 specifics

- **Tilemap colliders use Composite Operation: Merge** — this replaces the old
  "Used By Composite" checkbox from earlier Unity versions.
- **Cinemachine 3.x uses the `CinemachineCamera` component**, not the pre-3.x
  `CinemachineVirtualCamera`. Tutorials referencing the old component are out of
  date.
- **Build settings are under File → Build Profiles**, not "Build Settings".
- **TextMeshPro ships inside `com.unity.ugui` (2.0.0)** — there's no standalone
  `com.unity.textmeshpro` package to add.
- **The legacy input API (`Input.GetKey`, `Input.GetAxis`) is not used.** All
  input goes through the New Input System and `PlayerControls`.
