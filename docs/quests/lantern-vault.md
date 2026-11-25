# Quest: The Lantern Vault

**Experience:** Lanternward: The Delve  
**Quest ID:** `fsm-dungeon.quest.lantern-vault`  
**Status:** first SURPS integration quest — interactive developer playtest harness available; not yet a player-facing release  
**Scenario fixture:** [lantern-vault.json](../../samples/Dungeon.Proof/Scenarios/lantern-vault.json)

## Premise

The party is hired to recover the Lantern Seal from a vault beneath an abandoned waystation. The route was deliberately sealed after the old ventilation system failed. A rope span crosses a deep fracture, an iron-leaf door protects the vault, and an upper ledge offers a second route. The site records the decisions of its builders; the party must learn enough of that history to restore a safe route and recover the seal.

All names, places, scenario text, and mechanics in this quest are original Workshop material.

## Player onboarding

The experience requests a player character through the SURPS-owned entry point:

```csharp
Surps.Create.PlayerCharacter();
```

That call is expected to launch a GUI intake flow, collect character data under the active SURPS rules and the loaded experience/book configuration, then return a reusable character document. SURPS owns the character schema, validation, serialization, and deserialization. The experience may supply defaults, allowed choices, and book-specific constraints; it must not fork the character model.

A player should create a character once and reuse the serialized character document across compatible experiences. The experience save stores a reference or snapshot according to the SURPS persistence contract; it does not invent a second character format.

## Composition and book registration

When the experience and its books are registered, each may contribute declarative settings and data:

- **SURPS core books** contribute reusable character concepts, rules, and ontology.
- **Dungeons and Underground** contributes dungeon-specific options and guidance.
- **Underground Environments** contributes environment vocabulary and world facts.
- **Lanternward** contributes its premise, starting role, permitted options, and quest-specific defaults.

The composition host resolves the registered contributions before invoking character creation. Explicit experience settings take precedence over book defaults; incompatible constraints must be reported rather than silently discarded. The MicroBundle load context currently provides no settings/data registration API, so this document describes the target contract and does not pretend that load-time contribution plumbing is already implemented.

## Quest objectives

1. **Establish a safe route.** Discover the vault's access history and restore at least one traversable connection.
2. **Recover the Lantern Seal.** Locate and recover the meaningful cache from the sealed vault.
3. **Return with evidence.** Preserve the party's discovery and action history so the outcome can be replayed and reviewed.

The deterministic fixture proves the route/cache objective pattern and replay mechanics. A separate `Dungeon.Play` console host now loads this same fixture and accepts live `move`, `inspect`, `interact`, and `recover` commands against the immutable scenario state. It rejects invalid commands without advancing the command log and displays objective completion, selected outcome, and a final state digest. This is a developer playtest harness: it deliberately exposes fixture targets and is not the player-facing discovery interface. The full quest still needs a SURPS character document, encounter and obstacle resolution, a player-facing GUI, observer-appropriate discovery, and an integrated campaign save.\n\nRun it from the repository root:\n\n```bash\ndotnet run --project samples/Dungeon.Play/Dungeon.Play.csproj\n```\n\nUse `look`, `status`, `move <region name>`, `inspect <feature name>`, `interact <feature name>`, and `recover cache.survey-ledger`. The initial party can enter Silt Hall, open the Iron-leaf door from the adjacent crossing, enter the Sealed Vault, and recover the ledger. It can also activate the rope span from the adjacent Silt Hall crossing to establish the upper route.

## First encounter design

The party encounters environmental clues before any mandatory combat. A damaged ventilation path, old tool marks, and a disabled crossing feature reveal that the vault was isolated intentionally. A pressure-slab mechanism can be added as a deterministic Dungeon trigger whose effect descriptors are routed to the appropriate environment, Physics, or SURPS host adapter. Dungeon records the trigger and provenance; it does not calculate injury or decide an actor's behavior.

## Acceptance criteria

- A new player can invoke the SURPS character-creation entry point from this experience.
- A previously created character can be deserialized and reused without repeating intake.
- Registered books and the experience contribute settings/data without changing the shared character schema.
- The party uses SURPS actor references; FSM_Dungeon stores presence and world interactions only.
- The vault scenario replays deterministically from its authored seed and command sequence.
- Character, quest, and world persistence remain distinct documents with explicit references and provenance.
