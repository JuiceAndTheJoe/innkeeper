# Innkeeper — Roadmap

This roadmap describes phases of development, not fixed timelines. Each
phase ends in a playable state, even if rough. Phases may overlap in
practice; the order describes priority and dependency, not a strict
sequence.

## Game concept

Innkeeper is a cozy fantasy game about renovating and running a run-down
inn. The player walks around a 3/4 top-down world, repairs and decorates
their inn, hires staff, serves guests with needs (food, drink, lodging,
atmosphere), and gradually expands the property. A small surrounding
village provides shops, NPCs, and exploration. A farming system feeds
into the inn as a supply chain in a later phase.

The game is single-player, desktop-first, and built solo with AI assistance.
A publishable vertical slice is the medium-term goal; a finished game is
a long-term one.

## Current status

**Phase 1 — Foundation** is complete. The project has version control,
input handling, a base class for grid-based actors, a player that walks
around a tile-mapped inn interior, walls that block movement, a follow
camera, and a generic interaction framework with one concrete example
(a repairable chair) and a world-space UI prompt.

The code is small but architecturally honest — every system is structured
to grow without needing a rewrite.

## Phases

### Phase 1: Foundation ✅

Goal: a player can walk around a room and interact with one object.

- [x] Unity 6 LTS project with URP 2D, Git, LFS, folder structure
- [x] New Input System with WASD + E bindings
- [x] `GridActor` base class with grid movement and facing
- [x] `PlayerMovement` subclass driven by input
- [x] Tilemap inn interior with wall collision
- [x] Cinemachine follow camera
- [x] `Interactable` / `InteractionRegistry` / `PlayerInteraction` framework
- [x] `BrokenChair` example interactable
- [x] World-space "Press E to Repair" prompt UI

### Phase 2: Time and core loop

Goal: time passes; the player can sleep, days advance, and a few
interactable types exist.

- [ ] `TimeSystem` (in-game clock, hours, days, week, season)
- [ ] HUD clock display (screen-space UI Canvas)
- [ ] `Bed` interactable — interact to sleep until morning, advances day
- [ ] `DirtyFloor` interactable — interact to clean, restores to clean tile
- [ ] `Fireplace` interactable — light/extinguish, affects room state
- [ ] Time-of-day visual tint (URP 2D global light, simple day/night curve)
- [ ] Pause menu (Esc to open)

End-of-phase test: walk around, repair the chair, clean a floor, light
the fireplace, sleep, wake the next day, see the clock advance.

### Phase 3: Inventory and money

Goal: the player can hold items, money exists, and repairs cost something.

- [ ] `Item` ScriptableObject (id, display name, icon, stack size)
- [ ] `Inventory` component on Player (slots, add/remove/query)
- [ ] Inventory hotbar UI (screen-space)
- [ ] `Wallet` component on Player (gold)
- [ ] Repair / clean actions consume materials and/or gold
- [ ] First "drop" interactable — pick up a `Wood` item from the floor
- [ ] First container — chest that stores items between sessions

End-of-phase test: pick up wood, walk to broken chair, repair it (consumes
wood), see gold balance and inventory change.

### Phase 4: Save and load

Goal: progress persists across sessions.

- [ ] JSON-based save format (one file per slot in `Application.persistentDataPath`)
- [ ] Save manager: serialize player position, facing, inventory, wallet,
      current day/time, interactable states (which chairs repaired, floors
      clean, etc.)
- [ ] Load on game start, save on sleep / quit
- [ ] Manual save/load menu (optional this phase)

End-of-phase test: quit mid-day, reload, find everything exactly where it
was including inventory and repaired objects.

### Phase 5: NPCs and staff

Goal: other characters exist in the world.

- [ ] `NPCActor` (GridActor subclass) with simple patrol or wait behavior
- [ ] `StaffActor` — a hireable NPC that performs tasks
- [ ] Task queue: player assigns tasks ("clean this floor") or staff picks
      up nearby tasks autonomously
- [ ] Dialogue: ScriptableObject-driven, branching text shown in screen UI
- [ ] First hireable staff role (e.g. cleaner)
- [ ] First village NPC with a one-line dialogue

End-of-phase test: hire a cleaner, watch them autonomously clean a dirty
floor; talk to a villager.

### Phase 6: Guests and the inn loop

Goal: guests arrive, have needs, and pay for service.

- [ ] `GuestActor` (NPCActor subclass) with need-driven AI
- [ ] Guest spawning: arrive in evening, leave in morning
- [ ] Guest needs: bed (sleep), food, drink, atmosphere score
- [ ] Inn state: cleanliness, decoration score, capacity
- [ ] Income from satisfied guests
- [ ] First "menu" interactable — assign a guest to a bed

End-of-phase test: a guest walks into the inn at dusk, takes an available
bed, sleeps, leaves in the morning, and pays for the stay.

### Phase 7: Village expansion

Goal: the player can leave the inn and visit the village.

- [ ] Scene transitions (inn → outside → village)
- [ ] Persistent state across scenes
- [ ] First shop — buy decorations or supplies
- [ ] Time advances during travel
- [ ] World-map or signposted travel between locations

End-of-phase test: leave the inn, walk to the village, buy something, return.

### Phase 8: Vertical slice polish

Goal: a 30-minute polished demo.

- [ ] One full in-game week of content
- [ ] Replace placeholder art with consistent style (purchased pack or
      commissioned)
- [ ] Audio: ambient music, footstep sounds, interaction feedback
- [ ] Tutorial / onboarding flow
- [ ] Settings menu (volume, resolution, key rebinding)
- [ ] Title screen

This is the milestone where Innkeeper becomes shareable — a playable
build to give friends, post on itch.io, or use as a portfolio piece.

### Phase 9: Farming as a supply chain

Goal: the player can grow ingredients used in the inn.

This is the phase that was originally framed as the project's core but
has been deliberately deferred. By this point the inn loop is proven,
and farming becomes a natural extension rather than the focus.

- [ ] Outdoor farm scene (separate scene, transition from inn)
- [ ] Crop ScriptableObjects (seed → growth stages → harvested item)
- [ ] Soil tile states (untilled / tilled / watered / planted)
- [ ] Watering can interactable
- [ ] Seasonal growth rules
- [ ] Crops feed into inn menu (cook drinks/dishes from harvested items)

### Phase 10+: Beyond the vertical slice

Open-ended. Possibilities, in no particular order:
- Multiple inns to manage
- More staff roles (cook, bartender, musician)
- Light magic: enchanted furniture, magical guests, mystical events
- Seasonal festivals and special guests
- Expanded village with multiple shops and NPCs with schedules
- Achievements, content goals, an ending or open-ended endgame
- Mac/Linux builds
- Eventual Steam release if scope and quality justify it

## Anti-roadmap

Things explicitly not planned, to keep scope honest:

- **Multiplayer / co-op** — out of scope.
- **Procedural generation** — the inn and village are hand-designed.
- **Combat** — the game is non-violent. No enemies, no weapons.
- **Mobile / console ports** — not until after a stable desktop release.
- **3D** — Innkeeper is and will remain 2D.

## How phases are tracked

Each phase ends with a Git tag (`phase-1-foundation`, `phase-2-time`, etc.)
and a short retrospective written into [`DECISIONS.md`](DECISIONS.md) as an ADR
if any notable architecture changes happened during the phase. This makes it
easy to revert or audit major milestones later.
