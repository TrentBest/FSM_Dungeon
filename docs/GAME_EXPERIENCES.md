# Reusable Game Experiences

**Status:** architecture contract and two project briefs. These are not playable game releases. The manifests describe independently authored experiences that consume the shared FSM_Dungeon domain.

## The reuseability proof

FSM_Dungeon is the reusable environment system; a game is a consumer. The same world facts—regions, chambers, passages, barriers, environmental conditions, resources, hazards, construction history, and spatial presence—must support different rules, interfaces, pacing, and player roles without forking the domain.

The first two experience briefs intentionally exercise different interaction models:

1. **Lanternward: The Delve** — an original, legally distinct tabletop-inspired tactical dungeon adventure. It tests compact authored maps, party exploration, hidden information, traps, doors, treasure, encounters, and a human or software-run opposing world director.
2. **Underkingdom** — a future asymmetric underground-settlement management experience. It tests construction, excavation, logistics, resource networks, population needs, territorial expansion, environmental consequences, and defense.

These are working titles and original Workshop projects. They do not imply affiliation with, endorsement by, or reproduction of any third-party game.

## Shared domain contract

Both experiences consume the same concepts:

- **World and spatial identity:** stable world, region, chamber, passage, feature, and location identifiers.
- **Topology:** geometric adjacency, traversal state, actor capability, visibility, and environmental connectivity remain separate facts.
- **World state:** barriers, mechanisms, lights, water, air, temperature, hazards, resources, and structural condition have authoritative state.
- **Causality and provenance:** features record why they exist, who or what created them, their material/source, and subsequent changes.
- **External actors:** SURPS owns actor ontology, capabilities, needs, relationships, and behavior. Dungeon records presence and environmental consequences; it does not invent a second character or creature system.
- **Content catalogues:** an experience may provide original rooms, props, mechanisms, items, encounter definitions, architectural styles, and scenario data. Catalogue entries are inputs, not hard-coded engine types.
- **Observation:** hidden, discovered, remembered, rumored, and currently visible information must not be collapsed into one omniscient map.
- **Determinism:** fixed world definition, seed, commands, and time-step policy produce reproducible authoritative results.

Experience-specific rules must live outside the Dungeon domain. A board-game-style turn sequence must not become the environment's update loop; a management game's resource economy must not become a mandatory Dungeon subsystem.

## Experience A — Lanternward: The Delve

An original tabletop-inspired adventure for a small party exploring a dangerous underground site. Its first proof should be playable on a compact grid or room-and-passage board, while the domain remains three-dimensional and continuous.

### Intended player experience

- Assemble a party from original SURPS-backed character definitions.
- Explore a site whose map can contain known, discovered, and concealed information.
- Move through doors, corridors, ladders, bridges, shafts, and other explicit connections.
- Encounter original hazards, mechanisms, puzzles, inhabitants, and environmental obstacles.
- Recover objects whose placement and history make sense in the world.
- Resolve a scenario through explicit objectives and consequences, not merely by reaching the final room.

A human facilitator may direct the opposition and reveal information, or an independently authored rules/AI adapter may do so. That facilitator is an experience role, not an authority embedded in FSM_Dungeon.

### Legal and originality guardrails

The project may use broad, unprotectable genre conventions and independently designed mechanics. It must not copy or adapt a third party's protected expression, including:

- game title, logo, trade dress, branding, or confusingly similar product presentation;
- published rulebook wording, scenario text, quest structure, card text, or distinctive terminology;
- board layouts, room maps, illustrations, miniatures, character names/designs, monsters as specifically depicted, or other proprietary assets;
- proprietary combinations or presentation of distinctive game elements.

All names, rules text, board layouts, scenarios, characters, art, icons, audio, and product identity must be independently authored. Do not import scans, photographs, extracted assets, or copied rule text. Keep provenance for every third-party reference and obtain qualified legal review before commercial release. This is an engineering content policy, not legal advice or a guarantee of non-infringement.

### First vertical slice

The first data fixture is [The Lantern Vault](../samples/Dungeon.Proof/Scenarios/lantern-vault.json). It uses stable three-dimensional region identities, explicit feature-gated connections, SURPS-owned external actor references, observer-scoped discovery, two objective outcomes, and a replay command log. Its grid is only a presentation projection; the 3D world remains authoritative. The first host-neutral command processor now exists as `DungeonScenarioState`: it validates movement against traversable spatial connections, scopes inspection knowledge per actor reference, opens adjacent crossing features, records cache recovery, and produces deterministic state digests. The sample host now deserializes the fixture, replays its command log through the domain API, and emits a replay proof with a state digest. Objective/outcome resolution and a playable interactive host remain pending.

1. One original compact site with a small number of chambers and explicit passage connections.
2. Original party definitions supplied through SURPS references.
3. A turn-based host adapter that submits validated movement and interaction commands.
4. Per-player discovery state separate from authoritative world truth.
5. One original door/lock interaction, one environmental hazard, one trap mechanism, and one meaningful cache.
6. A scenario objective with at least two valid outcomes.
7. A replay fixture proving the same seed and command sequence reproduce the same world-state result.

Do not build a clone of another game's board, rules, scenarios, or character roster.

## Experience B — Underkingdom

A future asymmetric underground-world management experience. The player is a keeper, founder, or steward shaping an underground domain while external populations live, work, compete, and respond.

### Intended player experience

- Choose where to excavate, reinforce, construct, and connect spaces.
- Establish rooms and infrastructure with costs, material requirements, and construction provenance.
- Manage supply, water, air, heat, waste, storage, access, and defensive systems.
- Attract, host, or govern populations whose needs and behavior come from SURPS.
- Respond to incursions, rival claims, disasters, shortages, and internal consequences.
- Expand through a world whose geology, topology, and resource distribution constrain the plan.

This is not a direct recreation of Dungeon Keeper. Its own terminology, room catalogue, creatures, interface, progression, economy, narrative, art, and mechanics must be designed independently.

### First vertical slice

1. One bounded underground region with an authored substrate and explicit schematic status where provider truth is unavailable.
2. Excavation and construction commands validated against spatial and material constraints.
3. A small original set of room functions represented as experience content, not Dungeon enums.
4. A resource ledger with provenance and deterministic transfers.
5. SURPS-backed population references and needs; no parallel Dungeon creature AI.
6. Environmental consequences for at least one infrastructure failure.
7. A simulation tick and replayable command log.

## CiAi pipeline compatibility

These projects are intended as future CiAi autonomous-development workloads, not a reason to bypass its gates. The pipeline should operate on isolated issues/branches or generated proposals, compile and test against pinned dependencies, run deterministic fixtures and content-policy checks, report evidence, and request host approval where required.

A model may propose code, data, tests, maps, or content. It must not publish NuGet packages, merge protected branches, silently replace canonical world data, or bypass the host's approval policy. Generated assets and text must pass originality/provenance checks and ordinary review.

## Ownership and integration

| Concern | Owner |
|---|---|
| Underground geometry, topology, environmental state, provenance | FSM_Dungeon |
| Actor identity, capabilities, needs, relationships, behavior | SURPS |
| Physical material and terrain truth | Physics |
| Campaign canon, adventures, scenes, durable narrative events | StoryBuilder |
| Turn sequencing, scenario objectives, player-facing rules | Lanternward experience adapter |
| Construction progression, management economy, keeper interface | Underkingdom experience adapter |
| Input, authority, persistence, multiplayer, approval | Host / FSM_COS composition |
| Representation and pixels | Renderer / Visualization |

The same Dungeon library must be usable by both experiences without referencing either experience's assembly, manifest, rules, or assets.
