# FSM_Dungeon Architecture

## Purpose

FSM_Dungeon is an engine-independent, deterministic world and dungeon simulation library. It is also the foundation for an original isometric action-RPG proof inspired by the *kind* of experience established by the original Diablo: sanctuary, descent, exploration, real-time danger, rewards, growth, and return.

This is a systems and gameplay benchmark, not a content remake. All code, worldbuilding, characters, maps, art, audio, names, and writing created for the Workshop must be original.

The defining Workshop difference is environmental causality: an underground place is not a bag of rooms. Its formation, history, spatial structure, materials, resources, and hazards should explain one another. The entities that inhabit it receive their ontology and behavioral model from SURPS; FSM_Dungeon describes the environment they inhabit and the consequences of their presence and actions.

## Product boundaries

### NuGet library: the reusable engine

The NuGet package is the stable, host-independent underground-environment domain and simulation surface. FSM_Dungeon owns:

- underground place identity, registration references, and environmental discoverability facts;
- extensible origin, formation, geological, and constructed-history descriptions;
- three-dimensional spatial topology, regions, passages, barriers, and traversability facts;
- underground materials-in-context, deposits, environmental conditions, and causal provenance;
- environmental state and underground processes that are explicitly part of this domain;
- the spatial presence and location facts needed to situate external entities in the environment;
- queries and validated environment commands.

FSM_Dungeon does **not** define the universal ontology of inhabitants, their roles, minds, motivations, social structures, goals, or general behavior. Those belong to **SingularityUniversalRolePlayingSystem (SURPS)**. Dungeon may record that an entity occupies a location, is present in a region, or has caused an environmental change; it does not define what that entity is or why it chose to act.

The core must not require Unity, a graphics API, a browser, WebApp, ServerApp, FSM_COS, or SURPS to exist. SURPS integration is an external composition boundary, not a reason to make the underground environment depend on a particular role-playing implementation.

The first target should favor broad .NET compatibility and a small dependency surface. Runtime-specific or Workshop-specific adapters can be separate packages when their concrete contracts are understood.

### MicroBundle: the composed experience

NuGet distributes reusable code. A MicroBundle composes capabilities and content into an installable experience.

The first domain entry point now exists as `TheSingularityWorkshop.FSM_Dungeon.MicroBundle`. It exposes the provider `fsm-dungeon.underground-environment` and has a stable identity shared with SURPS. Its package references the authoritative FSM_Dungeon library; it does not move environment ownership into the composition layer.

SURPS composes this capability through `SurpsRulebookDungeonsAndUnderground`, which declares the Dungeon MicroBundle as a required child capability and presents it through the initial `ISurpsRulebook` marker. The SURPS wrapper does not copy Dungeon source or introduce a dependency from the Dungeon domain back to SURPS. The composition host resolves the child MicroBundle by its stable ID.

The current bundle is an executable composition entry point, not yet a complete playable experience. It establishes identity, provider, and dependency direction. World definitions, assets, optional capabilities, and host-specific experience configuration can be added as those contracts mature.

A dungeon experience bundle may declare:

- the dungeon domain package/version;
- world and generation definitions;
- optional AI vocabularies and grammars;
- optional Renderer and Visualization capabilities;
- host-specific adapters;
- assets and experience metadata.

The MicroBundle is not a replacement for NuGet, and the NuGet package is not itself the whole experience. Keep the manifest format aligned with the Workshop's existing composition rules rather than inventing a competing bundle system here.

### Hosts

The same domain should be usable from several hosts:

| Host | Responsibility |
| --- | --- |
| Local test/console harness | deterministic simulation, automated tests, fast iteration |
| GitHub Pages | public, low-friction proof and interactive inspection |
| WebApp | browser-based interactive experience |
| ServerApp | authoritative persistent worlds, multiplayer/session services |
| Desktop/VR host | richer input, audio, and presentation |

Hosts adapt input, persistence, networking, and presentation. They do not own dungeon rules.

### Multiple games, one environment domain

FSM_Dungeon must not become coupled to one game's turn loop, room catalogue, economy, camera, progression, or player role. [Reusable Game Experiences](GAME_EXPERIENCES.md) defines two independent consumers: **Lanternward: The Delve**, an original tabletop-inspired tactical adventure, and **Underkingdom**, a future asymmetric underground-management simulation. Both reference the same stable environment capability while keeping their own rules, content manifests, UI, and progression outside the domain assembly. Their manifests are validated as content contracts, not compiled into the engine.

## Core model: causes before templates

A generated place should retain enough provenance to answer:

- Why does this dungeon exist?
- What formed each region, chamber, passage, or barrier?
- What was the place used for?
- What changed it over time?
- Why are these inhabitants here?
- What do they want, protect, fear, consume, maintain, or travel toward?
- Why is a hazard or resource present?

Origins and formation mechanisms must be extensible. Natural caves, excavations, constructed facilities, biological structures, machine-bored passages, ruins, alien structures, and speculative/time-displaced formations are examples—not a closed enum.

Generation should be deterministic for a supplied seed and definition. A generated result should retain its inputs and causal records so it can be reproduced, inspected, compared, and explained. Connectivity is guaranteed by a minimal spanning backbone, while `AdditionalPassageCount` explicitly controls how many extra routes are generated. This separates the invariant (all chambers are reachable) from the authored topology (sparse, dense, or anything between) without encoding sewer, catacomb, or other themes as closed types.

## Simulation and state

The domain exposes explicit operations such as generation, inspection, command application, and advancing simulation. It must not require a renderer to keep the world meaningful.

FSM_API is the Workshop's state-machine substrate. Dungeon should reuse it where state-machine behavior is genuinely useful rather than implementing a competing FSM. At the same time, the public domain model should not leak FSM handles or require a particular host update loop. Integration should be isolated behind a focused adapter or internal orchestration boundary once the API usage is proven.

Environmental simulation should support multiple levels of fidelity:

- immediate: detailed local environmental interactions;
- local: nearby environmental state and route consequences;
- regional: coarse environmental processes and resource changes;
- distant: summarized or event-driven environmental state.

Actor cognition, roles, social organization, intentions, decision-making, and behavioral fidelity are SURPS responsibilities. Movement execution, perception, communication, and physical interaction may be supplied by other capabilities. FSM_Dungeon exposes the environmental facts and validated effects those systems need; it does not silently implement their behavior. Environmental simulation policy and rendering policy are related but not identical.

## AI: understanding and authoring through contracts

AI is an adapter and participant, not the owner of world truth.

The intended Workshop chain is:

- **SURPS — ontology and behavior:** defines what inhabitants are, their roles, relationships, capabilities, motivations, social structures, and behavioral rules.
- **ProtocolAi — WHAT:** application-owned identities for dungeon entities, concepts, observations, and commands.
- **GrammarAi — HOW:** valid structures and relationships for expressing queries, explanations, plans, and proposals.
- **CommandAi — DO:** typed command proposals that a host validates and applies under its own authority rules.

This creates two complementary flows:

1. **Understand:** expose bounded observations and provenance; encode known identities; structure the explanation; let an AI reason over that representation.
2. **Author:** let an AI propose a world definition or change; validate references, structure, constraints, and cost; preview the result; then let the host decide whether to apply it.

No model output directly mutates authoritative state. Unknown values remain explicit proposals for the host to resolve. Permission, validation, cost limits, and approval stay outside the model.

AI-generated worlds must pass the same deterministic validation and invariant checks as procedurally generated worlds.

## Renderer and Visualization

Renderer owns observer-aware computational representation: what needs to be evaluated, at what fidelity, update rate, and semantic depth for a particular observer and budget.

Dungeon should provide structured world facts and observation queries; it should not duplicate Renderer scheduling or Event Horizon policy.

Visualization turns selected representation into pixels and richer sensory output. It is a downstream presentation capability, not a dependency of the dungeon domain.

The intended pipeline is:

```text
Dungeon world + provenance
          |
          v
Observer / intent / budget
          |
          v
Renderer representation policy
          |
          v
Selected world representation
          |
          v
Visualization / host adapter
          |
          v
Browser, desktop, VR, or other display
```

A low-cost representation might show room outlines, labels, faction presence, and routes. A higher-cost representation can add geometry, materials, lighting, animation, and effects. The world remains the same; only the observer's representation budget changes.

## First playable proof

The initial vertical slice should prove the whole causal loop without attempting the entire game:

1. A small original sanctuary/town with a discoverable entrance.
2. A deterministic multi-room dungeon generated from a recorded origin, purpose, history, and seed.
3. At least two SURPS-defined populations or roles situated in the environment, with Dungeon proving their spatial presence and environmental interactions.
4. A controllable player with exploration, real-time combat, health, and a death/return loop.
5. Original loot and persistent character progression.
6. A complete descent, encounter, reward, and return cycle.
7. Tests for determinism, connectivity, provenance, inhabitant purpose, command validation, and simulation invariants.
8. A Pages proof that can inspect the generated world and explain why its parts exist.

The first proof may use a simple 2D/isometric presentation. It must not block the domain on the eventual high-definition renderer.

## Delivery sequence

1. Establish domain vocabulary, invariants, and deterministic generation.
2. Build the smallest runnable simulation and test harness.
3. Publish an inspectable Pages proof using real generated data.
4. Add explicit query/command contracts and AI vocabulary/grammar.
5. Add Renderer observation-budget integration.
6. Add Visualization and a richer browser presentation.
7. Add WebApp and ServerApp adapters when the local proof demonstrates the required contracts.
8. Package and validate the NuGet library; compose the MicroBundle experience separately.

## Non-goals and guardrails

- No copied Diablo code, art, audio, maps, dialogue, characters, or other protected expression.
- No mandatory game engine or graphics API in the domain package.
- No mandatory WebApp or ServerApp dependency.
- No model output directly applying privileged commands.
- No duplicate FSM runtime, rendering scheduler, or MicroBundle host.
- No claim of production rendering or multiplayer before those capabilities are implemented and tested.
- No package publication, release, merge, or deployment to a production host without the required explicit authorization.

## Proof principle

**The underground environment should make sense when no observer is present.** Its spaces have histories, its environmental state has causes, and its visible state is an affordable representation of a deeper world. Inhabitants bring their ontology and behavior from SURPS.

## Host placement and environmental conditions

A dungeon may be registered beneath, within, or adjacent to a larger host world without taking a dependency on that world. `DungeonSiteReference` carries opaque host identity, host kind, and anchor identity. For example, a sewer network can refer to a district in Singularity City; the city decides whether that anchor exists, who can discover it, and how an entrance is exposed. FSM_Dungeon remains responsible for the dungeon's own structure and simulation.

`DungeonCondition` records extensible environmental facts as an open kind, value, optional unit, and explanation. This is deliberately not a closed list of dungeon themes or condition enums. A sewer definition can describe hydraulic flow, water level, gate obstruction, air quality, and maintenance access. A catacomb can describe burial strata, sealed routes, spring controls, and recent disturbance. A future biological, mechanical, or alien place can introduce its own condition kinds without changing the domain assembly.

These records are authored world inputs at this stage, not yet a live fluid or ecology solver. The next simulation layer must define which conditions evolve, what causes those changes, and which events or commands are permitted to alter them.

The proof sample now generates two contrasting environments from the same API: the city's connected underworks and the older catacombs beneath the First Lantern. Both retain host anchors, provenance, environmental conditions, and topology. The sample's current group descriptions are provisional legacy fixture data, not the target ontology or behavior model. In the target architecture, SURPS defines inhabitants and their behavior; Dungeon records their placement in and effects upon the underground environment.


## First causal simulation slice: hydraulics

The hydraulic model is deliberately separate from authored `DungeonCondition` metadata. A `HydraulicNetworkDefinition` declares chamber geometry and initial water, plus hydraulic properties for selected existing passage pairs. The deterministic `HydraulicSimulation.Step(durationSeconds)` applies inflow, computes water-surface head, proposes flow down the head gradient, limits transfer by passage capacity and blockage, preserves water below each passage invert, and scales simultaneous outflows to prevent a chamber from sending more water than it contains. The world-bound constructor rejects hydraulic edges that do not exist in the dungeon's passage graph.

Water exceeding a chamber's declared capacity is counted as cumulative overflow rather than silently discarded. Snapshots expose per-chamber water volume and surface elevation, per-step transfers, elapsed time, tick, and cumulative overflow.

This is an intentionally transparent first-order model for validating causal state evolution and conservation accounting. It is not yet a calibrated fluid solver: it does not model momentum, pressure transients, turbulence, pipe cross-sections, rainfall fields, or structural failure. Those require additional domain inputs and validation before the model is suitable for prediction. The Pages sample runs six ten-second ticks over a small sewer network and writes `docs/data/hydraulic.json`.


## Living subterranean worlds

The next domain direction is a continuous spatial world rather than a stack of numbered floors. Floors are useful presentation and streaming partitions, but the authoritative model must account for elevation, spatial extent, and many forms of traversal: stairs, shafts, lifts, bridges, chasms, drops, crawlspaces, hidden routes, and passages that can become blocked or destroyed. Spatial adjacency, traversal capability, and perception are separate concepts. A chasm is a real spatial feature; a bridge or other crossing is a distinct feature whose state can change.

Inhabitants must become populations with homes, territory, needs, capabilities, resources, relationships, and intentions. They travel to forage, hunt, scout, patrol, trade, raid, migrate, reinforce, and return. The player can encounter scouts and expeditions far from a population's stronghold, then discover a much larger society in its territory. Dwarven cities, goblin warrens, orcish strongholds, and other subterranean ecologies should emerge from the same extensible settlement and population capabilities, not from hard-coded species or dungeon modes.

The creator supplies an allowed catalogue of content—creatures, populations, traps, treasure, mechanisms, props, resources, architectural elements, and other original assets. The world generator determines contextual placement and relationships from the authored origin, history, purpose, spatial structure, and population behavior. Placement should be explainable: who owns an item, who placed a trap, what it protects, and why a cache is where it is. This replaces arbitrary room-by-room scattering with causal composition.

Simulation fidelity must scale with observer proximity and significance: detailed local actors, regional group movement, and event-driven distant populations should remain parts of one authoritative world. Meaningful changes should be recorded as causal events so the system can explain sightings, travel, ownership, territory changes, route closures, and consequences.

The detailed design and staged proof sequence are in [Living Subterranean Worlds](WORLD_SIMULATION.md). This is a design target, not a claim that continuous 3D topology, autonomous populations, creator catalogues, or trap/treasure placement are already implemented.


## Subterranean observation and rendering

FSM_Dungeon treats the underground as a distinct world-rendering regime, not an outdoor scene with its daylight reduced. Natural caves, caverns, burrows, mines, tunnels, excavations, catacombs, sewers, foundations, and subterranean settlements all fit beneath the domain umbrella. The defining conditions are enclosure, local illumination, occlusion, and close-range material detail.

Underground illumination may come from flames, carried lamps, machinery, bioluminescence, unusual emitters, or daylight admitted and redirected through shafts, cracks, vents, and reflective surfaces. There is no assumed global daylight contribution. Relevant local light sources must contribute to the illumination and shadowing of the space; flickering flames must produce coherent time-varying light and shadows, not unrelated random brightness changes.

Material detail is evidence of formation and use: geological strata, roots, fractures, erosion, excavation scars, machining patterns, masonry, irregular bricks, mortar, repairs, soot, deposits, and wear should agree with the place's history and construction. A natural cave, hand-dug mine, machine-bored tunnel, and finished brick sewer should have visibly distinct surface character.

Keep illumination, geometric visibility, observer perception, and rendering separate. FSM_Dungeon owns semantic material, formation, and light-source facts; Renderer owns observer-aware representation and budget; Visualization and host adapters produce pixels. This is a target architecture, not a claim that dynamic multi-light shadowing or high-detail subterranean rendering is implemented. See [Underground Worlds and Rendering](UNDERGROUND_RENDERING.md) for the detailed design and staged proof.


## Continuous spatial topology proof

The new engine-independent spatial primitives represent a region with a 3D origin, extent, floor elevation, and formation reason. Spatial features remain extensible semantic records (for example, a chasm, bridge, ladder, gate, or collapse), while connections separately declare adjacency and a traversal mode. A connection may depend on an operational crossing feature.

`SpatialTopology.WithFeatureOperationalState` creates a new snapshot rather than mutating the prior one. The initial tests prove that destroying a bridge removes traversal across the fissure but does not remove either ledge, the chasm, or the original snapshot's bridge state. A vertical ladder connection remains independently usable.

The console proof serializes a cavern with two separated natural ledges, a deep fissure, a suspended bridge, and an upper gallery reached by ladder. GitHub Pages presents the before/after states interactively. This is an initial standalone spatial-topology slice: the legacy generated chamber/passage graph has not yet been replaced or automatically projected into these 3D regions. Integration with world generation and richer geometry is a subsequent step.

