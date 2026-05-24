# Innkeeper — Game Design

This document describes what Innkeeper *is*: its concept, the core gameplay
loop, and the tone it's reaching for. It's the "north star" the systems serve.
For the build order, see [ROADMAP.md](ROADMAP.md); for how it's implemented, see
[ARCHITECTURE.md](ARCHITECTURE.md).

## Concept

Innkeeper is a cozy fantasy game about **renovating and running a run-down inn**.
The player inherits a neglected inn, walks around a 3/4 top-down world, repairs
and decorates the building, hires staff, and serves guests who arrive with needs
— food, drink, lodging, and a pleasant atmosphere. A small surrounding village
provides shops, NPCs, and exploration. Over time the player expands the property
and, in a later phase, grows ingredients on a farm that feeds the inn as a
supply chain.

The game is **single-player, desktop-first, and non-violent**, built solo with
AI assistance. The medium-term goal is a publishable vertical slice; a finished
game is the long-term one.

## Core loop

The minute-to-minute and day-to-day loops reinforce each other:

1. **Explore & assess** — walk the inn and village, find what's broken, dirty,
   or missing.
2. **Repair & decorate** — fix furniture (e.g. the broken chair), clean floors,
   light the fireplace, and improve the room's atmosphere. Actions cost
   materials and/or gold.
3. **Serve guests** — guests arrive (typically in the evening), take beds, want
   food and drink, and rate the atmosphere. Satisfying them earns income.
4. **Earn & reinvest** — spend earnings on supplies, decorations, and staff.
5. **Delegate & expand** — hire staff to handle tasks autonomously (cleaning,
   later cooking/serving), and grow the property and the business.
6. **(Later) Supply** — a farm grows ingredients that become menu items,
   closing the loop between land and inn.

A day/night and calendar rhythm frames the loop: do daytime prep, host guests
through the evening, sleep to advance to the next day.

## Vibe & pillars

The feel is **calm, warm, and rewarding** — progress without pressure. There's
no fail state to dread; the satisfaction comes from a space visibly improving
under the player's care.

**Design pillars** (decisions should serve these):

- **Cozy over stressful.** No combat, no harsh timers, no punishing economy.
  Tension, if any, is gentle and optional.
- **Build & nurture.** The central verbs are *fix, improve, decorate, host,
  and grow* — turning a ruin into a beloved place.
- **Honest small scope.** Single-player, desktop, 2D. Systems are built to grow
  without rewrites, and ambitious features (farming, magic) are deferred until
  the inn loop is proven.

The "fantasy" register stays light: an inn in a gentle fantasy village, with
room for enchanted furniture, magical guests, and seasonal festivals later. The
isometric-feeling warmth is achieved through **art direction** — angled wall
sprites, drop shadows, and a day/night light tint — rather than true isometric
rendering (see [ADR-001](DECISIONS.md#adr-001-use-34-top-down-on-orthogonal-grid-not-true-isometric)).

## Player fantasy & setting

The player is the new innkeeper of a once-loved, now run-down inn. The fantasy
is **taking something neglected and making it thrive** — and being part of a
small community. The world expands outward in layers: the inn interior first,
then its surroundings, then a walkable village with shops and named NPCs, and
eventually an outdoor farm.

## Non-goals

To keep scope honest, the following are explicitly **not** part of Innkeeper
(mirrored in the [ROADMAP anti-roadmap](ROADMAP.md#anti-roadmap)):

- **Combat** — non-violent; no enemies, no weapons.
- **Multiplayer / co-op** — single-player only.
- **Procedural generation** — the inn and village are hand-designed.
- **3D** — Innkeeper is and remains 2D.
- **Mobile / console** — desktop-first until a stable release.

These boundaries are as much a part of the design as the features.
