# Current Scope

## Included

- Main menu with title, subtitle, New Run, and Quit actions.
- Gameplay shell scene with a simple placeholder top-down grid.
- Resizable gameplay layout that places the world board in the top-left half-width, top two-thirds-height area and keeps the side panel fitted to the right.
- Player marker placed near the centre of the grid.
- Discrete tile movement with WASD and arrow keys.
- Boundary checks that prevent movement outside the map.
- UI overlay showing mode, turn count, and player position.
- Central domain action pipeline for movement, wait, and pickup.
- Clickable Wait action button.
- Clickable Pick Up action button when the player is standing on item stacks.
- Message log with startup and movement messages.
- Escape key return from gameplay shell to main menu.
- Domain-level player inventory tracking by item id and quantity.
- Read-only gameplay UI section showing the player's held inventory items.
- JSON-backed prototype item definitions under `data/items/`.
- Item definitions include id, name, description, category, tags, stack size, weight, icon id, sprite id, and future action ids.
- Nested type paths are derived from category and tags, such as `Weapon -> gun -> rifle -> ak47`.
- Prototype starting inventory items for display testing.
- Prototype item stacks placed on a few map tiles and rendered as simple markers.
- Hover tooltip for tiles, showing surface details and any item names, quantities, and descriptions.
- JSON-backed prototype terrain/surface definitions under `data/surfaces/`.
- Surface definitions include id, name, description, category, tags, movement cost, map color, and optional sprite id.
- Surface tags are the current lightweight property mechanism, for example ice has a `slippery` tag.
- Prototype map surface layout using grass, carpet, concrete, tile, and ice.
- Generated sprite assets for grass, carpet, concrete, ceramic tile, and ice surfaces.
- UI overlay shows the surface beneath the player.
- Domain-level player vital tracking for health, hunger, thirst, fatigue, sleep debt, pain, and body temperature.
- Read-only gameplay UI section showing the player's tracked vitals.
- Domain-level player equipment loadout with slots for main hand, off hand, head, body, legs, feet, and back.
- Equipment slot definitions validate accepted `ItemTypePath` values.
- JSON-backed prototype static world object definitions under `data/world_objects/`.
- Ten common world objects: wall, tree, fridge, wooden door, window, table, chair, bed, storage crate, and boulder.
- Prototype world object placement on the map with simple rendering.
- Generated sprite assets for fridge, bed, and storage crate.
- Movement collision against world objects marked as blocking movement.
- Hover tooltip shows world object details when a tile contains one.

## Not Included Yet

- Opening, closing, moving, destroying, building, using, searching, or looting world objects.
- Container contents for fridges, crates, or other objects.
- Damage, healing, death, or health effects.
- Hunger, thirst, fatigue, sleep, pain, or body temperature simulation rules.
- Surface-driven movement effects such as sliding on ice.
- Item drop, use, equip, or inspect actions.
- Equip and unequip actions.
- Equipment UI.
- Equipment item effects or stat modifiers.
- Item effects, durability, damage, ammo, or behavior.
- Inventory weight, capacity, containers, or equipment containers.
- Hunger, thirst, fatigue, or other survival meters.
- Enemies, NPCs, or combat.
- Procedural generation.
- Crafting.
- Saving or loading.
- Pathfinding.
- Injuries or health systems.
- Weather.
- World simulation.
- Runtime content editing or mod loading.
