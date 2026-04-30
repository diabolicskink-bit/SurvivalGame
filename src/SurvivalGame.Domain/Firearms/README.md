# Firearms Domain

Plain C# firearm, ammunition, feed-device, and weapon-mod definitions plus runtime loaded state.

This layer owns ammunition compatibility, feed capacity, ammunition damage values, weapon range/accuracy definitions, supported/current fire modes, loaded counts, magazine insertion/removal, direct weapon loading, stateful weapon mod installation/removal, prototype test-fire rules, line-of-fire validation, and equipped-firearm shooting resolution. Godot UI should display this state and request actions through the domain action pipeline rather than mutating loaded state directly.

Targeted shooting traces line of fire through the local map before ammo or time mutation. Sight-blocking structure edges and intermediate sight-blocking world-object tiles stop the shot; test fire remains a one-round prototype action and does not use line-of-fire.

After targeted shooting passes precondition checks, the shot consumes ammunition/time and rolls against the weapon's modified hit chance. Accuracy is defined as two required endpoint values on each weapon: hit chance at or inside effective range, and hit chance at maximum range. Between those distances the chance falls linearly, then clamps to a 5-95% final roll band so a valid shot is never guaranteed or impossible. Current tile ranges are scaled for the 27x18 local viewport: pistols and shotguns are short-range tools, carbines and rifles can cover most of the visible board, and scoped long guns can reach beyond a single current viewport.

Current prototype weapon range/accuracy values:

| Weapon | Effective range | Maximum range | Effective accuracy | Maximum accuracy |
| --- | ---: | ---: | ---: | ---: |
| 9mm pistol | 4 | 10 | 72% | 12% |
| AK-style rifle | 9 | 20 | 76% | 18% |
| .308 hunting rifle | 12 | 26 | 87% | 35% |
| 12 gauge shotgun | 3 | 8 | 86% | 10% |
| .22 rifle | 8 | 18 | 82% | 24% |
| 5.56 burst carbine | 10 | 22 | 80% | 22% |

Weapon mods are currently stateful-item attachments only. They install into one slot per weapon, use weapon-family compatibility, and provide additive prototype range, damage, and accuracy modifiers for the current shooting rules.

Action availability and UI refresh should be read-only. Runtime firearm state should appear when a state-changing action succeeds, not because a panel asked what buttons to show.
