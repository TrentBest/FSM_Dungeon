<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# ✳️ FSM_Dungeon

[![Sith Policy](assets/sith-policy.svg)](https://github.com/TrentBest/FSM_Dungeon/actions/workflows/sith-policy.yml)
[![Build](https://img.shields.io/github/actions/workflow/status/TrentBest/FSM_Dungeon/build.yml?branch=development&style=flat-square&logo=github)](https://github.com/TrentBest/FSM_Dungeon/actions/workflows/build.yml)
[![Pages](https://img.shields.io/github/actions/workflow/status/TrentBest/FSM_Dungeon/pages.yml?branch=development&style=flat-square&logo=github)](https://github.com/TrentBest/FSM_Dungeon/actions/workflows/pages.yml)

**An engine-independent domain and simulation for places that have a reason to exist.**

FSM_Dungeon is the Workshop's reusable underground-environment domain and the foundation for an original isometric action-RPG proof. The gameplay benchmark is the classic sanctuary → descent → exploration → danger → reward → return loop. Its defining responsibility is a living causal environment: spaces have origins and histories; materials, routes, resources, and hazards have context and consequences. Inhabitants obtain their ontology and behavior from SingularityUniversalRolePlayingSystem (SURPS).

## 1. Purpose

A dungeon is not a random collection of rooms and monsters. It is a place shaped by formation, use, change, and life.

The domain should be able to explain why a chamber exists, why a passage connects it, why a group occupies it, what that group is doing, and how the place changes over time. Natural caves, excavations, constructed facilities, biological structures, machine-bored passages, ruins, and unknown origins are extensible examples—not a closed list.

The NuGet package is the reusable .NET library. It is not the whole game application. The same domain is intended to support several original game experiences; see [Reusable Game Experiences](docs/GAME_EXPERIENCES.md), including the tabletop-inspired **Lanternward: The Delve** and future management project **Underkingdom**.

## 2. Architecture

- **FSM_Dungeon:** deterministic underground-environment generation, spatial topology, geological and constructed provenance, environmental state, resources, and hazards.
- **SURPS:** the external ontology and behavior system for inhabitants, including roles, capabilities, motivations, relationships, and decision-making.
- **FSM_API:** the Workshop state-machine substrate; reuse it where state-machine behavior is useful rather than duplicating its runtime.
- **AI understanding and authorship:** ProtocolAi supplies semantic identity, GrammarAi supplies structure, and CommandAi carries proposals for host-side validation and authorization.
- **Renderer:** observer-aware selection of computational detail and update fidelity under an explicit budget.
- **Visualization:** transforms selected representations into images and progressively richer visual output.
- **Dungeon MicroBundle:** `TheSingularityWorkshop.FSM_Dungeon.MicroBundle` exposes the underground-environment capability through the neutral MicroBundle contract; SURPS can compose it without taking ownership of Dungeon implementation.
- **MicroBundles:** compose the dungeon package and optional capabilities into an experience; NuGet remains the code-distribution model.
- **Hosts:** GitHub Pages proves the domain; WebApp can provide browser interaction; ServerApp can later host authoritative persistent worlds.

See [the architecture document](docs/ARCHITECTURE.md) for ownership boundaries and the staged plan, [Living Subterranean Worlds](docs/WORLD_SIMULATION.md) for the continuous-world, population, creator-catalogue, trap, and treasure direction, [Underground Worlds and Rendering](docs/UNDERGROUND_RENDERING.md) for subterranean lighting, shadows, and material detail, and the [Fantasy Library](docs/books/index.md) for original fantasy sourcebooks, peoples, bestiary entries, and StoryBuilder-ready campaign seeds. The [Underground Environments sourcebook](docs/books/underground-environments.md) defines the LOD 2 domain contract and has a [machine-readable descriptor](docs/books/underground-environments.json), [original cover artwork](docs/assets/sourcebooks/underground-environments-cover.svg), and [LOD 2 badge](docs/assets/sourcebooks/lod-2-domain-detail.svg).

## 3. Boundaries

FSM_Dungeon owns the underground environment and its environmental simulation—not the universal ontology or behavior of inhabitants. SURPS supplies those inhabitants and their behavior through an integration boundary. Dungeon does not require SURPS, Unity, a graphics API, a browser, WebApp, ServerApp, FSM_COS, or an AI model to run.

Renderer and Visualization are downstream capabilities. Hosts adapt input, persistence, networking, and presentation. AI can inspect and propose changes, but model output never directly mutates authoritative world state.

NuGet and MicroBundles are complementary: NuGet distributes the reusable library; a MicroBundle composes it with content and optional Workshop capabilities into an experience.

The Diablo reference is a gameplay/systems benchmark only. The separate [game experience briefs](docs/GAME_EXPERIENCES.md) establish additional reuseability targets. All Workshop code, characters, worldbuilding, maps, writing, art, and audio are original; third-party protected expression and assets are not copied.

## 4. Development

The first deterministic domain slice now generates two contrasting places through the same API:

- **The Underworks of Singularity City:** dense municipal service topology, stormwater flow, water level, partially blocked gates, air quality, maintenance access, and groups with distinct reasons to occupy the network.
- **The Catacombs Beneath the First Lantern:** older constructed excavation, a sealed spring route, burial strata, later occupation, and evidence of recent disturbance.
- extensible origin, host-world anchor, and environmental condition records—no closed sewer/catacomb type enum;
- seeded, connected chamber and passage generation with exact authored additional-passage density;
- a first deterministic hydraulic solver: head-driven transfers, passage capacity, blockage, inflow, and explicit overflow accounting;
- a six-tick sewer hydraulic proof serialized as `hydraulic.json` alongside the generated worlds;
- engine-independent 3D spatial regions, chasm/bridge/ladder features, and immutable traversal-state snapshots;
- an interactive Pages cavern proof where destroying the bridge closes only that crossing while the ledges, chasm, and vertical ladder route remain;
- chamber formation, history, and purpose provenance;
- structural validation and automated tests;
- a console proof that serializes both generated worlds for the browser.

The Pages inspector now has a world selector and displays chamber provenance, provisional legacy inhabitant group descriptions, and environmental conditions from C#-generated data. Those group motivation/activity fields are transitional fixture data; SURPS is the target owner of inhabitant ontology and behavior. A separate cavern proof now visualizes 3D region facts and lets the viewer destroy/restore a bridge to see traversal change without erasing spatial adjacency. Environmental condition records remain authored inputs; the hydraulic solver is a separate evolving state model in this first slice. It is a deliberately simple, first-order model—not a calibrated fluid-dynamics or flood-prediction system. Pages deployment still needs the `github-pages` environment to allow `development` (see [issue #3](https://github.com/TrentBest/FSM_Dungeon/issues/3)).

### Mountain fortress integration target

The [Dwarven Mountain Fortress scenario](docs/DWARVEN_MOUNTAIN_FORTRESS.md) is the first large-scale cross-domain proving world. Physics owns the exterior terrain and future 3D solid mountain volume; FSM_Dungeon owns the underground voids, constructed spaces, environmental topology, and history; SURPS owns dwarven entities and society. The existing Physics height field can provide exterior relief but cannot represent internal caves or a complete mountain cutaway. The scenario document defines the handoff, districts, Pages exploration stages, and evidence gates without claiming the fortress is already generated.

## 5. Project status

**Status: Initial domain foundation implemented; first playable loop not yet complete.** The current generator proves deterministic structure and causal metadata, not yet autonomous inhabitant simulation, combat, loot, or high-definition rendering.

- [GitHub Pages proof](https://trentbest.github.io/FSM_Dungeon/) — deployment is pending the environment rule noted above.
- [Architecture and integration plan](docs/ARCHITECTURE.md)
- [Open development issues](https://github.com/TrentBest/FSM_Dungeon/issues)
- [Build workflow](https://github.com/TrentBest/FSM_Dungeon/actions/workflows/build.yml)

The active development branch is development; master remains the release-aligned branch. Keep exactly those two branches.

---

<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="240" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
