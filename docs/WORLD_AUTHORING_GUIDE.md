# World Authoring Guide

AI-facing companion to `docs/BACKGROUND.md` for content, tone, naming, site, item, NPC, faction-seed, automated-system, environmental-clue, and sprite-related work.

This guide is inspiration only. It never authorizes new mechanics, new systems, new content scope, or implementation work by itself. Current implementation scope still lives in `docs/CURRENT_SCOPE.md`; long-term system direction lives in `docs/DESIGN_GOALS.md`.

## When To Use This

Read this with `docs/BACKGROUND.md` when a task touches:

- Local site concepts, map dressing, loot logic, or environmental storytelling.
- Items, world objects, surfaces, structures, sprites, names, descriptions, categories, or tags.
- NPC definitions, faction seeds, rumors, logs, automated messages, or tone.
- Automated systems such as cameras, locks, pumps, turrets, gates, alerts, and continuity protocols.

Do not use this guide to add unrequested crafting, combat expansion, NPC AI, factions, weather, procedural generation, saving, or survival simulation.

## Authoring Rules

- Start from the current scope, not from the full future fantasy.
- Prefer practical survival questions over generic apocalypse flavor.
- Make content specific enough to support data ids, display names, map colors, footprints, and player-facing text.
- Keep Colorado and US infrastructure useful, grounded, and local.
- Make failures readable: locked, guarded, depleted, powered, broken, contested, stale, or misdirected.
- Favor clues the player can inspect over lore exposition.
- Preserve ambiguity around The Failover. Show consequences before causes.
- Use restrained, physical language. Avoid cartoon wasteland defaults.

## Content Decision Rules

Priority order for future world/content decisions:

1. Preserve current scope unless the user explicitly asks to expand it.
2. Prefer the choice that creates survival logistics, tactical risk, or route pressure.
3. Prefer the choice that fits Colorado geography and US infrastructure.
4. Prefer believable access problems over arbitrary scarcity.
5. Prefer specific sites, items, and objects over generic apocalypse props.
6. Prefer systems the player can understand and act on over hidden realism.

When adding or revising a site, define:

- why it existed before the collapse
- why the player cares now
- the obvious resource types
- the likely risks
- what systems may still have power
- what is locked, blocked, guarded, or depleted
- what the player can learn by looking around
- how the site connects to travel, combat, survival, or mobile-base needs

When adding items or objects, prefer specific, physically believable concepts over generic ones. Fuel pumps, doors, fridges, generators, shelves, cabinets, workbenches, tanks, gates, radios, and locks should eventually become interaction anchors when matching systems exist.

When an explicitly requested mechanic touches world content, prefer domain-owned state, Godot as presentation/input, action-pipeline resolution, clear failure messages, no silent mutation on failed actions, and focused vertical slices that preserve current scope.

Useful comparison:

- Strong: a service station has fuel in underground tanks, but the pump circuit is powered by a locked system and a nearby automated camera still treats the forecourt as protected property.
- Weak: a service station has random loot, generic enemies, and no reason its resources are present or absent.
- Strong: a mountain pass is technically open, but abandoned vehicles, weather, low fuel, and a hostile checkpoint make the route a decision.
- Weak: a road is blocked only because the level needs a wall.

## Local Site Concept Template

Use this before adding or revising a local site.

```text
Site id/name:
Real geography or fictional local name:
Pre-Failover purpose:
Why the player cares now:
Primary survival question:
Obvious resources:
Likely risks:
Powered systems:
Locked, blocked, guarded, depleted, or contested:
Environmental clues:
Current mechanics it can use:
Mechanics intentionally not added:
```

Good site concepts should explain why resources are present, why access is not trivial, and what the player can understand from the place itself.

## Automated System Or Hazard Template

Use this for cameras, locks, pumps, turrets, gates, alerts, warning systems, and other misaligned infrastructure.

```text
System id/name:
Original mandate:
Current mistaken assumption:
Trigger condition:
Warning or readable tell:
Escalation:
Player counterplay using current mechanics:
Reward or access problem:
Failure message tone:
Mechanics intentionally not added:
```

Automated systems should feel procedural and misaligned, not malicious. They are doing a job nobody can currently correct.

## NPC Or Faction Seed Template

Use this for NPC definitions, later faction notes, rumors, or social content.

```text
NPC/faction seed name:
Role before the Failover:
Role now:
What they want:
What they control or know:
What they misunderstand:
What they might trade or ask for:
What they would fight over:
How they describe The Failover:
Current mechanics it can use:
Mechanics intentionally not added:
```

NPCs should create practical choices. Avoid making them only exposition sources.

## Item Or World Object Concept Template

Use this before adding items, surfaces, structures, world objects, or sprites.

```text
Content id:
Display name:
Category:
Pre-Failover use:
Why it matters now:
Map readability role:
Movement or sight blocking:
Footprint or edge placement:
Likely tags/actions:
Player-facing description:
Current mechanics it can use:
Mechanics intentionally not added:
```

Prefer specific concepts over generic props. A `fuel_pump`, `single_bed`, `water_tank`, or `tractor_wreck` is usually stronger than a vague "machine" or "debris" entry.

## Environmental Clue, Log, Or Message Template

Use this for inspect text, map details, automated messages, logs, signs, and rumors.

```text
Clue/message type:
Speaker or source:
What it literally says or shows:
What it implies:
What it gets wrong or cannot know:
Nearby gameplay object or risk:
Tone notes:
Private truth it hints at, if any:
What not to reveal:
```

Clues should be local, partial, and useful. They should help the player understand a place, a risk, or a resource without explaining the whole collapse.

## Maintaining World Guidance

Use this guide as the operational companion to `docs/BACKGROUND.md`.

Update `docs/BACKGROUND.md` when work creates or changes durable setting, tone, Failover terminology, Colorado/site-family guidance, threat guidance, faction seeds, or private consistency guidance.

Update this guide when work creates or changes reusable AI-facing templates, naming/content rules, authoring checks, maintenance rules, or other practical guidance for future content work.

Do not update the world guidance docs for routine bug fixes, tests, formatting, temporary plans, abandoned ideas, pure architecture work, or one-off content that does not establish a reusable world pattern.

Keep private truth clearly labeled and avoid turning it into player-facing exposition. Prefer links over duplicated guidance. Add a `docs/TASK_LOG.md` entry only when the change is durable project memory under the task-log admission rule.

## Quick Checks

Before finishing content work, verify:

- The change fits the user request and `docs/CURRENT_SCOPE.md`.
- Names, ids, descriptions, and sprites point to the same concrete concept.
- The content creates a survival, travel, tactical, or access question.
- The player has enough feedback to understand the risk or failure.
- Private Failover truth remains private unless the task explicitly asks for public lore.
