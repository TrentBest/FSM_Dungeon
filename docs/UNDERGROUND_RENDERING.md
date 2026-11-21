# Underground Worlds and Rendering

## Design objective

FSM_Dungeon covers the world beneath the surface: natural caves and caverns, fissures, burrows, root systems, mines, tunnels, excavations, catacombs, sewers, foundations, constructed undercities, and stranger subterranean spaces. "Dungeon" is the domain umbrella, not a claim that every place is an artificial maze or an adventure-game level.

Subterranean environments require a distinct observation and rendering regime. They are enclosed, locally illuminated, heavily occluded, and often visually dense. The renderer must not treat them as outdoor scenes with a darker ambient color.

## 1. A different illumination model

Outdoor scenes commonly have a broad environmental light source and distant horizons. Underground, there may be no daylight contribution at all. Illumination comes from local sources and whatever light can physically reach the space:

- flames, torches, candles, braziers, furnaces, and burning materials;
- carried lamps and moving light sources;
- bioluminescent organisms, magical or otherwise unusual emitters;
- reflected or redirected daylight entering through shafts, cracks, vents, openings, or mirror systems;
- artificial fixtures, machinery, crystals, and other world-defined emitters.

A chamber with no effective light source should not be rendered as though it has a hidden outdoor fill light. Visibility and perception should follow the actual illumination and the observer's capabilities. Ambient or indirect illumination may exist, but it must be an explicit part of the lighting model rather than an arbitrary visibility allowance.

## 2. Light, shadow, and the whole space

Every relevant light source contributes to the illumination and shadowing of the environment. A torch is not a flat point of brightness pasted onto a room: it illuminates surfaces according to distance, direction, occlusion, material response, and the geometry between source and receiver.

The system should support:
- multiple simultaneous local lights with different intensity, color, range, and behavior;
- dynamic occlusion and shadows cast by geometry and moving objects;
- flickering flames whose changing output alters illumination and shadow over time;
- reflective and partially reflective surfaces that influence the light reaching other surfaces;
- shafts and narrow openings that create directional beams and sharply localized illumination;
- moving emitters that carry their changing light and shadows through the world.

The phrase "shadow culture" describes a core visual requirement: shadows are not a decorative effect applied after lighting. They are part of how the underground world communicates shape, distance, enclosure, motion, and uncertainty. The eventual implementation must account for the combined contribution of relevant lights, without assuming a single global shadow-casting sun.

A flickering flame should produce coherent temporal variation. Its brightness and position may vary, but the resulting illumination and shadows must respond to the same evolving source state; unrelated random brightness changes across surfaces would look synthetic. Deterministic simulation seeds or recorded source state should make the behavior reproducible when required.

## 3. Close-range material truth

Underground viewing distances are often short. The observer is surrounded by walls, ceilings, floors, ledges, and objects, so small-scale material detail carries more of the scene's identity.

The world representation should preserve or derive meaningful surface evidence such as:
- geological strata, mineral veins, sediment, fractures, inclusions, erosion, and water staining;
- roots, fungal growth, dampness, moss, deposits, and biological traces;
- irregular excavation marks, tool scrapes, overcuts, chisel scars, and successive layers of digging;
- machine-bored surfaces with characteristic repeated cutting or drilling patterns;
- worked stone, fitted blocks, brick irregularity, mortar joints, repairs, and weathering;
- soot, smoke deposits, corrosion, mineral crusts, scratches, impacts, and the accumulated marks of use.

These details should not be random noise. They should be consistent with the material, formation process, tools, age, environment, and history of the surface. A hand-dug tunnel, a naturally fractured cave, a machine-bored passage, and a carefully maintained brick sewer should read differently even before labels are shown.

Finished construction should look finished. A dressed stone wall or brick-lined channel should not inherit the visual language of raw rock merely because it is underground. Conversely, a collapsed or weathered section can reveal the original construction beneath later damage.

## 4. Geometry and material are evidence

Rendering detail should be grounded in domain meaning. A surface can carry material identity, formation method, construction technique, age, condition, and provenance. The presentation layer can use those facts to select geometry, surface variation, decals, and lighting response.

The domain should not become a graphics engine. FSM_Dungeon owns meaningful world facts and surface provenance; Renderer selects observer-appropriate representation and fidelity; Visualization and host adapters produce pixels and effects.

The same world may be inspected at several budgets:
- a low-cost map or schematic can communicate material regions, openings, light sources, and blocked sight;
- a close view can expose surface relief, tool marks, masonry, deposits, and local shadow;
- a high-fidelity view can add dynamic multi-light shadowing, reflected light, volumetric shafts, fine geometry, and animated flame response.

The representation must remain a view of the same authoritative world, not a second hand-authored scene with disconnected facts.

## 5. Observation is not illumination

Keep these concepts distinct:
- **illumination**: light energy reaching a surface or location;
- **visibility**: whether an observer has an unobstructed or otherwise valid line of sight;
- **perception**: what an observer can detect given vision, senses, equipment, and conditions;
- **rendering**: how the selected information is presented at a particular fidelity and budget.

A dark corridor may be geometrically visible but not perceptible to an unequipped observer. A bright shaft may illuminate dust and a distant wall without granting a clear view around a bend. Sound, scent, vibration, heat, and other senses may reveal things that vision cannot.

This separation also prevents the graphics implementation from becoming the authority for gameplay knowledge.

## 6. Progressive proof

Do not begin by promising a fully global real-time shadow system. Prove the concepts in slices:

1. Add world vocabulary for natural formation, material identity, surface process, and local light-source definitions without coupling the domain to a graphics API.
2. Create a small cave scene with no daylight, one torch, occluding geometry, and distinct geological surfaces.
3. Add a second moving/flickering flame and verify that both lights contribute coherent dynamic illumination and shadows.
4. Add a daylight shaft and a reflective redirection surface; trace the resulting light path as an explicit scene/lighting input.
5. Contrast natural rock, hand-excavated tunnel, machine-bored mine, and finished brick/masonry construction.
6. Connect observer perception and Renderer budgets so low-cost and high-detail views remain consistent with the same world facts.

Each stage should have a deterministic or reproducible proof and measurable rendering costs. Shadow-map strategy, light culling, multi-light compositing, global illumination, and hardware-specific techniques belong in the rendering implementation/adapters once the requirements are proven—not in the engine-independent dungeon domain.

## Guardrails

- No mandatory daylight or global sun contribution underground.
- No arbitrary ambient visibility that reveals an otherwise unlit space.
- No assumption that one light source owns all shadows.
- No random surface noise that contradicts formation or construction history.
- No graphics API dependency in the dungeon domain.
- No claim that the current Pages inspector implements this rendering system; this document defines the target and staged proof.
