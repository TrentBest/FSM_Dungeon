# Living Subterranean Worlds

## Design objective

A dungeon is a continuous, persistent place—not a numbered sequence of floors. Its geometry, inhabitants, routes, resources, hazards, and history continue to exist when no player is observing them.

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

## 2. Populations, not encounter placements

An inhabitant group is a population with identity, needs, capabilities, territory, relationships, resources, and intentions. It is not merely a list of monsters assigned to a room.

A population can:
- maintain a home settlement or stronghold;
- send scouts and patrols beyond its defended territory;
- forage, hunt, fish, farm fungi, mine, salvage, trade, raid, or collect tribute;
- move food, equipment, prisoners, wounded members, messages, and treasure;
- establish temporary camps, watch posts, outposts, and new settlements;
- react to danger, scarcity, weather, flooding, blocked routes, rival groups, and player activity;
- retreat, reinforce, negotiate, migrate, split, or collapse.

This creates a natural encounter gradient. A player may meet one scout, a foraging party, a patrol, a returning expedition, or a messenger far from the population's main settlement. Reaching its territory can reveal a much larger society. The early sighting is a clue about a living population, not a promise that the entire population is waiting in the next room.

For example, an orc group might maintain a deep stronghold, patrol an upper route, forage near an underground lake, and send a scouting party through an abandoned mine. The player can encounter those parties before discovering the stronghold. The group should have reasons for each journey and should respond when a party fails to return.

A dwarven city should be modeled with the same underlying capabilities as any other settlement: districts, households or cohorts, civic functions, production, storage, trade, defense, transport, water, waste, and political or social relationships. Its scale and sophistication emerge from its authored content and simulation, not from a special hard-coded "dwarf dungeon" mode.

Goblins, orcs, trolls, spiders, undead, and other subterranean beings are likewise not restricted to monster-room placement. Their ecologies, social structures, intelligence, diets, mobility, and relationships can differ. Some may be civilizations, some loose bands, some solitary predators, some colonies, and some forces that do not behave like ordinary societies at all.

## 3. Simulation at multiple scales

The world should not require every inhabitant to run a detailed actor update every frame.

Use layered fidelity:
- **immediate** — detailed movement, combat, perception, and interaction near the observer;
- **local** — group movement, patrols, encounters, and resource use in nearby regions;
- **regional** — schedules, expeditions, migration, supply, and territory changes;
- **distant** — event-driven or summarized population state, with enough continuity to produce plausible arrivals and consequences.

A group travelling through unobserved space should still make progress, consume resources, encounter obstacles, and produce events. When the observer approaches, the detailed representation should be reconstructed from the authoritative group state rather than spawning a disconnected encounter.

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

A world definition should combine:
1. an origin, purpose, and history;
2. spatial and environmental constraints or authored landmarks;
3. creator-supplied content catalogues and selection rules;
4. population definitions, goals, relationships, and initial knowledge;
5. generation policies and a reproducible seed;
6. optional host-world anchors and access rules.

The generator composes a valid world from those inputs. It may determine exact room placement, route geometry, population distribution, trap placement, and treasure location, but must preserve invariants and explain its decisions through provenance.

The creator should be able to specify intent and available ingredients without hand-placing every room, monster, or chest. Hand-authored landmarks and explicit placements remain possible when a creator wants precise control.

## 8. Proof sequence

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
