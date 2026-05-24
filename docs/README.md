# Innkeeper — Documentation

This folder is the project's written memory: what the game is, how the code is
organized, the rules for extending it, the reasoning behind major choices, and
the plan for what comes next.

> **Start with the repo-root [`README.md`](../README.md).** It's the fastest
> orientation — exact stack versions, folder layout, and the conventions/pitfalls
> an AI assistant or new contributor needs before touching code. The documents
> below go deeper.

## Contents

| Document | Read it when you want to… |
|----------|---------------------------|
| [GAME_DESIGN.md](GAME_DESIGN.md) | Understand the concept, core gameplay loop, and the cozy fantasy vibe — what Innkeeper *is*. |
| [ARCHITECTURE.md](ARCHITECTURE.md) | See how the codebase is structured, each system's responsibilities, and the patterns to follow when adding features. |
| [CONVENTIONS.md](CONVENTIONS.md) | Look up naming, folder, and code-style rules — plus the Unity 6 pitfalls that bite. |
| [DECISIONS.md](DECISIONS.md) | Find *why* a major choice was made (Architecture Decision Records). |
| [ROADMAP.md](ROADMAP.md) | Check what's built, what's next, and what's explicitly out of scope. |

## How these fit together

- **GAME_DESIGN** answers *what* and *why we're building it*.
- **ARCHITECTURE** and **CONVENTIONS** answer *how* — structure vs. style rules.
- **DECISIONS** records the *one-way doors*: choices worth remembering the
  reasoning for.
- **ROADMAP** tracks *progress* and *priority*.

When a notable architectural choice is made, add an ADR to
[DECISIONS.md](DECISIONS.md) and, if it changes structure, update
[ARCHITECTURE.md](ARCHITECTURE.md) to match.
