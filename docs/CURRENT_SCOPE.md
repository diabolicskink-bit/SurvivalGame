# Current Scope

## Included

- Main menu with title, subtitle, New Run, and Quit actions.
- Gameplay shell scene with a simple placeholder top-down grid.
- Resizable gameplay layout that keeps the board and side panel fitted to the current viewport.
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
- Hover tooltip for tiles containing item stacks, showing item names, quantities, and descriptions.
- JSON-backed prototype terrain/surface definitions under `data/surfaces/`.
- Surface definitions include id, name, description, category, tags, movement cost, and map color.
- Surface tags are the current lightweight property mechanism, for example ice has a `slippery` tag.
- Prototype map surface layout using grass, carpet, concrete, tile, and ice.

## Not Included Yet

- Surface-driven movement effects such as sliding on ice.
- Item drop, use, equip, or inspect actions.
- Item effects, durability, damage, ammo, or behavior.
- Inventory weight, capacity, containers, or equipment slots.
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
