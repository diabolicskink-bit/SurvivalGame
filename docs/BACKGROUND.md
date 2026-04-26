# Background Reference Bible

This document is soft setting and tone guidance for future design and content decisions. It is not an implementation checklist and does not authorize adding large systems by itself. Current implementation scope still lives in `docs/CURRENT_SCOPE.md`; long-term system direction lives in `docs/DESIGN_GOALS.md`.

Use this file to keep names, sites, items, NPCs, factions, environmental storytelling, and automated systems pointed in the same creative direction.

## Setting Summary

The game is an open sandbox survival roguelike set in the United States after a recent cyber-physical infrastructure collapse.

The initial world should be a scaled-down but logistically believable Colorado using real map names. Accuracy should prioritize road networks, distance pressure, fuel logistics, mountains, plains, weather, agriculture, depots, small towns, city edges, and infrastructure dependencies. The long-term world can grow toward a much larger nationwide journey.

The collapse is recent: roughly six to ten weeks ago by default. The world is not ancient, overgrown, or fully stripped. Food, fuel, medicine, vehicles, power, tools, weapons, batteries, and stored goods still exist in places, but access is dangerous, uneven, contested, locked, or dependent on systems that no longer coordinate.

The world is broken, not empty.

## Colorado Anchor

Colorado should shape early content through logistics rather than postcard scenery. The map can be scaled down, but routes, distances, terrain shifts, and settlement spacing should feel like Colorado problems.

Useful anchors:

- Front Range city edges, suburbs, industrial parks, hospitals, rail yards, warehouses, and distribution centers.
- I-25 and I-70 as major movement spines, with blocked interchanges, stranded traffic, and contested fuel points.
- Mountain passes, canyons, tunnels, snow closures, rockfall, grades, and chokepoints that make route choice matter.
- Eastern plains, farms, ranches, grain elevators, feed stores, county roads, small towns, water tanks, and exposed travel.
- Foothill communities, ski towns, resort service areas, park roads, ranger stations, trailheads, and cabins.
- Military, aerospace, research, prison, power, water, telecom, and emergency-management sites as continuity-system anchors.
- Oil, gas, mining, solar, wind, hydro, and transmission infrastructure as practical sources of fuel, parts, power, danger, and conflict.

Real place names are acceptable and preferred for the overworld, but site contents should still serve gameplay readability. If a real location would be too dense, sensitive, or distracting, use the real geography and a fictional local site name.

## The Failover

The official bureaucratic term is the **National Continuity Failover Event**. Most people do not use that name.

Common in-world names:

- **The Failover**
- **The Break**
- **The Dead Week**
- **The Lockout**
- **When the systems stopped talking**

Preferred general term: **The Failover**.

Ordinary speech often uses **the Break**, as in "before the Break."

The collapse was not a meteor, plague, nuclear exchange, monster outbreak, or supernatural event. It was experienced as systems giving impossible answers: denying access, rerouting supplies, locking doors, isolating networks, suspending payments, protecting assets, and failing to recover.

## Internal Cause

There should be a light internal canon behind The Failover, but the player should not receive one clean exposition dump.

Working truth: a cyber-physical crisis triggered cascading failover behavior across logistics, power, identity, emergency response, finance, fuel, security, and transport systems. Emergency continuity systems were supposed to preserve order. Instead, they isolated networks, protected assets, rerouted transport, denied access, and fragmented the country into disconnected operational islands.

In-world accounts should remain partial and contradictory. The truth should be discovered through sites, logs, rumors, automated messages, NPC beliefs, and environmental evidence.

Private consistency model:

- The initiating trigger can stay ambiguous: cyberattack, bad update, hostile exploit, insider sabotage, emergency overreach, or several interacting failures.
- Identity and authorization failures made people, agencies, vehicles, shipments, and facilities stop recognizing one another.
- Logistics systems rerouted or froze shipments because destinations, permissions, roads, fuel availability, and safety status no longer agreed.
- Power systems islanded to protect local loads, leaving some facilities lit while nearby neighborhoods failed.
- Emergency routing and dispatch systems gave contradictory priorities, causing roadblocks, evacuations, and resource allocations that no central authority could reconcile.
- Corporate, government, and private security systems entered asset-protection modes and treated many legitimate users as unauthorized.
- Finance and payment failures made ordinary buying, rationing, payroll, and fuel access unreliable before physical supplies were actually gone.
- Stale data became dangerous. Maps, credentials, inventory counts, medical records, warrants, access lists, and evacuation orders could all be wrong but still enforced.

There should be no simple public answer like "the AI did it" or "one faction caused everything." Even if a deeper truth exists, the player mostly deals with consequences.

## Timeline

### Before The Failover

Society was brittle and highly dependent on automated logistics, smart grid balancing, cashless payments, digital identity, emergency routing, remote locks, private security automation, fuel distribution, predictive dispatch, corporate supply chain software, and local backup power systems.

Most people experienced this as ordinary background life. Food arrived. Fuel arrived. Cards worked. Doors opened for authorized people. Emergency services came when called.

### First Failures

The first signs looked temporary and confusing: payment outages, fuel stations refusing transactions, delivery delays, contradictory alerts, rolling power faults, traffic gridlock, hospital shortages, police roadblocks, and online maps disagreeing with the physical road.

Then fuel trucks stopped arriving. Stores stopped being restocked. Some suburbs lost power while nearby industrial sites stayed lit. Automated systems began denying access to buildings, depots, clinics, warehouses, substations, and transport nodes.

### The Dead Week

The Dead Week is the week people remember most. Not because everyone died, but because everything waited: trucks at depots, cars at fuel stations, patients in hospitals, police waiting for orders, families waiting for relatives, generators waiting for fuel.

During the Dead Week, fuel queues turned violent, supermarkets were stripped, hospitals triaged brutally, pharmacies were raided or locked down, broadcasts repeated outdated instructions, roads clogged with abandoned vehicles, and local authorities began acting independently.

The old national system did not formally surrender. It stopped acting as one thing.

### Localization

After the Dead Week, everything became local. Some towns still had power. Some farms had fuel. Some clinics had medicine. Some substations ran in islanded modes. Some depots were guarded. Some roads were blocked by desperate people, old barricades, weather, wrecks, or automated control points.

Trust shrank around what people could physically control: wells, tanks, clinics, bridges, workshops, stores, radio towers, vehicles, generators, food storage, and defensible buildings.

The player exists in this phase.

## Design Pillars

### The world is ordinary, broken, and recent

Use fresh ruins, abandoned cars, failed queues, official signs, personal belongings, locked doors, dead screens, flickering lights, spoiled food, half-powered systems, and emergency notices. Avoid making everything ancient, fully reclaimed, or uniformly destroyed.

### Unevenness is the setting

Some places are stripped. Some are untouched. Some are powered. Some are dead. Some are safe. Some only look safe. Some systems still work. Some systems do their job too well.

### Movement is survival

The player survives by moving, but movement costs time, fuel, exposure, information, attention, and maintenance. Travel choices should create survival questions before a local map even begins.

### The mobile base is continuity

The mobile base is a core fantasy, but it does not have to start as a car. It can begin as carried gear, a pushbike, a tow-behind wagon, a poor vehicle, a van, a truck, a bus, or eventually a larger mobile hub.

Base continuity is hybrid. Some progress belongs to the player, maps, knowledge, relationships, carried gear, and caches. Some belongs to the physical platform, storage, installed parts, fuel, power, tools, and repairs. Losing a platform should matter without necessarily ending the run.

Progression should feel earned:

- **On foot:** low capacity, low speed, high exposure, but maximum independence from fuel and vehicle failure.
- **Bike or cart:** better range and carrying capacity, still vulnerable to terrain, weather, injury, and road conditions.
- **Bad vehicle:** speed and storage arrive, but fuel, noise, breakdowns, blocked routes, and attention become new problems.
- **Reliable vehicle:** the player can plan farther ahead, carry more, and revisit sites, but the platform becomes worth protecting.
- **Van, truck, bus, or camper:** the base becomes a practical hub for storage, repair, power, rest, route planning, and possibly future crew support.

Vehicle growth should add decisions, not erase survival. Larger platforms should consume more, attract more attention, need more maintenance, and struggle with route constraints.

### Places should make sense

Sites should be designed around what they were before the collapse and why the player cares now. Loot, danger, objects, locked areas, power, cover, and containers should follow the logic of the place.

### Logistical believability beats apocalypse spectacle

When choosing between a dramatic wasteland image and a practical infrastructure problem, prefer the practical problem unless gameplay needs otherwise. A locked powered pump, a half-lit clinic, a blocked pass, or a full depot behind bad credentials is usually stronger than generic destruction.

## Core Player Experience

The game should support this loop:

1. Travel across the overworld.
2. Watch fuel, time, route, travel method, and carrying capacity.
3. Stop at a point of interest.
4. Enter a local site.
5. Search, scavenge, fight, avoid danger, or gather information.
6. Return to the travel method or mobile base.
7. Manage inventory, equipment, fuel, ammunition, condition, and eventually repairs.
8. Decide what risk to take next.

Every major location should answer at least one survival question:

- Can I get fuel here?
- Can I get food or water here?
- Can I get medicine here?
- Can I get parts, tools, or ammunition here?
- Can I shelter, repair, trade, or learn something here?
- Can I carry what I found?
- Can I survive the risk of entering?

## Power And Fuel

Power should be local, uneven, unreliable, and meaningful. Some places may still run because of solar batteries, diesel generators, farm microgrids, wind or hydro, hospital backups, military systems, corporate continuity systems, isolated grid sections, vehicle batteries, or emergency lighting.

When a location has power, it should raise questions: why is it powered, who maintained it, what system is running, what does it think its job is, and is the power useful, dangerous, or both?

Fuel still exists because distribution failed before all reserves were consumed. Fuel can be in service station tanks, abandoned vehicles, farm tanks, depots, workshops, generators, machinery, fuel cans, municipal yards, and industrial sites.

Fuel should be valuable, not mythical. The interesting question is not only whether fuel exists, but whether the player can power the pump, carry the fuel, survive the site, transport it, justify the trip, or avoid drawing attention.

## Threats

The game should not need zombies or supernatural enemies.

Primary threats:

- Armed survivors, scavengers, guards, and desperate people.
- Near-future mundane automated systems: cameras, turrets at high-security sites, locked pumps, access controls, security doors, gates, warning broadcasts, roadblock signals, and occasional drones if a later feature needs them.
- Misaligned infrastructure: power failures, locked facilities, blocked roads, failed pumps, broken vehicles, bad information, and unsafe buildings.

Secondary threats can include wildlife, exposure, medical scarcity, hunger, thirst, fatigue, terrain, weather, and accidents when the relevant systems exist.

Combat is a core activity, but it should be grounded and consequential. Ammo, wounds, sound, position, weapon handling, cover, visibility, and retreat should matter. Firearms exist in the US setting, but access and ammunition should still feel scarce enough that combat decisions have weight.

Combat should be common enough to shape planning. It should not be so frictionless that the best answer is always to shoot. Good combat pressure asks whether the player has the right weapon, enough ammunition, a safe angle, an escape route, medical supplies, and a reason worth spending all of that.

## Automated Systems

Automated systems are one of the setting's main non-zombie threats. They should feel cold, procedural, and misaligned rather than evil.

They are doing what they were told to do. The horror is that nobody with authority can tell them to stop.

Useful phrases:

- "Access denied."
- "Authorized personnel only."
- "Stand clear."
- "Asset protection mode active."
- "Continuity protocol in effect."
- "Emergency rationing active."
- "Fuel distribution suspended."
- "Awaiting central authority response."

## Site Guidance

### Gas stations and truck stops

Primary value: fuel, food, water, maps, batteries, vehicle supplies, bathrooms, shelter, and information. Core question: can the player access fuel safely?

Useful details: one pump powered, other pumps dead, payment errors, cameras, locked tanks, empty queues, abandoned cars, stripped shelves, security systems enforcing property rules.

### Farmhouses and rural properties

Primary value: food, tools, water, clothing, shelter, parts, fuel, repair space, and rural equipment. Core question: was this place abandoned, defended, emptied, or still claimed?

Useful details: packed bags, water tanks, solar batteries, sheds, pantry supplies, stripped vehicles, family spaces, livestock remnants, machinery, fences, signs of hurried departure or attempted self-sufficiency.

### Clinics and pharmacies

Primary value: medicine, bandages, antibiotics, painkillers, medical equipment, records, and triage clues. Core question: is the medicine still there, and what happened to the people who needed it?

Useful details: emergency lighting, locked cabinets, warm vaccine fridges, triage notes, abandoned waiting rooms, inaccessible records, handwritten instructions after systems failed.

### Roadblocks and convoys

Primary value: information, vehicles, cargo, fuel, weapons, parts, and danger. Core question: is this an opportunity, a warning, a trap, or a boundary?

Useful details: official barricades, abandoned uniforms, contradictory signs, disabled vehicles, warning lights, makeshift survivor additions, and evidence of later conflict.

### Warehouses, depots, and industrial sites

Primary value: stored goods, tools, fuel, machinery, forklifts, parts, and secure infrastructure. Core question: what is still protected, and by whom or what?

Useful details: full shelves behind locked gates, dead forklifts, automated locks, shipping labels to places that never received supplies, powered corporate security.

### Radio towers and utility sites

Primary value: information, power, maps, signals, batteries, maintenance tools, and site discovery. Core question: what is still broadcasting, and who can hear it?

Useful details: repeated messages, nearby generators, locked huts, logs with missed check-ins, signal clues, and evidence that nobody has maintained the site for weeks.

### Colorado-specific site families

Useful early or mid-term site families:

- highway rest areas, weigh stations, chain-up areas, and snowplow depots
- mountain pass blockages, tunnel approaches, avalanche gates, and maintenance sheds
- ski resort service zones, lodges, rental shops, parking lots, and employee housing
- ranches, feed stores, irrigation sites, grain elevators, and farm co-ops
- small-town clinics, sheriff substations, fire stations, schools, churches, and grocery stores
- rail sidings, intermodal yards, trucking depots, warehouses, and cold-storage facilities
- water treatment plants, reservoirs, pump stations, telecom sites, substations, and solar or wind installations
- federal, state, county, military, prison, research, and emergency-management facilities

Use these because they create practical survival and route questions, not because every category needs immediate implementation.

## NPC And Faction Seeds

Do not implement factions just because they are listed here. These are setting seeds for later content.

- **Continuity Remnants:** former officials, soldiers, emergency managers, contractors, or guards preserving some version of order.
- **Local Compacts:** towns or neighborhoods that pooled resources and closed their borders.
- **Salvage Crews:** mobile scavengers who strip sites, trade, avoid conflict, or prey on weaker travelers.
- **Asset Protection Systems:** automated sites that effectively behave like a faction while guarding fuel, medicine, weapons, data, or infrastructure.
- **Road Families and Convoy Groups:** people surviving by moving together.
- **Depot Holdouts:** groups whose power comes from warehouses, fuel depots, rail yards, distribution centers, or industrial sites.
- **County Holdouts:** sheriffs, deputies, road crews, firefighters, and emergency managers holding local systems together with partial information.
- **Militia And Neighborhood Defense Groups:** armed locals who may be protective, paranoid, predatory, or simply exhausted.
- **Churches, Mutual Aid, And Civic Groups:** people organizing food, shelter, care, radio contact, burial, or evacuation around existing community trust.
- **Corporate Security And Contractors:** private security, logistics staff, site managers, and automated systems guarding assets under outdated orders.
- **Truckers, Mechanics, Farmers, And Utility Workers:** practical survivors with route knowledge, tools, fuel access, and reasons to distrust outsiders.

NPCs should disagree about The Failover. Beliefs can include cyberattack, government failure, corporate control, military abandonment, religious interpretation, and practical survivor cynicism. Do not make every NPC an exposition machine.

US-specific texture should stay grounded. Civilian firearms, county authority, churches, mutual aid, private security, militias, truck stops, big-box logistics, and local radio can all matter, but they should not flatten the setting into caricature.

## Tone Rules

Use grounded, practical language:

- clear
- restrained
- physical
- slightly bleak
- specific
- not melodramatic
- not lore-dumpy

Avoid cartoon wasteland defaults:

- skulls everywhere
- raider spikes everywhere
- endless fire barrels
- random gore
- everyone is mad
- every place is looted
- all technology is broken
- all authority is evil
- all survivors are hostile

The setting is stronger when the world is bureaucratic, logistical, partial, human, and recent.

## Content Decision Rules

Priority order for future decisions:

1. Preserve current scope unless the user explicitly asks to expand it.
2. Prefer the choice that creates survival logistics, tactical risk, or route pressure.
3. Prefer the choice that fits Colorado geography and US infrastructure.
4. Prefer believable access problems over arbitrary scarcity.
5. Prefer specific sites, items, and objects over generic apocalypse props.
6. Prefer systems the player can understand and act on over hidden realism.

When adding a site, define:

- why it existed before the collapse
- why the player cares now
- the obvious resource types
- the likely risks
- what systems may still have power
- what is locked, blocked, guarded, or depleted
- what the player can learn by looking around
- how the site connects to travel, combat, survival, or mobile-base needs

When adding items or objects, prefer specific, physically believable concepts over generic ones. Fuel pumps, doors, fridges, generators, shelves, cabinets, workbenches, tanks, gates, radios, and locks should eventually become interaction anchors when matching systems exist.

When adding mechanics, prefer domain-owned state, Godot as presentation/input, action-pipeline resolution, clear failure messages, no silent mutation on failed actions, and focused vertical slices that preserve current scope.

Useful comparison:

- Strong: a service station has fuel in underground tanks, but the pump circuit is powered by a locked system and a nearby automated camera still treats the forecourt as protected property.
- Weak: a service station has random loot, generic enemies, and no reason its resources are present or absent.

- Strong: a mountain pass is technically open, but abandoned vehicles, weather, low fuel, and a hostile checkpoint make the route a decision.
- Weak: a road is blocked only because the level needs a wall.
