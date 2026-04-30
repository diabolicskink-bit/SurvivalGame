namespace SurvivalGame.Domain;

public static class PrototypeNpcs
{
    public static readonly NpcDefinitionId TestDummyDefinition = new("test_dummy");
    public static readonly NpcDefinitionId CautiousSurvivor = new("cautious_survivor");
    public static readonly NpcDefinitionId WanderingScavenger = new("wandering_scavenger");
    public static readonly NpcDefinitionId InjuredTraveller = new("injured_traveller");
    public static readonly NpcDefinitionId QuietMechanic = new("quiet_mechanic");
    public static readonly NpcDefinitionId FieldResearcher = new("field_researcher");
    public static readonly NpcDefinitionId AutomatedTurretDefinition = new("automated_turret");

    public static readonly NpcId TestDummy = new("test_dummy_01");
    public static readonly NpcId GasStationTurret = new("gas_station_turret_01");
    public static readonly NpcId GasStationScavenger = new("gas_station_scavenger_01");
}
