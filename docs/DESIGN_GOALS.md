# Design Goals

This document describes the intended long-term game direction. It is a living end-state design document for future AI and developer sessions: when durable design direction becomes clearer, update this file so future work keeps pointing at the same game.

It is not a current implementation checklist and does not override `docs/CURRENT_SCOPE.md` or the scope guardrails in `AGENTS.md`. Implement new systems only when the user explicitly asks for them.

## How To Use This Document

- Use this file for long-term design direction, naming, content direction, system tradeoffs, and end-state intent.
- Use `docs/CURRENT_SCOPE.md` for exact current scope and `AGENTS.md` for scope guardrails.
- Use `docs/BACKGROUND.md` for setting, tone, and content flavor.
- Use `docs/ARCHITECTURAL_DEBT.md` for future architecture pressure.
- Use `docs/MECHANICS_BACKLOG.md` for deferred player-facing mechanics and systems.
- Use Git history, task summaries, tests, and `docs/TASK_LOG.md` for implementation history.

## Living Document Protocol

- Update this file only when long-term game direction changes or when a clarified end-state goal should guide future work.
- Do not update it for current implementation status, ordinary task summaries, bug fixes, local exceptions, or one-off feature details.
- Keep updates principle-based and focused on the intended future game shape. If a durable end-state goal becomes clearer, add or adjust concise guidance under the relevant section.
- Use compact `End-State Direction` notes only when they clarify the intended future game better than normal prose. Avoid dated decision logs unless the decision changes the lasting design picture.
- If a user chooses a local approach that conflicts with existing design goals, update this file only when the choice is a durable pivot. Otherwise, treat it as a local exception for the task summary or PR.

## AI Planning Protocol

- Before planning high-impact design choices, gather current repo and docs context first.
- Surface meaningful alternatives when a choice affects long-term direction, architecture, scope, or player-facing system shape. Name how each option aligns or conflicts with these goals.
- If an early slice is requested and the end-state goal is unclear, ask focused questions until the slice can point in the right long-term direction.
- Record newly clarified durable end-state goals here. Record future architecture work in `docs/ARCHITECTURAL_DEBT.md` and deferred player-facing mechanics in `docs/MECHANICS_BACKLOG.md` instead.
- Prefer choices that create readable logistics, practical resource tradeoffs, local-site risk, grounded tactical decisions, and domain-owned simulation. Avoid hidden realism, generic apocalypse flavor, unrequested scope, or systems the player cannot inspect or act on.

## Core Loop

The intended game is an open sandbox survival roguelike built around travel, local risk, and practical resource pressure.

The player moves across a broad overworld, chooses routes and destinations, enters local maps, searches or fights for resources, returns to their travel method or mobile base, and decides what risk to take next. A good system should make the player ask what they can carry, spend, repair, fuel, load, trade, risk, or abandon.

The game should grow through small, composable rules rather than one large simulation drop. Time, inventory, fuel, ammunition, visibility, sound, local-site state, NPC state, world objects, and terrain should become interconnected only as each piece has a clear player-facing purpose.

## Travel And Mobile Base

The mobile base is a core fantasy, but it should begin as continuity rather than a guaranteed vehicle. Early survival can involve walking, carried gear, a pushbike, or a tow-behind wagon. Later progression can include fragile cars, better cars, vans, trucks, buses, campers, and larger mobile hub platforms.

The base should feel practical: storage, fuel, power, repairs, equipment staging, route planning, and eventually crew support if party systems become real. It should not feel like high-tech spectacle. Losing or replacing a vehicle should hurt, but not erase everything. Some continuity lives in the player, knowledge, relationships, maps, carried items, and caches; some lives in the physical platform and can be lost.

World travel should imply cost even before detailed systems exist: time, fuel, exposure, attention, distance, route quality, and opportunity cost. Running out of fuel should change the plan, not simply end the run.

## Local Maps

Local maps are where abstract travel becomes concrete risk. They should be inspectable, tactical spaces with readable surfaces, objects, structures, containers, NPCs, and local stories.

Sites should be built around why they existed before the Failover and why the player cares now. Loot, danger, locks, power, cover, routes, and objects should follow real-world logic. A gas station is about fuel and access. A farmhouse is about food, tools, water, shelter, and signs of what happened. A clinic is about medicine, locked storage, triage, and failed systems.

Local actions should generally move through the action pipeline: request, validate, resolve, advance time when appropriate, emit messages, and refresh UI from domain state.

## Combat And Ballistics

Combat is a core activity, but it should feel grounded and consequential rather than arcade-like. The player should often fight, prepare to fight, or choose not to fight because ammunition, wounds, sound, position, weapon handling, and escape routes matter.

The long-term firearms model should use selective simulation. It can model range, cover, visibility, sound, ammunition type, weapon condition, reliability, wounds, material penetration, and armor when those details create readable tactical decisions. It should not simulate detail for its own sake if the player cannot understand or act on it.

Humans and automated systems are the main combat threats. Armed survivors, scavengers, guards, roadblock holdouts, and misaligned security systems should shape combat more than monsters. Wildlife and environmental hazards can exist, but they are secondary unless a specific feature asks for them.

## Inventory, Items, And Containers

Inventory pressure should be a major source of decisions. Size, quantity, weight later, ammunition type, feed devices, tools, fuel containers, medical supplies, food, repair parts, and equipped gear should all matter because they affect what the player can do next.

Items should be data-driven where practical. Use stack items for interchangeable quantities and stateful items for specific objects whose identity matters, such as a loaded magazine, a modified weapon, a damaged tool, a backpack with contents, or a special container.

Containers should make the world feel stored, not sprinkled with random loot. Homes, vehicles, shops, farms, depots, clinics, and workshops should hold resources in places that make sense. Search, transfer, capacity, access, locked state, and depletion should grow as focused vertical slices.

## NPCs, Factions, And Social World

The current player assumption is solo, but future companions or crew are possible. Do not build party systems unless explicitly asked, but avoid designs that make them impossible.

Long term, settlements, factions, trade, rumors, access rules, and local reputation should matter. They should shape routes, danger, information, and resource access without replacing the core survival logistics loop.

NPCs should not exist only to explain lore. They should want things, protect things, misunderstand the collapse, trade or threaten according to local conditions, and create practical choices for the player.

## Survival, Time, And Consequence

Survival pressure should come first from logistics: fuel, food, water, medicine, ammunition, tools, shelter, power, storage, time, and route risk. Bodily survival systems such as hunger, thirst, fatigue, pain, injury, exposure, sleep, infection, or temperature should arrive only when the game gives the player meaningful ways to respond.

Time should eventually connect local actions and overworld travel. Travel, scavenging, combat, searching, resting, repairing, waiting, and medical care should all be able to spend time. Time pressure should create tradeoffs, not just punish slow play.

Failure should be harsh but fair. The player should be able to read risks, recover from some setbacks, lose resources or platforms, and still continue when survival remains plausible.

## World Generation And Map Direction

The initial world direction is a scaled-down but logistically believable Colorado using real map names. Accuracy should prioritize logistics: road dependencies, distances, terrain, weather pressure, settlement spacing, fuel access, agriculture, industry, mountains, plains, and chokepoints.

The long-term map can grow toward a nationwide journey. Use a hybrid authored/procedural approach: real-world structure and authored key sites where they matter, with procedural variation for interiors, loot, encounters, depletion, and repeated site types.

Procedural generation should support believable places. It should not create arbitrary loot rooms or generic ruins when a site has a clear real-world purpose.

## UI Readability

The interface should make complex survival decisions understandable. The player should be able to inspect what matters: current travel method, fuel, time, inventory constraints, ammunition state, weapon state, health, local threats, nearby objects, action costs, and failure reasons.

Readability matters more than hidden realism. If a system affects player choice, the UI should expose enough state, feedback, and language for the player to reason about it.
