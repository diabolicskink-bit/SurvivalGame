# Task Log

Curated milestone history for current game state and architecture. Cleanup-only notes, one-off fixes, and detailed implementation churn are intentionally omitted unless they changed playable behavior, content, or important domain ownership.

## 2026-04-30 - Weapon Inventory Art

- Added generated transparent item sprite art for the 5.56 burst carbine and wired `carbine_556` to `item_carbine_556`.
- Replaced the hunting rifle and 9mm pistol item sprites with newly generated transparent inventory art.
- Replaced the AK-47 item sprite with a tight transparent cutout and added generated transparent item sprite art for the missing .22 rifle.
- Inventory grid items now render available item sprite art inside their occupied cells instead of only showing category-colored text boxes.

## 2026-04-30 - Local Map Mouse Wheel Zoom

- Local gameplay board mouse wheel zoom now switches between 18x12, 21x14, 27x18, 33x22, and 39x26 visible-tile viewports, with 27x18 remaining the default tactical view.
- Zoom is player-centered, visual/camera-only, and does not change local map state, movement, targeting, firearm range, accuracy, or line-of-fire rules.
- Local map rendering, hover/click conversion, layout sizing, and ground item sprite scale now follow the current zoom level.

## 2026-04-29 - Firearm Accuracy V1

- Added explicit per-weapon effective/max accuracy values and required per-mod accuracy bonuses to firearm JSON content.
- Rescaled current firearm ranges around the default 27x18 local viewport so pistols/shotguns are short-range, carbines/rifles cover most of the board, and scoped long guns can exceed a single default viewport.
- Targeted player firearm shots now consume ammunition/time after validation, roll hit or miss from modified range/accuracy stats, and apply damage only on hit.
- Weapon detail and inspect text now show base/modified accuracy and weapon mod accuracy effects.

## 2026-04-29 - Wider World Map Roads And Bend Joins

- World map roads now render at three times their previous lane-aware visual width.
- Road surfaces, casing, center dividers, and lane separators draw as connected bend-aware polylines with rounded joins, reducing visible clashes at corners.

## 2026-04-29 - Inventory Hover Text Width Fix

- Fixed inventory/equipment hover detail popups so item text keeps a stable readable width instead of wrapping into a narrow vertical column.
- Added minimum content sizing to the shared selected-item detail panel used by inventory hover details.

## 2026-04-29 - Travel Anchors, Cargo, And Fuel Containers

- Vehicle and pushbike local-site entry can place a matching travel anchor object; leaving a site with those travel methods requires returning adjacent to the active anchor.
- Campaign travel cargo persists stack and stateful items outside the player inventory, with adjacent-anchor take/stow actions that still respect inventory grid limits.
- Stateful `fuel_can` items hold up to 5.0 fuel, can be filled from non-depleting pump/drum fuel sources, and can pour partial fuel into the shared vehicle fuel state.
- Vehicle fuel transfer is fuel-can based; the legacy direct refuel action is not exposed as an available action.
- Local map data, world object data, item data, and tests cover the anchor/cargo/fuel-can loop.

## 2026-04-29 - Firearms, Fire Modes, Weapon Mods, And Line Of Fire

- Firearm data now covers 9mm pistol, AK-style rifle, .308 hunting rifle, 12 gauge shotgun, .22 rifle, and 5.56 burst carbine, with matching ammunition and feed devices.
- Weapons define range, accepted ammunition, feed compatibility, supported fire modes, and burst metadata where relevant.
- Stack-backed and stateful weapons preserve loaded feed state, inserted magazines, installed weapon mods, and current fire mode.
- Fire-mode toggling costs 0 ticks and is exposed only for weapons with more than one supported mode.
- Targeted shooting uses the equipped weapon's current mode: single shot consumes 1 round, deals modified ammunition damage, and costs 100 ticks; burst consumes 3 rounds, deals one deterministic 2x modified single-shot damage packet, and costs 150 ticks.
- Targeted shooting checks domain line of fire after target/range validation and before ammo, health, or time mutation. Sight-blocking structure edges and intermediate sight-blocking world objects block the shot with a clear message.
- Weapon mods are stateful item attachments with slot and weapon-family compatibility; current red dot, hunting scope, and match barrel mods can modify range and/or damage.
- Prototype Test Fire remains a one-round firearm action and does not use burst mode or line-of-fire.

## 2026-04-29 - Colorado World Map And Travel Layer

- The run starts on a data-backed scaled Colorado World Map with a tactical atlas background, generated terrain-cost grid, generated major-road geometry, curated city/town markers, landmark POIs, and local-site POIs.
- World Map travel supports walking, pushbike, and vehicle methods, smooth click-to-travel destinations, shared world time, vehicle fuel use, and first-pass road/terrain travel modifiers.
- Vehicle travel stops cleanly at zero fuel; walking and pushbike remain available without fuel.
- Route 18 Gas Station and Abandoned Farmhouse route to dedicated authored local sites; other current POIs fall back to the default prototype local site.
- Generated Colorado roads are simplified for overworld readability and render with lane-aware road bands while preserving route metadata for travel sampling.

## 2026-04-26 - Authored Local Sites, Structures, And Static World Objects

- Local site maps are authored JSON under `data/local_maps/`, including the default prototype site, Route 18 Gas Station, and Abandoned Farmhouse.
- Route 18 Gas Station is a 40x28 fixed site with forecourt, pump island, parking, convenience store interior, back room, restroom, blocked scenery, fuel pumps, and searchable store fixtures.
- Abandoned Farmhouse is a 64x44 fixed rural site with farmhouse rooms, verandah, rear utility yard, water tank area, shed/workshop, machinery yard, fenced paddock, rural surfaces, placed loot, and farm equipment.
- Edge-based structures model walls, doors, windows, fences, gates, and gaps on tile boundaries. Movement and line-of-fire rules query crossed edges instead of treating walls as tile objects.
- Static world objects support movement blocking, sight blocking, rectangular footprints, cardinal facing, visual-only sprite render metadata, and stable placement instance ids.
- Multi-tile objects such as vehicles, tanks, machinery, and large props occupy every data-defined footprint tile for collision, hover, and line-of-fire checks.

## 2026-04-26 - World Object Containers And Loot

- World-object definitions can declare searchable container profiles, while placements can define fixed loot and future loot table ids.
- Container runtime state is realized lazily per local site when searched, so unsearched containers remain placement/config data.
- Search Container and Take Container Item Stack actions go through the domain action pipeline, advance time, preserve remaining contents, and respect inventory grid capacity.
- The default prototype site, gas station fixtures, and farmhouse placements include first-pass searchable or fixed-loot examples.

## 2026-04-26 - NPCs, Targeting, And Automated Turret Hazard

- NPC content is JSON-backed with reusable definitions for name, species, tags, health, movement blocking, map color, optional sprite data, and simple behavior profile.
- Runtime NPCs have separate instance ids, positions, health, and blocking state.
- Clicking an NPC selects it as the current target and exposes Shoot through the action UI.
- Equipped firearm shots can damage NPCs, disabled NPCs visually grey out, and disabled automated turrets stop firing.
- Route 18 Gas Station has an automated turret NPC at tile 30,12. After successful time-costing local actions, automated turret behavior checks crossed 75-tick intervals, fires within 5 tiles, and deals 10 direct player health damage per shot.

## 2026-04-26 - Inventory, Equipment, Stateful Items, And Item UI

- Player inventory combines stack counts with a fixed 20x10 physical grid for grid-using items.
- Loose ammunition is grid-exempt and appears in a separate Ammo inventory mode; magazines/feed devices remain physical grid items.
- Equipment slots cover main hand, off hand, head, body, legs, feet, and back, with type-path validation for equippable items.
- Stateful items represent specific objects that must preserve identity or internal state, such as loaded magazines, equipped firearms, installed mods, backpacks-with-contents, travel cargo contents, and fuel cans.
- Supported item locations include player inventory, equipment, ground at a local site, inserted into another item, contained inside another item, and campaign travel cargo.
- Inventory and equipment UI support hover details plus right-click contextual actions routed through domain action requests.

## 2026-04-26 - Local Map Rendering And Visual Readability

- Local gameplay renders through a clipped tile viewport over full local map state; larger maps clamp near edges and smaller maps are centered with padding.
- Map rendering and hover details include surfaces, ground item markers, stateful ground items, edge-based structures, world objects, NPCs, and the player marker.
- A single Y-sorted entity layer draws world objects, NPCs, and the player; visual sprite overflow does not change collision, hover, targeting, or placement ownership.
- Surface sprites exist for early prototype terrain such as grass, carpet, concrete, ceramic tile, and ice, with fallback map colors for newer surfaces.
- Transparent gameplay sprites are wired for key items/world objects/NPCs, including weapons, fuel pump/store fixtures, tree, boulder, abandoned vehicle, automated turret, single bed, storage crate, tractor wreck, and farm trailer.

## 2026-04-26 - Campaign State, Action Pipeline, And Time

- `CampaignState` owns the persistent run state: shared world time, player state, stateful items, world map travel, vehicle fuel, active mode/site id, registered local sites, travel cargo, and local site preservation.
- Each `LocalSiteState` keeps its own local map, ground items, world objects, NPC roster, container state, display metadata, entry position, and last local player position across world-map/local transitions.
- The domain action pipeline dispatches movement, inventory, equipment, inspection, firearm, interaction, travel cargo, fuel, and world-object container actions through handlers.
- Successful local actions advance the shared tick clock according to action cost; failed actions generally cost 0 ticks.
- Current important local tick costs include: Move 100, Wait 100, Pick Up 50, Take from Container 50, Search Container default 75, ammo load 10 per round, feed insert/remove 25, fire-mode toggle 0, single shot 100, burst shot 150, and vehicle refuel 100.

## 2026-04-25 - Local Gameplay Foundation

- The playable prototype has a Godot main menu, New Run flow, local gameplay shell, grid movement, player marker, message log, status panels, and Escape/menu flow.
- Domain state owns grid bounds, player position, surfaces, ground items, inventory, equipment, vitals, world time, local map state, NPCs, structures, world objects, firearms, and action validation/resolution.
- Local movement uses discrete tile steps, boundary checks, surface display, structure-edge blocking, world-object blocking, and NPC blocking.
- Player vitals currently track health, hunger, thirst, fatigue, sleep debt, pain, and body temperature; only direct prototype health damage is simulated.

## 2026-04-24 - Repository And Domain Foundation

- Godot-facing scenes/scripts live under `src/Godot/`; plain C# simulation/domain code lives under `src/SurvivalGame.Domain/`; automated domain tests live under `tests/SurvivalGame.Domain.Tests/`.
- Runtime content data is JSON-backed under `data/`, including items, firearms, surfaces, world objects, structures, NPCs, local maps, and world map definitions.
- Core domain primitives include grid bounds/positions, item ids/definitions/catalogs, type paths, inventory containers, equipment slots, local maps, campaign state, world map travel, and action requests/results.
