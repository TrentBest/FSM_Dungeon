# FSM_Dungeon MicroBundle

The FSM_Dungeon MicroBundle exposes the Workshop's engine-independent underground-environment capability through the neutral MicroBundle contract.

## Ownership

- **FSM_Dungeon** owns underground spatial topology, regions, connections, features, environmental state, resources, hazards, and construction history.
- **SURPS** owns actor identity, ontology, capabilities, needs, relationships, and behavior.
- **Physics** owns physical calculations and material consequences.
- **Experience adapters** own their rules, objectives, player interaction, and progression.

The MicroBundle composes the capability; it does not transfer ownership of the Dungeon domain to the host or to SURPS.

## Compatibility

The package targets .NET 10 and depends on `TheSingularityWorkshop.MicroBundleDomain`. Consumers should pin compatible dependency versions and compose this bundle through the host's MicroBundle loading contract.

## Development status

This is an alpha integration package. The deterministic domain and scenario proof are implemented; a complete playable host and the runtime book/settings contribution contract remain in development.

## Source and documentation

- [Repository](https://github.com/TrentBest/FSM_Dungeon)
- [Architecture](https://github.com/TrentBest/FSM_Dungeon/blob/development/docs/ARCHITECTURE.md)
- [Reusable game experiences](https://github.com/TrentBest/FSM_Dungeon/blob/development/docs/GAME_EXPERIENCES.md)
- [Lantern Vault quest](https://github.com/TrentBest/FSM_Dungeon/blob/development/docs/quests/lantern-vault.md)
- [Build workflow](https://github.com/TrentBest/FSM_Dungeon/actions/workflows/build.yml)

This package is not published to NuGet by the repository's CI workflow. CI builds and uploads an artifact for verification only.
