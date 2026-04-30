# Task Log

Curated milestone history for current game state and architecture. This log answers what important player-facing, content, or architecture state changed, and when.

## How To Use This Log

- Read this for milestone history and durable project memory.
- Use `docs/CURRENT_SCOPE.md` for exact current included and not-included scope.
- Use `docs/ARCHITECTURAL_DEBT.md` for future work and known architecture pressure.
- Use `docs/MECHANICS_BACKLOG.md` for deferred player-facing mechanics and systems.
- Use Git commits and tests for full implementation detail.

## Admission Rule

- Add entries for player-facing behavior, content/data milestones, architecture ownership changes, resolved or created major tracker systems, or significant visual/content asset work.
- Skip routine bug fixes, tiny cleanup, pure investigations, plans, and review-only notes unless they change durable project state.
- Keep entries to 2-4 bullets focused on what is now true. Include preserved scope or non-changes only when they prevent likely confusion.

## 2026-04-30 - Tile-Wall Render Model Extraction

- Changed: Extracted procedural tile-wall kind detection, neighbor masks, render bounds, floor-contact sorting, orientation, and 2.5D geometry into Godot-local `TileWallRenderModel`.
- Changed: `MapEntityLayer` now consumes prepared tile-wall render data while keeping the existing draw calls for wall faces, windows, wooden doors, and glass doors.
- Preserved: Local-map JSON, collision, hover/tooltips, edge-based fence rendering, and current prototype/gas/farmhouse wall visuals stayed unchanged.

## 2026-04-30 - World Background Docs Consolidation

- Changed: Retitled `docs/BACKGROUND.md` to World Background and kept it focused on setting, tone, Failover guidance, Colorado anchors, site families, threats, and faction seeds.
- Changed: Moved reusable content decision rules and world-guidance maintenance rules into `docs/WORLD_AUTHORING_GUIDE.md`.
- Changed: Removed the separate world-doc maintenance file because it duplicated broader documentation governance.
- Preserved: World background docs remain inspirational guidance only; they do not authorize new mechanics, architecture changes, or scope expansion.

## 2026-04-30 - 2.5D Tile-Wall Architecture Direction

- Changed: Promoted procedural 2.5D tile-object walls as the near-term building-wall direction in architecture guidance and tracker docs.
- Changed: Split remaining wall architecture work into logical tracker slices: tile-wall render model extraction, edge-based building wall retirement, future Godot TileMap/Terrain compatibility, and remaining fence/gap edge semantics.
- Changed: Added mechanics backlog slices for 2.5D fence/gate/gap visuals and tile-wall material variants.
- Preserved: Current paddock fences, gates, and broken fence gaps remain edge structures until their representation is deliberately revisited.

## 2026-04-30 - Tracker Split Planning Rule

- Changed: Updated `ARCH-*` and `MECH-*` guidance so implementation planning can identify an item as too broad and propose smaller replacement items before work begins.
- Changed: Added stable-ID handling for accepted splits: create new items and either narrow or supersede the original with links to the replacements.
- Preserved: Split proposals remain planning/triage only and do not authorize architecture refactors or mechanics by themselves.

## 2026-04-30 - Tracker Notes Fields

- Changed: Added `Notes` fields to every `ARCH-*` and `MECH-*` tracker item so useful implementation context can be preserved during ordinary work.
- Changed: Updated tracker and AI guidance so future sessions append dated notes when there is real information to record and update canonical fields when learning changes priority, direction, dependencies, next action, or completion signals.
- Preserved: Notes are tracker memory only; they do not authorize architecture refactors or new mechanics.

## 2026-04-30 - Farmhouse Tile-Wall Visual Conversion

- Changed: Converted Abandoned Farmhouse farmhouse and shed building walls/windows from edge structures to legacy tile-world-object walls so they render with the procedural 2.5D tile-wall renderer.
- Changed: Left passable farmhouse/shed doorways as open gaps for this visual pass and kept paddock fencing, gates, and broken fence gaps as edge structures.
- Preserved: Authored map schema, existing wall sprite assets, farm object content, and current movement/collision systems stayed unchanged.

## 2026-04-30 - Mechanics Backlog Tracker

- Changed: Added `docs/MECHANICS_BACKLOG.md` as a living tracker for deferred player-facing mechanics and systems with stable `MECH-*` IDs.
- Changed: Seeded the backlog with 10 single-system entries from current scope and design docs, including cover shooting modifiers, drag-to-move inventory, item rotation, recipe-backed local map generation, thirst, hunger, fatigue, save/load snapshots, basic hostile NPC behavior, and vehicle cargo capacity.
- Changed: Updated AI guidance so future sessions auto-capture meaningful mechanics that are identified but intentionally excluded from current implementation plans.
- Preserved: The backlog is memory and triage only; it does not add implemented scope.

## 2026-04-30 - Living Background Docs V1

- Changed: `docs/BACKGROUND.md` now acts as the readable hub for a lightweight living world guidance system.
- Changed: Added `docs/WORLD_AUTHORING_GUIDE.md` for AI-facing content templates and practical authoring rules.
- Preserved: Background and world-authoring docs remain inspirational guidance only; `docs/CURRENT_SCOPE.md` and `docs/DESIGN_GOALS.md` still control current scope and long-term system direction.

## 2026-04-30 - Living Design Goals Guidance

- Changed: `docs/DESIGN_GOALS.md` now acts as a living end-state design document for durable long-term direction, not a static north-star note.
- Changed: Added AI planning guidance for high-impact alternatives, early-slice end-state questions, durable design-goal updates, and local exceptions.
- Preserved: Current scope still lives in `docs/CURRENT_SCOPE.md`, while future architecture and system-work pressure stays in `docs/ARCHITECTURAL_DEBT.md`.

## 2026-04-30 - Application Session Boundary

- Added `SurvivalGame.Application` as a non-Godot application layer for content path wiring, catalog loading, campaign/session creation, starting item seeding, action pipeline setup, and local site session packaging.
- Replaced the Godot-side prototype session factory with a thin `GodotSessionFactory` adapter that only resolves `res://data` and delegates to the application layer.
- Added application tests that create sessions from ordinary filesystem data paths, verify committed local sites and starting items, and cover standalone local-site creation plus unknown-POI fallback.
- Archived `ARCH-1` as resolved in the architectural debt tracker.

## 2026-04-30 - Task Log Curation Rules

- Changed: Added explicit task-log usage and admission rules so future entries stay curated rather than becoming a full changelog.
- Changed: Tightened `AGENTS.md` expectations so the task log is considered after tasks and sweeps, but updated only when durable project state changed.
- Preserved: Git remains the detailed change history; `CURRENT_SCOPE.md` and `ARCHITECTURAL_DEBT.md` remain the sources of truth for current scope and future architecture work.

## 2026-04-30 - Tile Wall Coverage Cleanup

- Changed: Made procedural 2.5D tile-wall geometry cover the full wall-owned tile footprint so underlying floor/grid tiles do not show around exposed wall edges.
- Changed: Kept small connected-edge overlap and removed seam-prone drop shadows and outline strokes.
- Preserved: Tile-wall collision, passable glass door behavior, authored map data, sprite assets, and farmhouse edge-structure rendering stayed unchanged.

## 2026-04-30 - Low-Rise 2.5D Tile Walls

- Changed: Replaced legacy tile-wall object rendering with low-rise procedural 2.5D walls, wooden doors, windows, and glass doors.
- Changed: Expanded tile-wall render bounds so raised wall faces do not cull early at local viewport edges.
- Preserved: Existing collision, hover data, authored maps, sprite assets, and farmhouse edge-structure rendering stayed unchanged.

## 2026-04-30 - Architectural Debt Tracker

- Changed: Added `docs/ARCHITECTURAL_DEBT.md` as the living architecture debt index with stable `ARCH-*` IDs, priorities, statuses, guardrails, and resolution signals.
- Changed: Migrated the useful architecture roadmap work into the tracker and removed stale one-off planning docs.
- Changed: Updated project guidance so sweeps, refactors, and architecture work check and maintain the tracker.
- Preserved: The tracker records future architecture pressure only; it does not authorize new gameplay scope.

## 2026-04-30 - Weapon Inventory Art

- Changed: Added or replaced transparent item sprites for the 5.56 carbine, hunting rifle, 9mm pistol, AK-style rifle, and .22 rifle.
- Changed: Inventory grid items now render available item sprite art inside occupied cells instead of only category-colored text boxes.
- Preserved: Sprite work stayed tied to existing item ids and gameplay-readable inventory presentation.

## 2026-04-30 - Local Map Mouse Wheel Zoom

- Changed: Local gameplay now supports mouse-wheel viewport zoom levels of 18x12, 21x14, 27x18, 33x22, and 39x26 visible tiles.
- Changed: Rendering, hover/click conversion, layout sizing, and ground item sprite scale follow the current zoom level.
- Preserved: Zoom is visual and camera-only; it does not change local map state, movement, targeting, firearm range, accuracy, or line-of-fire rules.

## 2026-04-29 - Firearm Accuracy V1

- Changed: Firearm data now defines explicit effective/max accuracy values, and weapon mods define accuracy bonuses.
- Changed: Targeted firearm shots consume ammunition and time after validation, roll hit or miss from modified range/accuracy stats, and apply damage only on hit.
- Changed: Weapon detail and inspect text show base and modified accuracy plus weapon mod accuracy effects.
- Preserved: Firearm ranges remain scaled around the default 27x18 local viewport.

## 2026-04-29 - Wider World Map Roads And Bend Joins

- Changed: World map roads render wider than before with lane-aware casing, surfaces, center dividers, and lane separators.
- Changed: Road polylines use bend-aware rounded joins to reduce visual clashes at corners.

## 2026-04-29 - Inventory Hover Text Width Fix

- Changed: Inventory and equipment hover detail popups keep a stable readable width instead of wrapping into narrow columns.
- Changed: The shared selected-item detail panel has minimum content sizing for hover details.

## 2026-04-29 - Travel Anchors, Cargo, And Fuel Containers

- Changed: Vehicle and pushbike site entries place matching local travel anchors, and leaving with those methods requires returning adjacent to the active anchor.
- Changed: Campaign travel cargo persists stack and stateful items outside the player inventory, with adjacent-anchor take/stow actions that still respect inventory grid limits.
- Changed: Stateful `fuel_can` items hold fuel, fill from non-depleting fuel sources, and pour partial fuel into the shared vehicle fuel state.
- Preserved: Vehicle fuel transfer is fuel-can based; the legacy direct refuel action is not exposed as an available action.

## 2026-04-29 - Firearms, Fire Modes, Weapon Mods, And Line Of Fire

- Changed: Firearm content covers current prototype weapons, ammunition, feed devices, fire modes, ranges, compatibility, burst metadata, and stateful weapon mods.
- Changed: Stack-backed and stateful weapons preserve loaded feed state, inserted magazines, installed mods, and current fire mode.
- Changed: Targeted shooting uses the equipped weapon's current mode, validates line of fire before ammo/time mutation, and supports single-shot and burst outcomes.
- Preserved: Prototype Test Fire remains a one-round action and does not use burst mode or line-of-fire.

## 2026-04-29 - Colorado World Map And Travel Layer

- Changed: New runs start on a data-backed scaled Colorado world map with atlas background, terrain-cost grid, major-road geometry, city/town markers, landmark POIs, and local-site POIs.
- Changed: World map travel supports walking, pushbike, and vehicle methods with smooth click destinations, shared world time, vehicle fuel use, and first-pass road/terrain modifiers.
- Changed: Route 18 Gas Station and Abandoned Farmhouse route to dedicated authored local sites; other current POIs fall back to the default prototype site.
- Preserved: Generated Colorado roads are simplified for overworld readability while preserving route metadata for travel sampling.

## 2026-04-26 - Authored Local Sites, Structures, And Static World Objects

- Changed: Local site maps are authored JSON under `data/local_maps/`, including the default prototype site, Route 18 Gas Station, and Abandoned Farmhouse.
- Changed: Edge-based structures model walls, doors, windows, fences, gates, and gaps on tile boundaries; movement and line-of-fire query crossed edges.
- Changed: Static world objects support movement/sight blocking, rectangular footprints, facing, visual-only sprite render metadata, and stable placement instance ids.
- Preserved: Multi-tile objects occupy every data-defined footprint tile for collision, hover, and line-of-fire checks.

## 2026-04-26 - World Object Containers And Loot

- Changed: World-object definitions can declare searchable container profiles, while placements can define fixed loot and future loot table ids.
- Changed: Container runtime state is realized lazily per local site when searched, so unsearched containers remain placement/config data.
- Changed: Search and take actions go through the domain action pipeline, advance time, preserve remaining contents, and respect inventory grid capacity.

## 2026-04-26 - NPCs, Targeting, And Automated Turret Hazard

- Changed: NPC content is JSON-backed with reusable definitions, while runtime NPCs have separate instance ids, positions, health, and blocking state.
- Changed: Clicking an NPC selects it as the current target and exposes Shoot through the action UI.
- Changed: Equipped firearm shots can damage NPCs; disabled NPCs visually grey out and disabled automated turrets stop firing.
- Changed: Route 18 Gas Station includes an automated turret hazard that fires after crossed time intervals while the player is within range.

## 2026-04-26 - Inventory, Equipment, Stateful Items, And Item UI

- Changed: Player inventory combines stack counts with a fixed physical grid, while loose ammunition is grid-exempt and shown in a separate Ammo mode.
- Changed: Equipment slots cover current body/hand/back slots with type-path validation for equippable items.
- Changed: Stateful items preserve identity and internal state for loaded magazines, equipped firearms, installed mods, backpacks-with-contents, travel cargo contents, and fuel cans.
- Changed: Inventory and equipment UI support hover details plus right-click contextual actions routed through domain action requests.

## 2026-04-26 - Local Map Rendering And Visual Readability

- Changed: Local gameplay renders a clipped tile viewport over full local map state, including padding for maps smaller than the viewport.
- Changed: Map rendering and hover details include surfaces, ground item markers, stateful ground items, edge structures, world objects, NPCs, and the player marker.
- Changed: A single Y-sorted entity layer draws world objects, NPCs, and the player while sprite overflow remains visual-only.
- Changed: Key item, world object, NPC, and surface sprite assets are wired into the local map with fallback map colors where needed.

## 2026-04-26 - Campaign State, Action Pipeline, And Time

- Changed: `CampaignState` owns persistent run state, including shared time, player state, stateful items, world map travel, vehicle fuel, mode/site id, local sites, and travel cargo.
- Changed: `LocalSiteState` preserves each site's map, ground items, world objects, NPC roster, container state, display metadata, entry position, and last player position.
- Changed: The domain action pipeline dispatches movement, inventory, equipment, inspection, firearm, interaction, travel cargo, fuel, and container actions through handlers.
- Changed: Successful local actions advance shared ticks according to action cost, while failed actions generally cost 0 ticks.

## 2026-04-25 - Local Gameplay Foundation

- Changed: The playable prototype has a Godot main menu, New Run flow, local gameplay shell, grid movement, player marker, message log, status panels, and Escape/menu flow.
- Changed: Domain state owns grid bounds, player position, surfaces, ground items, inventory, equipment, vitals, world time, local map state, NPCs, structures, world objects, firearms, and action validation/resolution.
- Changed: Local movement uses discrete tile steps with boundary checks, surface display, structure-edge blocking, world-object blocking, and NPC blocking.
- Preserved: Player vitals exist as tracked state, but only direct prototype health damage is simulated.

## 2026-04-24 - Repository And Domain Foundation

- Changed: Godot-facing scenes/scripts live under `src/Godot/`; plain C# simulation/domain code lives under `src/SurvivalGame.Domain/`; automated domain tests live under `tests/SurvivalGame.Domain.Tests/`.
- Changed: Runtime content data is JSON-backed under `data/`, including items, firearms, surfaces, world objects, structures, NPCs, local maps, and world map definitions.
- Changed: Core domain primitives include grid bounds/positions, item ids/definitions/catalogs, type paths, inventory containers, equipment slots, local maps, campaign state, world map travel, and action requests/results.
