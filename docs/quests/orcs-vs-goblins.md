# Experience premise: Orcs vs Goblins

**Working title:** Orcs vs Goblins  
**Setting:** a single inhabited mountain with connected natural and constructed spaces  
**Status:** seed premise for a future SURPS composition; not a playable experience

## Core premise

A mountain is home to two distinct populations: Orc communities and Goblin communities. They occupy different elevations, caverns, mines, ledges, halls, and engineered passages. Their settlements expand, trade, compete for resources, negotiate, raid, defend, and adapt to changing conditions inside the same persistent mountain.

The conflict is not a fixed alignment or a requirement that every individual fight. SURPS should describe the inhabitants as reusable entities with individual identities, capabilities, needs, roles, relationships, motivations, and behavior. Factions and settlements emerge from those entities and their shared history. Some groups may cooperate while others compete.

## Ownership boundaries

- **FSM_Dungeon:** mountain topology, natural geology, excavated regions, passages, structural features, environmental conditions, construction provenance, and world history.
- **SURPS:** Orc and Goblin character/creature ontology, population needs, capabilities, relationships, social roles, motivations, behavior, and rulebook composition.
- **Physics:** material properties, forces, structural consequences, fluids, heat, and other physical simulation.
- **StoryBuilder:** durable narrative events, campaign continuity, and authored story arcs.
- **Experience adapter:** settlement-management rules, conflict pacing, player goals, and experience-specific choices.
- **FSM_COS / FSM_Execution:** composition and execution of the loaded experience.

Neither species nor faction should become a hard-coded FSM_Dungeon enum. They are SURPS-authored data and ontology contributions.

## Composition experiment

The experience should register its premise and book contributions before world/character creation. The mountain book may supply geology and settlement-space defaults; SURPS books supply reusable population and social concepts; the experience supplies its own faction relationships, starting conditions, and available choices.

Character creation should call the same SURPS entry point used by Lanternward:

```csharp
Surps.Create.PlayerCharacter();
```

The loaded experience changes the available context through configuration, not by replacing the API or creating another character schema. Saved characters and population entities should be serializable and reusable.

## First simulation questions

1. What natural and constructed spaces exist at each elevation?
2. Which resources are locally available, renewable, scarce, or contested?
3. What does each settlement need to remain viable?
4. Which passages are public, controlled, concealed, damaged, or strategically important?
5. How do trade, migration, negotiation, construction, and conflict change the mountain over time?
6. What events cause a temporary rivalry to become a lasting conflict—or a durable alliance?

## First bounded proof

Start with one mountain cross-section represented by authoritative 3D regions and explicit connections. Place one Orc settlement and one Goblin settlement in distinct but connected regions. Give each a small set of SURPS-backed inhabitants, needs, roles, and relationships. Add one shared resource, one contested passage, and one construction project per settlement. Simulate a short deterministic sequence containing a trade proposal, a resource shortage, a construction decision, and a conflict or negotiated resolution.

The proof is successful when the same mountain supports both populations without Dungeon owning their ontology, and when a saved character/entity can be reloaded without repeating creation intake.

## Future development

Use this premise as a second proving ground for the future Autonomous Development Pipeline (ADP). Keep the initial scope deliberately small: establish the composition contract, prove reusable character persistence, and simulate one meaningful interaction before attempting a large-scale faction or settlement simulation.
