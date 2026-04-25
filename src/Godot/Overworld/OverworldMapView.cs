using System;
using System.Collections.Generic;
using Godot;
using SurvivalGame.Domain;

public partial class OverworldMapView : Control
{
    private static readonly Color BackgroundColor = new(0.035f, 0.047f, 0.054f);
    private static readonly Color MapColor = new(0.18f, 0.27f, 0.19f);
    private static readonly Color FieldColor = new(0.26f, 0.35f, 0.19f, 0.72f);
    private static readonly Color ScrubColor = new(0.15f, 0.23f, 0.19f, 0.72f);
    private static readonly Color WaterColor = new(0.08f, 0.20f, 0.25f, 0.78f);
    private static readonly Color BorderColor = new(0.48f, 0.57f, 0.45f);
    private static readonly Color DestinationColor = new(0.91f, 0.81f, 0.46f);
    private static readonly Color SiteColor = new(0.90f, 0.76f, 0.42f);
    private static readonly Color PartyColor = new(0.92f, 0.94f, 0.90f);

    private OverworldTravelState? _travelState;
    private IReadOnlyList<OverworldPointOfInterest> _sites = Array.Empty<OverworldPointOfInterest>();

    public event Action<OverworldPosition>? DestinationSelected;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
    }

    public void Configure(OverworldTravelState travelState, IReadOnlyList<OverworldPointOfInterest> sites)
    {
        _travelState = travelState;
        _sites = sites;
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (_travelState is null
            || @event is not InputEventMouseButton mouse
            || !mouse.Pressed
            || mouse.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        var mapRect = GetMapRect();
        if (!mapRect.HasPoint(mouse.Position))
        {
            return;
        }

        DestinationSelected?.Invoke(ScreenToMap(mouse.Position, mapRect));
        AcceptEvent();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), BackgroundColor);
        var mapRect = GetMapRect();
        DrawRect(mapRect, MapColor);
        DrawMapFeatures(mapRect);
        DrawRect(mapRect, BorderColor, filled: false, width: 2.0f);

        if (_travelState is null)
        {
            return;
        }

        foreach (var site in _sites)
        {
            DrawSiteMarker(site, mapRect);
        }

        if (_travelState.Destination is { } destination)
        {
            var partyPoint = MapToScreen(_travelState.Position, mapRect);
            var destinationPoint = MapToScreen(destination, mapRect);
            DrawLine(partyPoint, destinationPoint, DestinationColor, 2.0f);
            DrawCircle(destinationPoint, 7.0f, DestinationColor);
            DrawCircle(destinationPoint, 3.0f, BackgroundColor);
        }

        DrawPartyMarker(MapToScreen(_travelState.Position, mapRect));
    }

    private void DrawMapFeatures(Rect2 mapRect)
    {
        DrawRect(new Rect2(
            mapRect.Position + new Vector2(mapRect.Size.X * 0.08f, mapRect.Size.Y * 0.13f),
            new Vector2(mapRect.Size.X * 0.28f, mapRect.Size.Y * 0.22f)
        ), FieldColor);
        DrawRect(new Rect2(
            mapRect.Position + new Vector2(mapRect.Size.X * 0.58f, mapRect.Size.Y * 0.56f),
            new Vector2(mapRect.Size.X * 0.26f, mapRect.Size.Y * 0.18f)
        ), FieldColor);
        DrawRect(new Rect2(
            mapRect.Position + new Vector2(mapRect.Size.X * 0.45f, mapRect.Size.Y * 0.18f),
            new Vector2(mapRect.Size.X * 0.36f, mapRect.Size.Y * 0.22f)
        ), ScrubColor);

        var stream = new[]
        {
            mapRect.Position + new Vector2(mapRect.Size.X * 0.02f, mapRect.Size.Y * 0.72f),
            mapRect.Position + new Vector2(mapRect.Size.X * 0.22f, mapRect.Size.Y * 0.63f),
            mapRect.Position + new Vector2(mapRect.Size.X * 0.48f, mapRect.Size.Y * 0.68f),
            mapRect.Position + new Vector2(mapRect.Size.X * 0.76f, mapRect.Size.Y * 0.58f),
            mapRect.Position + new Vector2(mapRect.Size.X * 0.98f, mapRect.Size.Y * 0.62f)
        };
        for (var i = 0; i < stream.Length - 1; i++)
        {
            DrawLine(stream[i], stream[i + 1], WaterColor, 14.0f);
        }
    }

    private void DrawSiteMarker(OverworldPointOfInterest site, Rect2 mapRect)
    {
        var point = MapToScreen(site.Position, mapRect);
        var diamond = new[]
        {
            point + new Vector2(0, -10),
            point + new Vector2(10, 0),
            point + new Vector2(0, 10),
            point + new Vector2(-10, 0)
        };
        DrawColoredPolygon(diamond, SiteColor);
        DrawCircle(point, 4.0f, BackgroundColor);
        DrawString(
            ThemeDB.FallbackFont,
            point + new Vector2(13, -8),
            site.DisplayName,
            HorizontalAlignment.Left,
            width: 180,
            fontSize: 14,
            modulate: new Color(0.88f, 0.91f, 0.82f)
        );
    }

    private void DrawPartyMarker(Vector2 point)
    {
        var shadow = new[]
        {
            point + new Vector2(-11, 10),
            point + new Vector2(11, 10),
            point + new Vector2(8, 15),
            point + new Vector2(-8, 15)
        };
        DrawColoredPolygon(shadow, new Color(0.02f, 0.025f, 0.02f, 0.45f));

        var marker = new[]
        {
            point + new Vector2(0, -16),
            point + new Vector2(13, 10),
            point + new Vector2(0, 5),
            point + new Vector2(-13, 10)
        };
        DrawColoredPolygon(marker, PartyColor);
        DrawCircle(point, 4.0f, new Color(0.23f, 0.34f, 0.27f));
    }

    private Rect2 GetMapRect()
    {
        const float margin = 34.0f;
        var width = Mathf.Max(160.0f, Size.X - (margin * 2.0f));
        var height = Mathf.Max(160.0f, Size.Y - (margin * 2.0f));
        return new Rect2(new Vector2(margin, margin), new Vector2(width, height));
    }

    private Vector2 MapToScreen(OverworldPosition position, Rect2 mapRect)
    {
        if (_travelState is null)
        {
            return mapRect.Position;
        }

        return mapRect.Position + new Vector2(
            (float)(position.X / _travelState.MapWidth) * mapRect.Size.X,
            (float)(position.Y / _travelState.MapHeight) * mapRect.Size.Y
        );
    }

    private OverworldPosition ScreenToMap(Vector2 point, Rect2 mapRect)
    {
        if (_travelState is null)
        {
            return new OverworldPosition(0, 0);
        }

        var x = (point.X - mapRect.Position.X) / mapRect.Size.X * (float)_travelState.MapWidth;
        var y = (point.Y - mapRect.Position.Y) / mapRect.Size.Y * (float)_travelState.MapHeight;
        return new OverworldPosition(x, y);
    }
}
