# Firearms Domain

Plain C# firearm, ammunition, and feed-device definitions plus runtime loaded state.

This layer owns ammunition compatibility, feed capacity, ammunition damage values, weapon range definitions, loaded counts, magazine insertion/removal, direct weapon loading, prototype test-fire rules, and equipped-firearm shooting resolution. Godot UI should display this state and request actions through the domain action pipeline rather than mutating loaded state directly.

Action availability and UI refresh should be read-only. Runtime firearm state should appear when a state-changing action succeeds, not because a panel asked what buttons to show.
