<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# ✳️ FSM_Dungeon

[![Sith Policy](assets/sith-policy.svg)](https://github.com/TrentBest/FSM_Dungeon/actions/workflows/sith-policy.yml)

**An engine-independent domain for defining, composing, exploring, and simulating living worlds.**

FSM_Dungeon is being restarted from a clean foundation. Its former Unity-facing direction is not part of the new architecture. The repository will describe and implement the Dungeon domain itself; it will not be an engine plugin, a scene wrapper, or a second general-purpose workflow runtime.

## 1. Purpose

A dungeon is a structured, stateful world: places connect, entities inhabit those places, conditions change, and encounters emerge from the relationships and rules in that world.

FSM_Dungeon will provide the domain model and simulation foundations for creating those worlds. The first goal is not a renderer or a full game. It is a small, deterministic, testable world model that can later be presented through different experiences.

## 2. Architecture

The implementation is data-first and engine-agnostic.

- **Domain:** worlds, regions, rooms, passages, entities, and encounters.
- **State and behavior:** use [FSM_API](https://github.com/TrentBest/FSM_API) for state-machine primitives; do not duplicate its runtime.
- **Simulation:** domain behavior remains independently executable and testable.
- **Presentation:** visualization and interaction are adapters, not prerequisites.
- **Composition:** preserve clean boundaries for future integration with Physics, Ontology, Experiences, and FSM_COS. Add a dependency only when a concrete contract and use case justify it.

Unity is not a target dependency. No Unity editor, scene, assembly, package, or runtime assumption belongs in the new design.

## 3. Boundaries

FSM_Dungeon owns dungeon/world domain concepts and their simulation.

Foundry owns general Hub workflow execution. A Foundry integration, if one proves useful, must be a thin adapter around a real Hub-managed experience—not a requirement for Dungeon to run.

GUI is optional. The domain must remain usable without a GUI, renderer, game engine, or Foundry host.

Cross-repository integrations must be explicit. Work in this repository does not authorize direct modifications to another repository.

## 4. Development

The first vertical slice will establish:

1. A precise domain vocabulary and core invariants.
2. A minimal world representation with connected locations and entities.
3. Deterministic state transitions and simulation rules.
4. Automated tests that run without Unity or rendering infrastructure.
5. A small demonstration of the domain model before adding presentation.

Implementation decisions will be recorded as they become necessary. The design should earn complexity through working examples rather than speculate about every future feature.

Track the reboot in [issue #2](https://github.com/TrentBest/FSM_Dungeon/issues/2).

## 5. Project status

<badge>Restarting</badge> The repository currently contains its Workshop documentation and policy foundation. The new domain implementation has not yet been established.

The active development branch is `development`; `master` remains the release-aligned branch. Keep the repository to those two branches.

---

<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="240" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
