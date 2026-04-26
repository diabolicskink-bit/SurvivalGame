# Firearms Domain

Plain C# firearm, ammunition, feed-device, and weapon-mod definitions plus runtime loaded state.

This layer owns ammunition compatibility, feed capacity, ammunition damage values, weapon range definitions, loaded counts, magazine insertion/removal, direct weapon loading, stateful weapon mod installation/removal, prototype test-fire rules, and equipped-firearm shooting resolution. Godot UI should display this state and request actions through the domain action pipeline rather than mutating loaded state directly.

Weapon mods are currently stateful-item attachments only. They install into one slot per weapon, use weapon-family compatibility, and provide additive prototype range/damage modifiers for the current shooting rules.

Action availability and UI refresh should be read-only. Runtime firearm state should appear when a state-changing action succeeds, not because a panel asked what buttons to show.
