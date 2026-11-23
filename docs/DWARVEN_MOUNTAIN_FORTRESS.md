# The Dwarven Mountain Fortress — world scenario and integration contract

**Status:** authored scenario and integration target. The fortress is not yet a generated or playable world. This document defines the first shared proving environment for Physics, FSM_Dungeon, SURPS, and the Pages experience.

## The premise

A dwarven civilization has built its ancestral fortress inside a mountain massif. It is not a stack of dungeon floors. It is a continuous three-dimensional environment connected by gates, stairs, lifts, ramps, ladders, shafts, bridges, tunnels, galleries, and natural passages.

The mountain existed before the fortress. Its geology, ridges, faults, water, material distribution, and physical boundaries constrain every construction decision. Dwarven settlement history is visible in the mountain: old natural caverns, first excavations, abandoned workings, expanded halls, defensive works, workshops, reservoirs, and later repairs.

The scenario should feel inhabited and purposeful. A hall exists because a society needed it; a passage follows a route because of geology, engineering, defense, trade, water, or historical accident. We should be able to inspect those reasons.

## Sourcebook reference

The environmental ownership and authoring contract for this scenario is defined by [Underground Environments: Formation, Space, and Consequence](books/underground-environments.md) (sourcebook ID: `fsm-dungeon.domain.underground-environments`, LOD 2, edition 0.1.0). This scenario applies that reusable domain contract; it does not replace it with fortress-specific rules.

## LOD and destination boundary

The fortress is a higher-detail LOD 2 destination, not the baseline definition of dwarves. A world can record the existence of dwarves at LOD 0; a town-market scene can use the general Dwarves LOD 1 sourcebook without exposing the stronghold. Entering or meaningfully exploring the stronghold requires the SURPS Dwarven Strongholds LOD 2 sourcebook. A specific premade dwarven city is a further LOD 3+ settlement instance, composed with Singularity City for its civic geography and persistent place relationships.

## Domain boundaries

- **Physics** owns the exterior terrain and the physical mountain volume, geological material fields, and any supported physical calculations.
- **FSM_Dungeon** owns the underground world: natural voids, excavated spaces, constructed spaces, chambers, passages, shafts, gates, topology, environmental state, and excavation/construction history.
- **SURPS** owns dwarven entities and groups: identity, roles, capabilities, needs, motivations, crafts, relationships, culture, governance, trade, defense, and behavior.
- **Perception and communication** determine what each dwarf knows, has sensed, or has been told. Shared world state is not shared knowledge.
- **Gestures / movement** provide climbing, walking, hauling, operating mechanisms, mining actions, and other movement/action procedures.
- **Renderer / Visualization** present the same authoritative world at map, cutaway, isometric, and close inspection scales.
- **Host / FSM_COS / FSM_Foundry** compose capabilities and validate authoritative transitions.

Dungeon must not invent a separate dwarf ontology. Physics must not become a fortress generator. SURPS must not become the owner of geological calculations or chamber geometry.

## Mountain-to-fortress composition

The integration is a one-way handoff of authoritative data with explicit provenance:

1. Physics generates an exterior terrain definition and a bounded 3D mountain-volume snapshot.
2. Physics exposes stable spatial conventions, material regions, bounds, resolution, and a versioned provenance record.
3. Dungeon imports or references that snapshot as the geological substrate for an underground world.
4. Dungeon defines natural voids, excavation events, constructed volumes, chambers, passages, gates, and environmental regions in the same coordinate frame.
5. A host composes SURPS dwarven populations and organizations into selected settlement regions.
6. Renderer and Visualization present the world from an observer's position and knowledge state.

The existing Physics height field is a valid first exterior-surface source, but it cannot by itself represent the solid mountain or its internal voids. Until a 3D volume provider exists, any cutaway mountain must be clearly labeled as a schematic or authored scenario—not presented as generated physical terrain.

## Fortress districts (initial authoring vocabulary)

| District | Purpose and defining features | Questions the world should answer |
|---|---|---|
| High Gate and Switchback | Exterior approach, defensive gatehouse, terraces, steep road, watch positions | What can approach, from where, and what is visible? |
| The Great Hall | Civic heart, assembly, feasts, public memory, major vertical volume | Why was this hall placed here, and how is it supported? |
| The Deep Foundry | Smelting, forging, heat, ventilation, fuel and material logistics | Where do heat, smoke, slag, water, and supplies go? |
| The Stonewright Quarter | Masonry, carving, structural components, repair and construction | Which materials and tools are available, and where did they come from? |
| The Delving Works | Active and abandoned mines, shafts, spoil routes, ore seams | What is being extracted, what is depleted, and what hazards follow? |
| The Underways | Main circulation, lifts, stairs, bridges, gates, freight routes | Which routes are open and traversable by this actor? |
| The Deep Reservoir | Cisterns, channels, aquifer access, drainage and water control | What are the water sources, capacities, outlets and downstream consequences? |
| The Root Halls | Older natural caverns incorporated into the fortress | Which features predate dwarven construction? |
| The Ancestor Vaults | Memorial and burial architecture, records and protected chambers | What is preserved, who has access, and what is known? |
| The Lower Dark | Sealed, abandoned, dangerous or unexplored depths | What evidence exists, and what remains unknown? |

District names are working names. They should become stable identities only when the scenario catalogue is formalized.

## Construction is history, not decoration

Every important built feature should be explainable through a construction record:

- builder or organization (external SURPS identity);
- construction purpose and historical period;
- source material and provenance;
- geometry and spatial extent;
- method and required capabilities;
- excavation volume and spoil destination, where known;
- current condition and later modifications;
- known supports and unresolved structural questions;
- evidence and confidence.

A carved arch, dressed-stone wall, timber support, metal gate, or abandoned shaft should not be an arbitrary texture. It is a constructed feature with a history. A rendered support is not proof that the structure is physically safe.

## Three-dimensional topology

The fortress is one continuous spatial world. Named districts, galleries, and "depths" are useful partitions and navigation aids, not independent floors.

Represent separately:
- geometric adjacency and spatial containment;
- traversable connections and their current state;
- visibility and sensory paths;
- actor-specific traversal capability;
- physical obstruction and clearance;
- environmental connectivity for water, air, heat, and sound.

A shaft may connect two regions vertically while preventing ordinary walking. A lift may connect the same regions only while powered and maintained. A collapsed passage may remain a geometric feature while ceasing to be traversable. A secret door may be physically present but unknown to a particular observer.

## First authored scenario sequence

1. **Approach:** establish the exterior mountain, ridgeline, switchback road, gate and visible entrances.
2. **Threshold:** pass through the gatehouse and descend into the Great Hall.
3. **Civic life:** inspect workshops, stores, public spaces and residential areas with purposeful population placement.
4. **Industry:** follow ore from a seam through extraction, sorting, hauling, smelting and finished goods.
5. **Infrastructure:** trace water from source through reservoir, distribution, use and drainage; trace ventilation from intake to exhaust.
6. **History:** compare natural cavern, first delving, expansion, defensive construction, collapse and repair.
7. **Incident:** introduce a blocked route, water event, structural warning or other bounded problem whose consequences propagate through the world.
8. **Expedition:** enter the Lower Dark with partial knowledge, actor-specific equipment and real route constraints.

This sequence creates an explorable place with a reason to exist before we attempt a full action-RPG loop.

## Dwarven society: content, not a stereotype

The initial society should not reduce dwarves to miners, warriors, beards, ale, and treasure. Those can be setting conventions where deliberately chosen, not universal biological or cultural rules.

A dwarven population may include engineers, masons, miners, metallurgists, farmers, healers, historians, merchants, cooks, water stewards, scouts, diplomats, teachers, children, elders, and people with no formal craft. Roles are not identities; a dwarf can hold several roles, change occupation, disagree with tradition, or belong to multiple organizations.

SURPS content should define:
- individual and collective identities;
- family, household, guild, crew, council, military and civic organizations;
- craft knowledge and training;
- needs, aspirations, loyalties, disputes and obligations;
- resource ownership, exchange, production and maintenance;
- law, custom, memory, language and cultural variation;
- relationships with other peoples and neighboring settlements.

A living society needs mundane continuity: food, sanitation, water, ventilation, waste, sleep, childcare, education, maintenance, trade and conflict resolution—not only dramatic encounters.

## Pages experience

The existing FSM_Dungeon Pages proof already demonstrates generated-world inspection, spatial topology, hydraulic snapshots and geological observations. Extend it into a mountain-fortress explorer in stages:

### View 1 — Mountain overview
A navigable exterior terrain surface with a fortress entrance marker, elevation inspection, coordinate readout, and source/seed provenance. This must be backed by Physics-generated terrain data.

### View 2 — Mountain cutaway
A section plane through the mountain showing the actual solid/void boundary and material regions. This requires the Physics 3D volume provider; until then use a labeled conceptual illustration only.

### View 3 — Fortress atlas
A 3D or isometric view of Dungeon regions, chambers, passages, vertical connections, gates and water infrastructure. Selection opens an inspector for purpose, formation, construction history, material, condition and connectivity.

### View 4 — Inhabitant lens
Select a dwarf or organization to inspect role, capabilities, current activity, needs, relationships and knowledge. Show only what the selected observer knows in the observer view; keep omniscient world truth as a separate authoring/debug mode.

### View 5 — Systems
Trace ore/material logistics, water, heat/ventilation, maintenance and route availability. Every displayed flow must be sourced from an implemented model or labeled as authored/static scenario data.

### Interaction
Start with selection, layer toggles, cutaway position, district navigation and route highlighting. Later add movement and action only when the relevant movement provider and host validation exist. A browser camera must not silently mutate world state.

## Acceptance gates

- One stable mountain coordinate frame is shared across Physics and Dungeon.
- The generated exterior terrain and 3D volume have reproducible definitions and run records.
- Dungeon can identify internal voids and construct topology without owning the physical solid model.
- Every district, chamber, passage and constructed feature has a stable identity and provenance.
- A selected route can explain why it is or is not traversable.
- Dwarven content is supplied by SURPS and is not hard-coded into Dungeon.
- Observer knowledge differs from omniscient world truth.
- Pages visualizations consume canonical world data and clearly distinguish implemented from conceptual layers.
- Tests cover deterministic generation, coordinate alignment, topology invariants, and any physical accounting claimed by the implementation.

## Implementation order

1. Physics: define the bounded 3D mountain-volume capability and evidence gates; retain the existing height field for the exterior.
2. Physics Pages: add a mountain terrain inspector using existing generated height-field data as the first vertical slice.
3. FSM_Dungeon: define the mountain-substrate handoff and author the fortress regions/topology against the shared frame.
4. SURPS: expand this scenario into dwarven society, role, craft, organization, and daily-life sourcebook material without changing the core contract.
5. Pages: combine the generated mountain, Dungeon topology, and SURPS population into one explorable experience.
6. Add coupled water, heat, ventilation, structural and logistics models only as their owning providers become validated.

## References

- [Physics mountain-volume design brief](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/GEOLOGY_MOUNTAIN_VOLUME_AND_FORTRESS.md)
- [Physics terrain height-field source](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/Physics/Geology/TerrainHeightField2D.cs)
- [FSM_Dungeon living-world architecture](WORLD_SIMULATION.md)
- [FSM_Dungeon underground rendering boundary](UNDERGROUND_RENDERING.md)
- [SURPS Dungeons and Underground sourcebook](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/blob/development/docs/books/dungeons-and-underground.md)
