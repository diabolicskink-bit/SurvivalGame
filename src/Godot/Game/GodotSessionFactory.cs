using Godot;
using SurvivalGame.Application;
using SurvivalGame.Domain;

public static class GodotSessionFactory
{
    public static LocalSiteSession CreateStandaloneLocalSiteSession(VehicleFuelState? vehicleFuelState = null)
    {
        return GameSessionFactory.CreateStandaloneLocalSiteSession(CreateContentPaths(), vehicleFuelState);
    }

    public static CampaignSession CreateCampaignSession(VehicleFuelState? vehicleFuelState = null)
    {
        return GameSessionFactory.CreateCampaignSession(CreateContentPaths(), vehicleFuelState);
    }

    private static GameContentPaths CreateContentPaths()
    {
        return GameContentPaths.FromDataRoot(ProjectSettings.GlobalizePath("res://data"));
    }
}
