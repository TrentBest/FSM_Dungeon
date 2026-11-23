# Underground Fortifications, Bunkers, and Civil Shelters

**Status:** domain design and integration contract; not a claim that structural or blast simulation is implemented.

## Purpose

Underground environments include more than caves, mines, and adventure spaces. They also include deliberately engineered protective spaces: storm shelters, public bomb shelters, hardened command facilities, refuge chambers, protected hospitals, civil-defence networks, and military bunkers.

These are not merely rooms with a stronger wall material. A shelter is a system of spatial volumes, boundaries, access routes, environmental services, protection assumptions, occupancy limits, maintenance history, and dependencies on the surrounding structure and terrain.

This document establishes the FSM_Dungeon ownership boundary and the handoff to Physics, raWWar, SURPS, and host systems.

## Ownership

| Concern | Owner | Contract |
|---|---|---|
| Underground voids, chambers, passages, entrances, shafts, constructed volumes, and spatial relationships | FSM_Dungeon | Authoritative underground spatial and construction history |
| Shelter identity, intended use, design basis as authored metadata, capacity declaration, access policy references, service layout, inspection history, and current recorded condition | FSM_Dungeon | Descriptive facility state; not a safety certification |
| Rock/soil substrate, blast propagation, pressure, heat, structural loading, fracture, collapse, and other physical calculations | Physics | Physical inputs, solver assumptions, outputs, and uncertainty |
| Military emplacement, fortification doctrine, defensive networks, command roles, operational use, siege, assault, and force-level tactics | raWWar | Military doctrine and operational composition |
| Civilian identities, groups, needs, capabilities, accessibility requirements, roles, and behavior | SURPS | Actor ontology and behavior; Dungeon stores only opaque references and occupancy facts |
| Emergency authority, evacuation coordination, alert delivery, communications, persistence, and user interaction | Host / composed capabilities | Policy and orchestration outside the environmental domain |
| Observer-specific presentation | Renderer / Visualization | Derived representations, never structural truth |

A facility may be represented in Dungeon and referenced by raWWar. The two systems must not maintain competing copies of its physical condition or topology.

## Shelter and fortification vocabulary

Use extensible authored classifications rather than a closed enum. Initial vocabulary includes:

- **Civil shelter:** public or private refuge intended for civilian protection.
- **Storm shelter:** refuge for severe weather and related environmental threats.
- **Fallout shelter:** a facility whose authored design assumptions include radiological shielding and controlled exposure; no protection guarantee is implied by the label.
- **Refuge chamber:** a protected temporary holding space within a larger facility, mine, transit system, or industrial complex.
- **Medical refuge:** protected care space with declared access, power, air, water, and logistics dependencies.
- **Command bunker:** protected operational facility; its command doctrine and organization belong to raWWar or the relevant host.
- **Personnel bunker:** protected accommodation or duty space.
- **Hardened infrastructure node:** protected communications, power, water, control, or logistics facility.
- **Fortified district/network:** multiple facilities and connecting routes composed into a larger spatial system.

These labels describe intended function, not validated performance. A shelter can be abandoned, flooded, contaminated, inaccessible, structurally compromised, or obsolete.

## Facility model: facts to preserve

A future implementation should represent a facility through stable identities and references, not one monolithic bunker type.

1. **Spatial composition**
   - one or more constructed volumes and regions;
   - entrances, exits, emergency egress, shafts, access corridors, and connecting passages;
   - barriers, doors, hatches, seals, and controlled boundaries;
   - adjacency and containment kept distinct from current traversability.

2. **Construction and provenance**
   - builder/organization as an opaque external identity;
   - construction and modification events, ordering, and stated purpose;
   - substrate/material references owned by Physics or an authoritative material provider;
   - supports, linings, partitions, penetrations, and service routes;
   - inspection, repair, damage, abandonment, and repurposing history.

3. **Declared design and capacity**
   - intended use and threat/design assumptions as authored metadata;
   - nominal occupancy and any declared duration assumptions;
   - accessibility and equipment constraints;
   - declared protection zones or boundary layers;
   - source, revision, confidence, and provenance for every engineering assertion.

   Capacity and design metadata are not inferred from room volume alone. Any computed occupancy, egress, or safety claim requires an explicitly validated model and policy supplied by the appropriate provider.

4. **Environmental services**
   - air intake, exhaust, filtration equipment references, and ventilation topology;
   - water supply, storage, treatment, drainage, and sanitation connections;
   - power, lighting, communications, heat rejection, and backup-system references;
   - consumable stores and service dependencies;
   - operational, degraded, failed, isolated, and unknown states where supported.

   Dungeon records the facility's spatial/service topology and observed state. Physics evaluates physical processes; domain-specific service solvers may own their own calculations. Do not imply that a filtration label proves breathable air or that a generator label proves available power.

5. **Occupancy and access**
   - opaque actor/group references and current spatial presence;
   - access-control policy references, not an independent authorization engine;
   - entry/exit events and observed occupancy changes;
   - privacy-aware, observer-scoped knowledge;
   - no assumptions that all occupants share the same information.

## Damage, events, and uncertainty

A consequential event should preserve what changed, where, when/order, the cause or actor reference, evidence, and the provider that supplied any physical result. Examples include a blocked entrance, damaged barrier, failed ventilation component, water ingress, loss of power, structural warning, partial collapse, route isolation, or repair.

Keep three things distinct:

- **Observed condition:** what an observer or inspection actually recorded.
- **Modeled condition:** what a named provider calculated under a declared model and input set.
- **Unknown condition:** what has not been inspected or cannot be established.

Do not turn an alert, damage observation, or schematic visualization into an unsupported statement about survivability. A structural warning can be represented without Dungeon pretending to solve structural engineering.

## Cross-domain composition with raWWar

raWWar should reference Dungeon facility and topology identities. It can then define:

- why a position or facility is selected for a military purpose;
- how facilities relate to command, logistics, observation, communications, and defensive organization;
- doctrine for protecting, evacuating, abandoning, isolating, supplying, or contesting a facility;
- force-level operations involving fortification networks, approaches, routes, and surrounding terrain;
- how an operation changes as routes or facilities become unavailable.

Dungeon supplies the authoritative spatial facts and recorded environmental/structural condition. Physics supplies physical effects. raWWar owns military interpretation and operational decisions. SURPS supplies the actors who perform actions and experience consequences. A host validates and applies cross-domain commands.

This also supports civilian emergency use without importing military doctrine into the civil-shelter model.

## Proof scenarios

The first authored proving cases should be deliberately small and distinguish data from validated simulation:

1. **Community shelter:** a constructed underground refuge with two entrances, declared capacity metadata, independent ventilation and power dependencies, and an inspection history.
2. **Mine refuge chamber:** a chamber connected to a working tunnel network, with a blocked primary route and a separately represented alternate route.
3. **Fortified mountain complex:** a command facility, protected service nodes, multiple access routes, and a connected refuge network inside the Dwarven Mountain Fortress scenario.
4. **Civil-defence network:** multiple shelters connected by surface and underground routes, with observer-specific knowledge and an emergency host workflow.

For each proof, identify which values are authored, observed, calculated, or unknown. Do not use decorative cutaways as proof of a physical solver.

## Acceptance gates

- Every facility and spatial component has a stable identity.
- Spatial geometry, adjacency, traversability, and access policy remain separate facts.
- Construction and modification history is attributable and ordered.
- Services are represented as explicit dependencies, not implied by a room label.
- Physical calculations identify the Physics/provider model, inputs, units, and uncertainty.
- No unsupported claim of blast resistance, radiation protection, structural safety, survivability, or certified occupancy is emitted.
- raWWar references Dungeon identities instead of duplicating the facility state.
- SURPS actor ontology and behavior remain outside the Dungeon assembly.
- Deterministic replay includes authoritative facility changes and their provenance once the runtime model is implemented.

## Implementation sequence

1. Extend the domain vocabulary and sourcebook contract with facilities, service dependencies, and facility events.
2. Define immutable facility state and validation contracts without inventing physical calculations.
3. Add deterministic scenario tests for facility identity, topology, service dependencies, event ordering, and immutable snapshots.
4. Integrate a provider-neutral physical-result boundary for Physics outputs, preserving model/version/provenance.
5. Add the small community shelter and mine refuge proofs.
6. Coordinate raWWar references and operational doctrine against stable Dungeon facility IDs.
7. Add Pages inspection only after canonical data and its evidence labels are available.

## Non-goals

- No blast, radiation, structural, fire, or life-safety solver is introduced by this design document.
- No engineering certification or real-world shelter recommendation is implied.
- No military doctrine or command ontology is owned by FSM_Dungeon.
- No civilian or military actor ontology is owned by FSM_Dungeon.
- No copied third-party bunker designs, maps, scenarios, or protected expression.
