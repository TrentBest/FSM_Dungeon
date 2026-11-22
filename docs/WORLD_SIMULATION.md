# Living Subterranean Worlds

## Design objective

A dungeon is a continuous, persistent underground environment—not a numbered sequence of floors. Its geometry, routes, materials, environmental state, resources, hazards, and history continue to exist when no player is observing them. Inhabitants also persist there, but their ontology and behavior are supplied by SURPS, not defined by FSM_Dungeon.

The domain must support anything from a municipal sewer to an ancient burial complex, a dwarven city, a goblin warrens network, an orcish frontier, a biological nest, or an impossible ruin. These are not generator modes. They emerge from authored causes, available content, spatial structure, and simulation.

## Domain scope

The word *dungeon* is the domain umbrella for places below the surface, not a requirement that a place be artificial or maze-like. Natural caves and caverns, fissures, burrows, root systems, mines, tunnels, excavations, catacombs, sewers, foundations, constructed undercities, and unfamiliar subterranean environments all belong. Their origin and material history distinguish them; they do not require separate engine modes.

The underground also needs a distinct observation and rendering regime: enclosed spaces, local light sources, strong occlusion, dynamic shadows, and close-range material detail. The domain should preserve the facts that make those representations truthful without becoming a graphics engine. See [Underground Worlds and Rendering](UNDERGROUND_RENDERING.md) for the lighting, shadow, material, perception, and staged proof requirements.

## 1. One world, many elevations

Do not make "level" the primary spatial unit. A level or floor is a useful way to partition, stream, summarize, or present a world, but it is not the world itself.

The authoritative model is a spatial world containing regions and places with position and extent in three dimensions. Connections describe how entities can move between places; they are not restricted to ordinary doors or stairs.

Examples include:
- stairs, ladders, ramps, lifts, elevators, and spiral shafts;
- tunnels, doors, gates, bridges, narrow ledges, and crawlspaces;
- wells, pits, vertical drops, one-way slides, and climbing routes;
- chasms, abysses, lava fissures, underground lakes, and vast open voids;
- broken bridges, collapsed passages, sealed gates, flooded routes, and temporary crossings;
- hidden passages, secret doors, portals, and deliberately non-Euclidean connections.

A chasm is not simply a missing passage. It is a spatial feature with extent, depth, edges, visibility, possible crossings, hazards, and consequences for movement and sight. A bridge can connect two otherwise separated regions; destroying it changes route availability without erasing either region.

Separate:
- **spatial adjacency** — what is physically near or connected;
- **traversal capability** — who or what can cross, by what means, and at what cost;
- **visibility and perception** — what can be seen, heard, smelled, sensed, or inferred;
- **simulation reachability** — which routes are currently usable by a particular actor or group.

This avoids treating a blocked door, an impassable chasm, a hidden tunnel, and a long stair as equivalent graph edges.

Floors and named dungeon depths should be derived views over the spatial world. A host may stream or render one region at a time, but simulation and navigation must preserve continuity across those boundaries.

## 2. Inhabitants are external entities, not Dungeon-owned behavior

**SingularityUniversalRolePlayingSystem (SURPS) owns the ontology and behavior of inhabitants.** It defines what an entity is and how it can behave: identity and classification, roles, capabilities, needs, motivations, goals, relationships, social organization, culture, decision-making, and behavioral rules. It should support people, creatures, groups, societies, solitary beings, colonies, machines, and other kinds of actors without Dungeon inventing a separate behavior system.

FSM_Dungeon owns the environment those entities inhabit. It provides spatial regions, positions and extents, routes, environmental conditions, resources and materials in context, hazards, and the consequences that the underground environment imposes. It can record an external entity's presence at a location and the environmental effects of an action without defining the entity's ontology or choosing its goals.

The integration boundary should be explicit:

- SURPS describes the actor, its role, capabilities, motivations, relationships, and behavioral state.
- FSM_Dungeon describes the underground environment and answers environment-specific queries such as where an actor is, what routes exist, and what environmental conditions or hazards apply.
- Other capabilities may supply movement execution, sensing, communication, physics, combat resolution, or interaction mechanics.
- A host or orchestration layer coordinates those capabilities and validates authoritative state transitions.

This separation allows the same SURPS-defined entity to inhabit a dungeon, a city, an overworld, a spacecraft, or another environment without changing its fundamental ontology. Likewise, FSM_Dungeon can host inhabitants from different SURPS catalogues without hard-coding species or behavior.

A scout, patrol, expedition, settlement, or society is therefore not a Dungeon-specific AI archetype. It is a SURPS-defined entity or organization acting within a Dungeon environment. The environment can record their spatial presence, routes, encounters, and observable consequences; the behavior model determines why they undertake those actions.

## 3. Environmental simulation at multiple scales

FSM_Dungeon should not require every region or environmental process to run at maximum detail every frame. Use layered environmental fidelity:
- **immediate** — detailed local environmental interactions and route consequences;
- **local** — nearby environmental state and resource availability;
- **regional** — coarse environmental processes and changes across connected regions;
- **distant** — event-driven or summarized environmental state.

SURPS and other actor capabilities determine how inhabitants are simulated at corresponding scales. A group travelling through unobserved space is not a Dungeon-owned behavior loop; Dungeon supplies the environmental queries and consequences needed by the external simulation. When an observer approaches, detailed environmental representation should be reconstructed from authoritative state rather than inventing a disconnected scene.

Simulation must be deterministic for a fixed initial definition, seed, command sequence, and time-step policy. Randomness is recorded or derived from stable identities and simulation state so that a world can be reproduced and investigated.

## 4. Creator-provided content, world-determined placement

Separate the **content catalogue** from the **generated world**.

The creator supplies the ingredients the world is allowed to use. These can include:
- creatures, populations, factions, and settlement concepts;
- treasure, currency, equipment, relics, consumables, and resources;
- traps, mechanisms, locks, alarms, puzzles, and defensive devices;
- furniture, tools, containers, doors, bridges, lifts, and environmental props;
- architectural styles, materials, symbols, language fragments, and cultural details;
- hazards, environmental phenomena, and special spatial features.

Each supplied entry can describe its identity, tags, provenance, compatibility, capabilities, constraints, rarity or abundance, value, and relationships. The domain should not assume that every creator supplies every category, or that every item has a fixed placement.

The generator and simulation determine where and how supplied content appears based on the world's causes and current state. A trap might guard a valuable store, protect a settlement's boundary, defend a narrow crossing, or remain as a forgotten remnant of an older civilisation. Treasure might be carried by a patrol, stored in a treasury, buried in a grave, hidden in a cache, or scattered by a failed expedition.

Placement is therefore a contextual decision, not a random scatter operation. The system should be able to explain why an item is present, who placed it or owns it, what purpose it serves, and how it came to be where it is.

The creator's catalogue is an allowed-content boundary. The world may compose and place supplied content, but should not silently invent creator-owned assets or protected third-party material outside the supplied/authorized catalogue. Procedurally generated combinations and descriptions must still respect the creator's declared constraints.

## 5. Traps, treasure, and meaningful discovery

Treat traps and treasure as world elements with state and provenance, not as level-completion rewards.

A trap can have:
- a mechanism and trigger condition;
- an activation state and reset/rearm behavior;
- a detection surface and discoverability;
- an intended target or protected asset;
- a builder, owner, maintenance history, and failure modes;
- physical consequences that interact with the world.

A treasure object or cache can have:
- an owner, claimant, or historical association;
- a reason for being stored, carried, hidden, buried, or abandoned;
- access requirements and protective measures;
- condition, quantity, and provenance;
- consequences when removed, traded, consumed, or destroyed.

HeroQuest-like traps, secret doors, treasure chests, keys, pressure plates, falling blocks, darts, pits, and hidden passages are useful concept references for interactions—not a fixed template library or a limit on the system. Each should be expressible as an original content definition and composed into a world for a reason.

## 6. Causal explanation and events

The domain should answer:
- Why is this creature here now?
- Where is its home, and where is it going?
- What route did it take, and why?
- Who owns this cache, and why was it hidden?
- Who built this trap, what does it protect, and is it maintained?
- What lies across this chasm, and what would make crossing possible?
- What changed after a gate was opened, a bridge collapsed, or a patrol disappeared?

Record meaningful state changes as causal events with stable identities, time/tick, actor or cause, affected entities, and resulting changes. Events support debugging, replay, history, explanations, quests, and downstream AI observations. Avoid turning every low-level arithmetic operation into an event; record domain-significant changes.

## 7. Generation and authoring contract

A Dungeon environment definition should combine:
1. an underground origin, formation history, and purpose;
2. spatial and environmental constraints or authored landmarks;
3. environmental materials, resources, hazards, and authored features;
4. generation policies and a reproducible seed;
5. optional host-world anchors and access rules;
6. optional references to externally defined SURPS entities or populations and their initial spatial placements.

SURPS owns the inhabitant definitions, ontology, roles, goals, relationships, and behavior policies. Dungeon may accept opaque external identities and placement instructions at an integration boundary, but must not become the canonical catalogue or behavior engine for those entities.

The Dungeon generator composes a valid environment from its own inputs. It may determine chamber placement, route geometry, geological or constructed features, and environmental resource locations, while preserving invariants and explaining decisions through provenance. External orchestration composes SURPS entities into that environment and resolves their behavior. Hand-authored environmental landmarks and explicit placements remain possible.

## 8. Observer knowledge and perception

Authoritative geological history and an inhabitant's knowledge are separate state. A world may contain formation, exposure, discovery, and survey facts without every inhabitant knowing any of them. `GeologicalObserverKnowledge` is an immutable per-observer snapshot; `LearnEvent` returns a new snapshot after a host perception system has adjudicated that evidence was perceived. It does not itself perform line-of-sight, lighting, sensing, interpretation, or movement checks. The host must supply the observation sequence, confidence, and interpretation. Learning an event never changes `GeologicalWorld`, and one observer's knowledge is not shared with another.

The current code still contains a provisional Dungeon-owned group placement model with motivation and activity strings. That is a boundary leak to be retired as the SURPS integration contract becomes concrete; it must not be mistaken for the target ontology. The knowledge boundary now has a group-level snapshot and an explicit scout-report transfer. A scout's discoveries remain private; `GeologicalGroupKnowledge` changes only when a `ScoutGeologicalReport` is delivered after the host establishes that the scout physically returned to the addressed group. A scout killed, captured, delayed, or otherwise prevented from returning does not transmit its discoveries. The group can notice that an expected report is overdue (`ScoutReportingStatus`), but overdue means whereabouts unknown—not confirmed death, capture, or knowledge of what happened. The enemy does not receive the scout's knowledge either. It may form its own suspicion only from evidence available to it, such as a missing patrol member it expected to see or other observable events; no telepathy or implicit faction-wide mind sharing exists. Individual actor identity, movement, sensing, and the host's authoritative adjudication of death, return, and report delivery remain integration work. In the target architecture, SURPS owns the actor and group identities/behavior; Dungeon only supplies underground location and environmental facts. The report-delivery mechanism belongs to the communication/orchestration capability, not to Dungeon's ontology.

## 9. Proof sequence

Build this in layers rather than pretending the full simulation already exists:

1. Extend spatial topology to represent elevation, extents, and non-stair traversal features, including an impassable chasm with a separate bridge/crossing feature.
2. Introduce a creator-supplied content catalogue and contextual placement provenance for a small set of original traps, treasure, and props.
3. Give one population a home, territory, needs, and a route-based expedition. Demonstrate a scout appearing far from its home while the main population remains elsewhere.
4. Demonstrate multiple populations with different movement capabilities and relationships.
5. Add settlement-scale production, supply, and civic systems for a dwarven city.
6. Connect environmental simulation—such as the existing hydraulic slice—to route accessibility, settlement needs, and population decisions.

Each step needs deterministic tests and an inspectable proof. Do not jump to combat polish before the world itself behaves coherently.

## Non-goals

- No assumption that a dungeon is a vertical stack of numbered floors.
- No assumption that every inhabitant is hostile or that every encounter is pre-positioned.
- No closed enum for fantasy peoples, dungeon origins, trap families, or content categories.
- No mandatory real-time simulation of every individual at full fidelity.
- No copied game content, protected maps, names, art, writing, or assets.
