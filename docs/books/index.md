# FSM_Dungeon Fantasy Library

**Status:** first authored library foundation. These are original, system-neutral fantasy sourcebooks and story seeds. They are not a reproduction of any existing tabletop game's protected setting, text, characters, or rules.

FSM_Dungeon is the Workshop home for fantasy-world content that benefits from a shared place-and-environment perspective: peoples and their homelands, bestiaries, lairs, ruins, underground civilizations, wilderness sites, dungeons, and the histories that connect them.

The library is designed to compose with SURPS and StoryBuilder rather than duplicate either.

## How the library composes

| Layer | What it contributes | Owner |
|---|---|---|
| Sourcebook knowledge | Peoples, creatures, places, cultures, histories, ecology, hazards, adventure locations | Domain sourcebook repository; this library for Dungeon/fantasy content |
| Roleplaying ontology and rules | Entity identity, capabilities, needs, behavior, relationships, action semantics, resolution | [SURPS](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem) |
| Environment and spatial truth | Caves, lairs, ruins, topology, terrain handoffs, environmental conditions, spatial presence | FSM_Dungeon and its owning providers |
| Campaign authorship and continuity | Campaigns, adventures, quests, scenes, choices, events, canon, timelines, observer knowledge, published manuscripts | [StoryBuilder](https://github.com/TrentBest/StoryBuilder) |
| Warfare doctrine | Threat models, organized force, fortifications, siege, logistics, recovery | [raWWar](https://github.com/TrentBest/raWWar) |
| Civic life and settlement geography | Residents, visitors, markets, schedules, civic institutions, place relationships | [Singularity City](https://github.com/TrentBest/TheSingularityWorkshop.SingularityCity) |

A sourcebook is reusable knowledge, not a campaign. A campaign pins the sourcebook editions it uses and records campaign-specific changes separately. StoryBuilder's campaign model explicitly distinguishes sourcebooks, campaigns, adventures, quests, sessions, scenes, encounters, and durable events.

## LOD and disclosure

- **LOD 0 — Existence:** a people, creature, place, or phenomenon may be known to exist without its full definition being available.
- **LOD 1 — General knowledge:** broad traits, variation, ecology, culture, and common encounters.
- **LOD 2 — Domain detail:** specialized societies, habitats, lairs, institutions, regional histories, and actionable environmental detail.
- **LOD 3+ — Named instances:** a particular kingdom, forest, ruin, lair, city, lineage, or campaign-specific location with stable identity and persistent history.

Higher LOD adds detail; it does not retroactively make every lower-LOD observer omniscient. Rumor, public knowledge, scholarly account, direct observation, and GM-only truth remain distinguishable.

## Books

### Foundations
- [Fantasy Foundations](fantasy-foundations.md) — a system-neutral framework for building coherent fantasy worlds, their peoples, environments, magic, histories, and conflicts.

### Peoples
- [Elves: Peoples of the Long Memory](elves.md) — an original people sourcebook covering variation, communities, history, relationships, and a campaign-ready storyline seed.

### Bestiary
- [The Living Bestiary](bestiary.md) — an ecological and narrative framework for creatures, populations, habitats, lairs, signs, encounters, and coexistence.
- [Dragons and Wyrms: Ecology of the Elder Sky](dragons-and-wyrms.md) — draconic lineages, habitats, life histories, intelligence, hoards, and relationships without assuming one universal dragon.

### Places and history
- [Ruins and Lost Places](ruins-and-lost-places.md) — a causal framework for abandoned, repurposed, buried, and partially active sites.

### Story seeds
- [The Broken Star](../stories/the-broken-star.md) — an original, system-neutral fantasy campaign seed designed to be expanded in StoryBuilder.

## Authoring rules

1. Prefer original setting expression. Familiar fantasy archetypes are starting points, not permission to copy another publisher's distinctive expression.
2. Describe creatures and peoples as populations with variation, ecology, history, and agency—not as encounter statistics alone.
3. Separate observed facts, common beliefs, disputed accounts, rumors, and GM-only truths.
4. Never hard-code SURPS rules into sourcebook prose or Dungeon environment code.
5. A lair, ruin, or settlement must have a reason to exist and a causal history.
6. StoryBuilder owns the evolving campaign narrative. It references sourcebook identities and pinned revisions instead of forking sourcebook truth.
7. Treat any proposed world canon as a proposal until reviewed and accepted by the author/GM.

## StoryBuilder handoff

The sourcebook library should eventually be discoverable as typed, versioned sourcebook references in StoryBuilder's World and Sourcebooks area. The first authoring integration should support:
- stable sourcebook and entry IDs;
- title, domain, LOD, revision, and source URI;
- reusable entry references from campaigns, adventures, quests, scenes, and encounters;
- campaign-owned adaptations without changing the shared sourcebook;
- explicit canon status and provenance for generated story proposals.

See the [StoryBuilder Campaign Builder and GM Workbench](https://github.com/TrentBest/StoryBuilder/blob/development/docs/CAMPAIGN_BUILDER_AND_GM_WORKBENCH.md).
