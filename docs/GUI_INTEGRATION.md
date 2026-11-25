# GUI Integration: the player's interface is not the dungeon

## Purpose

FSM_Dungeon is the authoritative underground-environment domain. GUI is the semantic interaction and information layer through which a person or an input adapter observes and interacts with an experience. Renderer and Visualize own the separate decisions and execution involved in presenting a spatial scene.

The distinction is deliberate:

- **Dungeon** answers what exists, where it is, why it formed, what changed, and which validated environmental effects are possible.
- **SURPS** answers what inhabitants are, what they can do, what they want, and how they behave.
- **GUI** describes the meaning and structure of the interface: navigation, status, inventory, map, quest journal, dialogue, action choices, inspection, settings, and diagnostics.
- **Renderer** selects observer-relative representation and computational detail under a budget.
- **Visualize** manifests selected spatial/visual representations, including scene and GPU work where supported.
- **Host / FSM_COS / Foundry** compose capabilities, connect adapters, authorize commands, and execute the experience. The host remains responsible for command validation, permissions, persistence, and lifecycle.

GUI is not a replacement for Renderer or Visualize, and a viewport is not permission to mutate the world.

## The composition

```text
Dungeon domain + SURPS + game rules + content
                  |
                  v
       runtime experience / state
                  |
          +-------+--------+
          |                |
          v                v
   GUI semantic tree   observer + budget
          |                |
          v                v
   platform adapter     Renderer policy
          |                |
          |                v
          |             Visualize
          |                |
          +------ host-owned viewport
                  |
          input -> host validation -> domain command
```

A player can inspect the same world through several manifestations without changing the domain: a browser HUD, native desktop panels, a VR wrist interface, a diagnostic inspector, or an accessible text-first interface. The physical input mechanism (mouse, keyboard, touch, gaze, controller) is not the semantic meaning of an action.

## A dungeon interface is more than a map

A useful action-RPG interface should compose distinct semantic regions. These are experience-level responsibilities, not new types to add to the Dungeon domain.

| Interface region | What it communicates | Authoritative source |
| --- | --- | --- |
| World viewport | Current spatial scene and visible effects | Renderer policy + Visualize; host supplies viewport |
| Minimap / topology | Known routes, blocked crossings, discovered areas | Dungeon topology filtered by observer knowledge |
| Character / party | Health, capabilities, conditions, equipment | SURPS and the experience rules |
| Inventory / loot | Items, ownership, capacity, comparison | Experience inventory/economy rules; SURPS identity where applicable |
| Quest / objectives | Goals, evidence, completion and consequences | Scenario/quest evaluator |
| Dialogue / interaction | Available choices and responses | SURPS, scenario rules, host-authorized commands |
| Inspection / codex | Provenance, geology, history, hazards, inhabitants | Dungeon queries plus SURPS observations |
| System / diagnostics | Simulation tick, state digest, network and capability health | Host/runtime diagnostics |

The interface should be contextual. Selecting a chamber can reveal its formation, history, known entrances, hazards, resources, and current observations. Selecting an actor can reveal only information the observer is entitled to know. A blocked bridge should appear as a blocked crossing, not as deleted geography. A quest objective should distinguish unknown, observed, actionable, completed, and failed rather than reducing every state to a checkbox.

## Concrete GUI integration seam already implemented

The GUI repository documents and tests a hosted viewport contract:

- Core provides `GuiBuilders.Viewport(id, accessibleLabel)`.
- `BlazorGuiRenderer.RenderWithHostedSurface(GuiNode, Func<GuiNode, RenderFragment?>)` delegates each viewport to a host-supplied Blazor fragment.
- The renderer wraps that fragment in a semantic, accessible region with stable node identity.
- The host owns the supplied scene component and may compose Renderer and Visualize behind it.

The GUI integration example from the GUI repository is intentionally host-owned:

```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using TheSingularityWorkshop.Workshop.Gui;

var page = GuiBuilders.Panel("experience")
    .Child(GuiBuilders.Viewport("battlefield", "Battlefield view")
        .Property("class", "battlefield-viewport"))
    .Build();

var content = BlazorGuiRenderer.RenderWithHostedSurface(
    page,
    viewport => builder =>
    {
        builder.OpenComponent<DungeonSceneSurface>(0);
        builder.CloseComponent();
    });
```

Here, `DungeonSceneSurface` is a host component, not a Dungeon or GUI type. It can consume Renderer decisions and Visualize output. The current hosted viewport contract does **not** claim typed camera/transform semantics, spatial hit testing, automatic policy negotiation, GPU availability, input routing, or complete resize/disposal lifecycle. Those remain host and cross-repository integration work. See the GUI repository's [hosted viewport contract](https://github.com/TrentBest/TheSingularityWorkshop.GUI/blob/development/docs/integrations/VISUALIZE_HOSTED_VIEWPORT.md).

The recursive Blazor `GuiNode` renderer is also explicitly a prototype: its currently documented known kinds are Panel, Button, Image, Text, and Warning. Do not assume that arbitrary properties automatically become portable accessibility, event, or interaction contracts. Build against the exact adapter surface and tests in the GUI repository.

## What this Pages site proves today

The current Dungeon Pages explorer is a useful browser-side proof of domain evidence: it selects generated worlds, inspects chamber provenance, replays a deterministic scenario, and toggles a bridge state from serialized C# proof data. It is **not** yet a GUI.Core semantic tree rendered by GUI.Blazor, nor a live authoritative game client. Keep that distinction visible.

The next integration should be a separate host proof rather than quietly turning the static inspector into a second game engine:

1. Compose a semantic experience shell with named regions for scene, map, status, inventory, objectives, dialogue, and inspection.
2. Give the scene region a labelled GUI viewport and supply a host-owned scene surface.
3. Populate non-scene panels from explicit runtime observations; preserve observer knowledge and authorization boundaries.
4. Route semantic actions to host commands. Validate them against the current authoritative state before calling Dungeon/SURPS/experience operations.
5. Test the same experience intent through the Blazor adapter and at least one non-browser representation before claiming cross-adapter parity.

## First game direction: original isometric action RPG

The genre target can borrow broad design patterns—readable combat space, deliberate exploration, meaningful drops, build choices, dangerous excursions, and a compelling return loop—without copying Diablo or IBM Hack's protected maps, characters, writing, art, or distinctive content.

The first playable vertical slice should be small but end-to-end:

- one sanctuary hub and one connected delve;
- one player-controlled SURPS actor and a small, behaviorally distinct population;
- movement, inspection, one validated interaction, and one combat encounter;
- a clear danger/retreat/return loop;
- one recoverable reward and a persistent character consequence;
- a GUI HUD, minimap, inventory, objective panel, and contextual inspector;
- a repeatable scenario/evidence record proving the state transitions.

Do not build every panel as a hard-coded page before the underlying contracts exist. Start with the complete player loop and make each panel reflect authoritative state.

## Evidence and next work

- [GUI repository](https://github.com/TrentBest/TheSingularityWorkshop.GUI/tree/development)
- [GUI hosted viewport contract](https://github.com/TrentBest/TheSingularityWorkshop.GUI/blob/development/docs/integrations/VISUALIZE_HOSTED_VIEWPORT.md)
- [Dungeon architecture](ARCHITECTURE.md)
- [Reusable game experiences](GAME_EXPERIENCES.md)
- [Lantern Vault scenario fixture](../samples/Dungeon.Proof/Scenarios/lantern-vault.json)
- [Current Pages explorer](https://trentbest.github.io/FSM_Dungeon/)

**Status:** architecture and integration plan documented; no claim is made that GUI.Blazor is already wired into the Dungeon Pages site or a playable game host.
