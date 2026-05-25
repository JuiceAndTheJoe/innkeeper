# Innkeeper — Decisions

This is the log of major architectural and design choices, recorded as
Architecture Decision Records (ADRs). Each ADR captures the context, the
decision, the reasoning, and the consequences — so the *why* survives even
after the *what* is obvious from the code.

**Format:** newest decisions are appended with the next ADR number. Status is
one of `Proposed`, `Accepted`, `Superseded by ADR-NNN`, or `Deprecated`. When a
phase ends with a notable architecture change, add an ADR here (see
[ROADMAP.md](ROADMAP.md#how-phases-are-tracked)).

---

## ADR-001: Use 3/4 top-down on orthogonal grid, not true isometric

**Date:** 2026-05-20
**Status:** Accepted

**Context:** Initial design called for isometric perspective like Don't Starve.

**Decision:** Use 3/4 top-down view on an orthogonal grid (Stardew-style).

**Why:**
- True isometric requires diamond tile math, multi-directional sprites,
  manual depth sorting — much higher complexity for solo dev
- Visual style "feels isometric" can be achieved via art direction
  (angled wall sprites, drop shadows) without the engineering cost
- All gameplay code (movement, interaction, inventory) translates if
  we ever migrate to true isometric later — only rendering changes

**Consequences:**
- Use Unity's default Rectangular Tilemap, not Isometric Tilemap
- Sprite sorting is by Y-axis, standard 2D

---

## ADR-002: Static spatial registry keyed by tile coordinate

**Date:** 2026-05-22
**Status:** Accepted

**Context:** Multiple systems need to answer "what is at tile X?" — interaction
targeting first, and later tilled-soil state, NPC schedules, and object
placement. The naive options are repeated `FindObjectsOfType` scans or per-frame
`Physics2D` overlap queries.

**Decision:** Maintain a **static dictionary keyed by `Vector2Int`**, populated
by objects that self-register. `InteractionRegistry` is the first instance:
`Interactable`s register in `OnEnable` and unregister in `OnDisable`.

**Why:**
- O(1) lookup by tile, with no per-frame allocation or scene scan
- Objects own their lifecycle — registration is local to the object
- A single place to enforce invariants (one `Interactable` per tile)
- A reusable pattern for the spatial-query systems coming in later phases

**Consequences:**
- Subclasses overriding `OnEnable`/`OnDisable` must call `base` or they silently
  fall out of the registry
- Tile collisions are resolved last-write-wins with a warning, not an exception
- Static state must be reset carefully across play-mode/domain reloads
- Future spatial systems should follow this pattern rather than inventing their
  own lookup

---

## ADR-003: uGUI (Canvas + TextMeshPro) over UI Toolkit

**Date:** 2026-05-23
**Status:** Accepted

**Context:** The first UI need is a prompt that floats in the world above a
targeted interactable; later needs include a screen-space HUD and menus.

**Decision:** Build UI with **uGUI** (Canvas + RectTransform + Image +
TextMeshPro). World-space and screen-space UI live on separate Canvases.

**Why:**
- uGUI has mature, first-class **world-space canvas** support — exactly what the
  floating prompt needs; UI Toolkit's runtime world-space support is far less
  mature
- TextMeshPro is battle-tested and ships with the project (via `com.unity.ugui`)
- More community answers and examples for a solo dev to lean on
- Performance is a non-issue at this project's UI scale

**Consequences:**
- TMP comes from `com.unity.ugui 2.0.0` in Unity 6 — no separate TMP package
- UI is laid out with Canvas/RectTransform, not UXML/USS
- Revisit only if a heavily data-driven UI emerges where UI Toolkit's binding
  model would clearly pay off

---

## ADR-004: Kinematic Rigidbody2D + MovePosition for grid movement

**Date:** 2026-05-21
**Status:** Accepted

**Context:** `GridActor`s move tile-to-tile and must respect wall collisions
without tunneling, while the same actors are queried against `Physics2D` to
decide whether a step is blocked.

**Decision:** Each `GridActor` carries a **Kinematic `Rigidbody2D`** and moves
via **`rb.MovePosition`** (`Vector2.MoveTowards` toward the tile center each
`FixedUpdate`). A `Physics2D.OverlapCircle` against the `Walls` layer gates each
step before it's committed.

**Why:**
- Writing `transform.position` bypasses the physics system, can desync collider
  positions, and allows tunneling; `MovePosition` integrates with physics
- Kinematic (not Dynamic) gives full control — no gravity, no forces, no
  unwanted collision response — while still participating in queries

**Consequences:**
- Body Type = Kinematic is configured on the prefab/Inspector; it is **not**
  asserted in code (a known gap — could be set in `GridActor.Awake` if drift
  becomes a problem)
- Movement is one tile at a time, resolved in `FixedUpdate`
- All future moving actors (`StaffActor`, `GuestActor`) inherit this approach
  via `GridActor`

---

## ADR-005: Defer farming to Phase 9 — the inn loop is the core

**Date:** 2026-05-20
**Status:** Accepted

**Context:** The project was originally framed with farming as its central
mechanic. On reflection, farming is a well-trodden mechanic, while the
inn-renovation-and-hosting loop is the distinctive hook.

**Decision:** Make the **inn-management loop** the core, and **defer farming to
Phase 9** as a supply-chain extension that feeds the inn's menu.

**Why:**
- The inn loop (repair → host guests → earn → expand → delegate) is the unproven,
  differentiating part and must be validated first
- Farming is lower-risk to add later and becomes a natural extension once the
  inn loop exists, rather than a large system with no proven loop around it
- Scope discipline: a solo dev gets to a playable vertical slice faster by not
  front-loading crops, soil, and seasons

**Consequences:**
- Early phases build inn systems (time, interactions, inventory, guests), not
  crops/soil/weather
- `Scripts/World`, `Scripts/Items`, and `Scripts/Time` are scaffolded for inn
  systems first
- The [ROADMAP](ROADMAP.md) orders farming as Phase 9, after the vertical slice

---

## ADR-006: TimeSystem — integer-minute clock as singleton MonoBehaviour

**Date:** 2026-05-24
**Status:** Accepted

**Context:** Phase 2 introduces in-game time. Many later systems will read or
react to it: sleep (skip to morning), HUD clock, day/night light tint, NPC
schedules (Phase 5), guest arrival windows (Phase 6), and crop growth (Phase 9).
Save/load (Phase 4) must restore the calendar exactly. The design has three
axes: how time is stored, how systems consume it, and where the system lives.

**Decision:**
- **Authoritative unit is `long totalMinutes`** since game start. Hour, day,
  day-of-week, season, and year are computed views, not stored fields.
- **Calendar shape is fixed:** 60 minutes/hour, 24 hours/day, 7 days/week,
  4 weeks/season (28 days), 4 seasons/year. Locked at ADR time — changing it
  later requires a save migrator.
- **Tick discretely**, not smoothly. Default 10 in-game minutes per tick; real
  seconds per tick is `[SerializeField]` on the runtime component for tuning.
  Driven by an `Update` accumulator, **not `FixedUpdate`** — game time is
  independent of physics timestep.
- **Hybrid consumption:** polling via `TimeSystem.Now` (a `GameTime` view
  struct) for the HUD and any "what time is it?" query; events
  (`OnTick`, `OnHourChanged`, `OnDayChanged`, `OnSeasonChanged`) for systems
  that react to transitions.
- **Time-skip API:** `AdvanceTo(targetMinutes)` fires each boundary event
  **exactly once**, regardless of how much time is skipped. Sleep uses this;
  intermediate hours don't spam subscribers.
- **Lives as a singleton `MonoBehaviour`** auto-instantiated via
  `[RuntimeInitializeOnLoadMethod]` with `DontDestroyOnLoad`. Accessed via
  `TimeSystem.Instance` and static convenience forwarders (`TimeSystem.Now`,
  `TimeSystem.OnDayChanged`, etc.).
- **Configuration in a `TimeConfig` ScriptableObject**
  (`Assets/_Project/ScriptableObjects/TimeConfig.asset`): minutes-per-tick,
  real-seconds-per-tick, starting date. State stays on the singleton; tunables
  stay on the SO.
- **Pause is a `TimeSystem.IsPaused` flag**, not `Time.timeScale`. Timescale
  also freezes physics/animation; we want those concerns separable.

**Why:**
- Integer minutes eliminate float drift over long sessions, give exact equality
  ("is it 06:00?"), and serialize as a single `long`. A struct-as-source-of-truth
  alternative would push carry/rollover logic onto every caller
- Discrete ticks match the discrete feel of grid movement and the cozy genre
  (Stardew, not RTS). Smooth interpolation is a render concern the clock UI can
  add later if needed
- The hybrid consumption model fits two genuinely different use cases: the HUD
  wants the current value every frame and ignores transitions; spawn/schedule
  systems want transitions and shouldn't poll. Forcing either pattern on the
  other creates friction
- `AdvanceTo` with single-fire events prevents an entire class of sleep bugs —
  guests spawning 12 times when the player sleeps 12 hours, etc. Cheap to
  design in now, expensive to retrofit
- Singleton MonoBehaviour is a deliberate deviation from ADR-002's "prefer
  static registry" pattern. ADR-002 fits *passive spatial lookup*; TimeSystem
  is *active* — it has an `Update` loop, configurable tunables, and a lifecycle.
  A static class would need a hidden driver MonoBehaviour anyway and would
  inherit ADR-002's domain-reload pitfall on a system the whole game depends on.
  The runtime-init pattern gives the ergonomics of "it just exists" without
  requiring placement in every scene

**Consequences:**
- Calendar shape is a one-way door. Old saves become unreadable if the shape
  changes without a migration step. The save format (Phase 4) must include a
  `configVersion` field from day one
- Save state for the clock is one number: `totalMinutes` (plus `configVersion`).
  Subscribers re-register themselves on load via `OnEnable`
- Event subscription discipline mirrors ADR-002's registry pattern: subscribe
  in `OnEnable`, unsubscribe in `OnDisable`, call `base` if overriding.
  Forgetting leaks subscribers across scene reloads
- `SetTime` is not publicly exposed. Public mutation is `AdvanceTo` (for
  sleep/skip) and internal `Tick`. Debug-only manipulation goes behind an
  editor-only panel — prevents Phase-5+ scripts from quietly nudging the clock
  and creating mystery bugs
- Tick rate is unknowable until playtest. Default of ~1 real second per 10
  in-game minutes (24h day in ~144 real seconds) is a starting point; expect
  to tune it once guests exist in Phase 6
- Two pause concepts coexist: `TimeSystem.IsPaused` (clock only) and
  `Time.timeScale = 0` (visual/physics freeze). The pause menu in Phase 2 will
  set both; cutscenes that should freeze the world but not the clock (or vice
  versa) can use only one
