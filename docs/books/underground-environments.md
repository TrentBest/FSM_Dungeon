# Underground Environments: Formation, Space, and Consequence

**Library:** FSM_Dungeon Domain Sourcebooks  
**Sourcebook ID:** `fsm-dungeon.domain.underground-environments`  
**Entry namespace:** `dungeon.environment.*`  
**Domain:** Underground environments  
**LOD:** 2 — domain detail  
**Edition:** 0.1.0  
**Cover:** [Original sourcebook cover](../assets/sourcebooks/underground-environments-cover.svg)  
**LOD badge:** [LOD 2 — Domain Detail](../assets/sourcebooks/lod-2-domain-detail.svg)  
**LOD badge:** ![LOD 2 — Domain Detail](../assets/sourcebooks/lod-2-domain-detail.svg)  
**Status:** authored domain sourcebook; implementation contracts are normative only where explicitly marked  
**Owner:** FSM_Dungeon

## Purpose

An underground environment is a continuous three-dimensional place shaped by formation, material, water, air, construction, use, and change. It is not a stack of rooms or a sequence of encounter floors.

This sourcebook establishes the environmental vocabulary and authoring rules for caves, caverns, fissures, burrows, root systems, mines, excavations, tunnels, sewers, catacombs, foundations, undercities, buried facilities, and unfamiliar subterranean spaces. These are not separate engine modes. They are outcomes of causes, histories, materials, and spatial relationships.

FSM_Dungeon owns underground spatial topology, voids, constructed spaces, environmental state, and the history of environmental change. Physics owns authoritative terrain/solid substrate and physical calculations where a provider exists. SURPS owns inhabitants and their behavior. StoryBuilder owns campaign continuity and authored events.

## 1. Environmental identity and origin

Every environment has a stable identity independent of its current name, owner, inhabitants, or presentation. Names may change; identity must not.

An environment origin is descriptive and extensible. Examples include:

- natural dissolution, erosion, fracture, collapse, lava formation, or sedimentary processes;
- biological excavation, nesting, root growth, or colony construction;
- human or other sapient excavation, mining, tunnelling, or quarrying;
- engineered construction, including foundations, transit systems, reservoirs, and service infrastructure;
- machine excavation or boring;
- mixed origins, later reuse, unknown origin, or deliberately disputed accounts.

Do not encode this list as a closed enumeration. Preserve the authored origin label, supporting evidence, confidence, and provenance so new formation types do not require a breaking API change.

## 2. Space is continuous and three-dimensional

The authoritative environment is a bounded or explicitly unbounded spatial frame containing regions and spatial features. Floors, levels, maps, and rooms are useful partitions and views, not the world itself.

Model separately:

- **Region:** a spatially bounded or logically grouped volume with stable identity.
- **Void:** an unoccupied or occupiable volume within a substrate; natural, excavated, constructed, or unknown provenance.
- **Chamber:** a named or functional place within a region.
- **Passage:** a spatial connection with geometry, directionality, clearance, and environmental properties.
- **Boundary:** a surface or interface separating materials, spaces, or environmental conditions.
- **Barrier:** a door, gate, blockage, collapse, seal, or other condition affecting passage.
- **Vertical feature:** shaft, pit, well, ladder, lift, stair, ramp, ledge, or drop.
- **Hazard region:** a spatially located condition whose consequences are evaluated by the relevant provider.

Spatial adjacency is not the same as traversability. A sealed gate can preserve adjacency while preventing passage. A chasm can separate two ledges without making either ledge disappear. Actor capability and movement execution remain external concerns.

## 3. Substrate handoff and coordinate truth

Where an authoritative solid substrate exists, FSM_Dungeon references it rather than generating a competing terrain or rock volume.

A substrate handoff must identify:

1. provider and stable snapshot identifier;
2. coordinate frame, axis convention, units, and origin;
3. world bounds and spatial resolution;
4. material/region field identity and version;
5. snapshot revision or content hash;
6. provenance and generation inputs, when available.

Dungeon-authored voids and structures must declare the coordinate frame they use. A mismatch is a validation error, not an invitation to silently rescale or reinterpret coordinates.

If no 3D substrate provider is available, use explicitly authored schematic geometry. Never describe a schematic cutaway as a generated physical mountain.

## 4. Provenance: natural, excavated, and constructed

A space or feature should preserve how it came to exist. At minimum, distinguish:

- natural formation;
- biological formation;
- excavated void;
- constructed space or structure;
- converted or repurposed space;
- unknown or contested origin.

A feature may have multiple events in its history. An ancient natural cavern can be enlarged by miners, lined by masons, flooded, drained, and later reused as a reservoir. Preserve this sequence rather than replacing the origin with the latest use.

An excavation or construction event should record stable event identity, affected feature identities, actor/organization references supplied externally, time or ordering, method, material movement where known, and a provenance link to the resulting geometry or state.

## 5. Environmental state

Environmental state is spatially located, time-aware, and attributable. It may include:

- water level, flow, pressure, source, sink, blockage, and overflow;
- air composition, ventilation, pressure, smoke, and contaminant presence;
- temperature, humidity, condensation, and thermal gradients;
- illumination sources and occlusion-relevant environmental facts;
- sediment, mineral deposition, corrosion, erosion, and weathering;
- structural damage observations and environmental obstructions;
- resource deposits, depletion, replenishment, and access conditions.

Do not imply calibrated physical accuracy when a model is only a first-order simulation. Every solver must document its assumptions, supported domain, units, stability limits, and known exclusions. Physics remains authoritative for general physical calculations.

## 6. Connectivity and validation invariants

A valid world definition should be inspectable and deterministic. Validation should report errors with stable feature identifiers rather than silently repairing authored input.

Core validation expectations:

1. IDs are unique within their declared namespace.
2. Every referenced region, chamber, passage, barrier, and substrate exists.
3. Coordinate frames and units are compatible.
4. Spatial extents are finite and well-formed where bounded geometry is required.
5. Passage endpoints reference valid spatial features.
6. A blocked passage remains represented as a connection with a blocked state; it is not deleted.
7. A destroyed bridge or removed gate changes the relevant traversal/obstruction state without deleting adjacent regions.
8. Natural, excavated, and constructed provenance remains distinguishable after later modifications.
9. Deterministic generation with the same definition and seed produces equivalent authored outputs.
10. Validation distinguishes malformed data from a valid but disconnected or intentionally isolated region.

Connectivity is a domain fact, not a universal requirement that every space be reachable. A sealed vault, isolated cavern, or collapsed shaft may intentionally be disconnected. A scenario can declare required routes and test those requirements explicitly.

## 7. Environmental processes and fidelity

Environmental processes should support different simulation budgets without changing the meaning of authoritative state:

- **Local:** detailed processes near an active observer or interaction.
- **Regional:** aggregated state for connected regions and nearby processes.
- **World:** coarse state and scheduled updates for distant regions.

A lower-fidelity update may aggregate or defer work, but it must not invent precision or silently erase causal history. Record the model, timestep or elapsed interval, inputs, and approximation level used for consequential updates.

## 8. Observer, visibility, and illumination

Illumination, visibility, and knowledge are different facts.

- Illumination describes light reaching a location or surface.
- Visibility describes whether a particular observer can perceive a target under relevant occlusion and sensory conditions.
- Knowledge describes what an observer has learned, inferred, or been told.

Dungeon may provide geometry, light-source placement, and occlusion-relevant environmental facts. Renderer selects an observer-appropriate representation; SURPS or another perception provider determines sensory capability and knowledge. A dark chamber is not automatically known to be empty.

## 9. Materials and environmental evidence

Material identity should be linked to the authoritative substrate/material provider when possible. Dungeon may add context: exposed surface, deposit, lining, debris, construction use, weathering, contamination, or resource access.

Surface evidence should be explainable by material and history: mineral veins, fractures, tool marks, machine-boring patterns, masonry joints, soot, water stains, corrosion, biological growth, repairs, and collapse. These are evidence-bearing features, not arbitrary decorative noise.

Renderer and Visualization may derive geometry and pixels from these facts. They do not become the authoritative owner of material or environmental truth.

## 10. Hazards and resource context

Dungeon locates and describes environmental hazards and resources; it does not duplicate universal actor damage, needs, or action-resolution rules.

Examples include unstable ground, falling debris, flooding, poor air, extreme temperature, darkness, contamination, deep water, toxic deposits, restricted clearance, and blocked escape routes. Each hazard record should identify location, observed or modeled condition, provenance, uncertainty, and relevant environmental process.

Resources should record type/reference, location, accessible quantity or state when known, depletion/replenishment model where supported, and extraction history. SURPS-defined actors may have capabilities and motivations related to those resources; those rules remain external.

## 11. Sourcebook-to-implementation map

| Sourcebook concept | Owning implementation area | Proof expectation |
|---|---|---|
| Environment identity and origin | Domain model | Stable IDs and extensible origin metadata |
| 3D regions and voids | Spatial model | Deterministic region/void scenario |
| Passages and barriers | Topology model | Block/unblock without deleting adjacency |
| Substrate handoff | Physics integration boundary | Coordinate and snapshot validation |
| Excavation/construction history | Provenance model | Ordered events with affected feature IDs |
| Water and air state | Environmental simulation | Explicit assumptions and repeatable scenario |
| Materials and deposits | Provider references + Dungeon context | No duplicate substrate authority |
| Hazards and resources | Environmental facts | Located, attributable, uncertainty-aware records |
| Observation inputs | Renderer/Perception boundary | Illumination distinct from visibility/knowledge |
| Scenario presentation | Pages/Visualization | Same authoritative data across map/cutaway/inspection |

## 12. First proving scenario: Dwarven Mountain Fortress

The Dwarven Mountain Fortress is the first large cross-domain proving environment. Physics supplies the mountain substrate when its 3D volume contract is ready. FSM_Dungeon authors the natural voids, excavations, constructed districts, passages, gates, water systems, and history. SURPS supplies dwarven populations and institutions. Renderer and Visualization present the same world at map, cutaway, isometric, and close-inspection scales.

Until the Physics 3D substrate exists, the scenario must remain explicitly schematic. The fortress is not to be represented as physically generated merely because a height field or authored cutaway is available.

See [The Dwarven Mountain Fortress scenario](../DWARVEN_MOUNTAIN_FORTRESS.md).

## 13. Non-goals and ownership guardrails

- No second terrain, geology, or general-purpose solid-volume engine in FSM_Dungeon.
- No universal inhabitant ontology, AI behavior, needs, combat, or social rules in FSM_Dungeon.
- No graphics engine or renderer-owned world truth.
- No forced Unity, SURPS, GUI, Renderer, Visualization, WebApp, ServerApp, or AI dependency in the core library.
- No claim of engineering safety or calibrated fluid/structural simulation without validated models.
- No campaign canon created by a generated scenario; StoryBuilder owns campaign continuity and canon decisions.

## Revision policy

This is a reusable domain sourcebook. Consumers should pin its stable sourcebook ID and edition. Corrections that clarify this contract may increment the patch revision; new compatible domain coverage may increment the minor revision; changes to identity, ownership, or required semantics require a major revision.
