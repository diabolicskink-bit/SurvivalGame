using System;
using System.Collections.Generic;
using Godot;
using SurvivalGame.Domain;

public partial class WorldMapView : Control
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

    private static readonly WorldFeatureRect[] FeatureRects =
    [
        new(new WorldMapPosition(95.0, 100.0), 335.0, 170.0, FeatureKind.Field),
        new(new WorldMapPosition(690.0, 430.0), 310.0, 140.0, FeatureKind.Field),
        new(new WorldMapPosition(540.0, 135.0), 430.0, 170.0, FeatureKind.Scrub),
        new(new WorldMapPosition(1320.0, 210.0), 360.0, 180.0, FeatureKind.Scrub),
        new(new WorldMapPosition(1480.0, 835.0), 430.0, 190.0, FeatureKind.Field),
        new(new WorldMapPosition(420.0, 910.0), 390.0, 170.0, FeatureKind.Field),
        new(new WorldMapPosition(1160.0, 1040.0), 300.0, 150.0, FeatureKind.Scrub)
    ];

    private static readonly WorldMapPosition[] Stream =
    [
        new(25.0, 545.0),
        new(265.0, 480.0),
        new(575.0, 515.0),
        new(910.0, 440.0),
        new(1175.0, 470.0),
        new(1430.0, 565.0),
        new(1710.0, 545.0),
        new(2070.0, 650.0)
    ];

    private WorldMapTravelState? _travelState;
    private IReadOnlyList<WorldMapPointOfInterest> _sites = Array.Empty<WorldMapPointOfInterest>();

    public event Action<WorldMapPosition>? DestinationSelected;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
    }

    public void Configure(WorldMapTravelState travelState, IReadOnlyList<WorldMapPointOfInterest> sites)
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

        var viewport = CreateCurrentViewport();
        DestinationSelected?.Invoke(ScreenToMap(mouse.Position, mapRect, viewport));
        AcceptEvent();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), BackgroundColor);
        var mapRect = GetMapRect();
        DrawRect(mapRect, MapColor);

        if (_travelState is null)
        {
            DrawRect(mapRect, BorderColor, filled: false, width: 2.0f);
            return;
        }

        var viewport = CreateCurrentViewport();
        DrawMapFeatures(mapRect, viewport);
        DrawRect(mapRect, BorderColor, filled: false, width: 2.0f);

        foreach (var site in _sites)
        {
            if (viewport.Contains(site.Position))
            {
                DrawSiteMarker(site, mapRect, viewport);
            }
        }

        if (_travelState.Destination is { } destination)
        {
            DrawDestination(destination, mapRect, viewport);
        }

        DrawPartyMarker(MapToScreen(_travelState.Position, mapRect, viewport));
    }

    private void DrawMapFeatures(Rect2 mapRect, WorldMapViewport viewport)
    {
        foreach (var feature in FeatureRects)
        {
            DrawFeatureRect(feature, mapRect, viewport);
        }

        for (var i = 0; i < Stream.Length - 1; i++)
        {
            DrawStreamSegment(Stream[i], Stream[i + 1], mapRect, viewport);
        }
    }

    private void DrawFeatureRect(WorldFeatureRect feature, Rect2 mapRect, WorldMapViewport viewport)
    {
        var featureRect = new WorldRect(feature.Position, feature.Width, feature.Height);
        if (!featureRect.Intersects(viewport))
        {
            return;
        }

        var screenRect = WorldRectToScreen(featureRect, mapRect, viewport);
        var clippedRect = ClipRect(screenRect, mapRect);
        if (clippedRect.Size.X <= 0 || clippedRect.Size.Y <= 0)
        {
            return;
        }

        DrawRect(clippedRect, feature.Kind == FeatureKind.Field ? FieldColor : ScrubColor);
    }

    private void DrawStreamSegment(
        WorldMapPosition start,
        WorldMapPosition end,
        Rect2 mapRect,
        WorldMapViewport viewport)
    {
        if (!viewport.Contains(start) && !viewport.Contains(end) && !SegmentMayCrossViewport(start, end, viewport))
        {
            return;
        }

        var screenStart = ClipPointToRect(MapToScreen(start, mapRect, viewport), mapRect);
        var screenEnd = ClipPointToRect(MapToScreen(end, mapRect, viewport), mapRect);
        DrawLine(screenStart, screenEnd, WaterColor, 14.0f);
    }

    private void DrawSiteMarker(WorldMapPointOfInterest site, Rect2 mapRect, WorldMapViewport viewport)
    {
        var point = MapToScreen(site.Position, mapRect, viewport);
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

    private void DrawDestination(WorldMapPosition destination, Rect2 mapRect, WorldMapViewport viewport)
    {
        if (_travelState is null)
        {
            return;
        }

        var partyPoint = MapToScreen(_travelState.Position, mapRect, viewport);
        var destinationPoint = MapToScreen(destination, mapRect, viewport);
        var visibleDestinationPoint = ClipPointToRect(destinationPoint, mapRect);

        DrawLine(partyPoint, visibleDestinationPoint, DestinationColor, 2.0f);
        DrawCircle(visibleDestinationPoint, 7.0f, DestinationColor);
        DrawCircle(visibleDestinationPoint, 3.0f, BackgroundColor);
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

    private WorldMapViewport CreateCurrentViewport()
    {
        if (_travelState is null)
        {
            return WorldMapViewport.Create(
                PrototypeWorldMapSites.MapWidth,
                PrototypeWorldMapSites.MapHeight,
                PrototypeWorldMapSites.VisibleWidth,
                PrototypeWorldMapSites.VisibleHeight,
                PrototypeWorldMapSites.StartPosition
            );
        }

        return WorldMapViewport.Create(
            _travelState.MapWidth,
            _travelState.MapHeight,
            PrototypeWorldMapSites.VisibleWidth,
            PrototypeWorldMapSites.VisibleHeight,
            _travelState.Position
        );
    }

    private Vector2 MapToScreen(WorldMapPosition position, Rect2 mapRect, WorldMapViewport viewport)
    {
        var viewportPosition = viewport.MapToViewport(position);

        return mapRect.Position + new Vector2(
            (float)(viewportPosition.X / viewport.Width) * mapRect.Size.X,
            (float)(viewportPosition.Y / viewport.Height) * mapRect.Size.Y
        );
    }

    private WorldMapPosition ScreenToMap(Vector2 point, Rect2 mapRect, WorldMapViewport viewport)
    {
        var x = (point.X - mapRect.Position.X) / mapRect.Size.X * (float)viewport.Width;
        var y = (point.Y - mapRect.Position.Y) / mapRect.Size.Y * (float)viewport.Height;
        return viewport.ViewportToMap(new WorldMapPosition(x, y));
    }

    private static Rect2 WorldRectToScreen(WorldRect rect, Rect2 mapRect, WorldMapViewport viewport)
    {
        var position = viewport.MapToViewport(rect.Position);
        var scaleX = mapRect.Size.X / (float)viewport.Width;
        var scaleY = mapRect.Size.Y / (float)viewport.Height;
        return new Rect2(
            mapRect.Position + new Vector2((float)position.X * scaleX, (float)position.Y * scaleY),
            new Vector2((float)rect.Width * scaleX, (float)rect.Height * scaleY)
        );
    }

    private static Rect2 ClipRect(Rect2 rect, Rect2 clip)
    {
        var left = Mathf.Max(rect.Position.X, clip.Position.X);
        var top = Mathf.Max(rect.Position.Y, clip.Position.Y);
        var right = Mathf.Min(rect.Position.X + rect.Size.X, clip.Position.X + clip.Size.X);
        var bottom = Mathf.Min(rect.Position.Y + rect.Size.Y, clip.Position.Y + clip.Size.Y);
        return new Rect2(
            new Vector2(left, top),
            new Vector2(Mathf.Max(0, right - left), Mathf.Max(0, bottom - top))
        );
    }

    private static Vector2 ClipPointToRect(Vector2 point, Rect2 rect)
    {
        return new Vector2(
            Mathf.Clamp(point.X, rect.Position.X, rect.Position.X + rect.Size.X),
            Mathf.Clamp(point.Y, rect.Position.Y, rect.Position.Y + rect.Size.Y)
        );
    }

    private static bool SegmentMayCrossViewport(
        WorldMapPosition start,
        WorldMapPosition end,
        WorldMapViewport viewport)
    {
        var left = Math.Min(start.X, end.X);
        var right = Math.Max(start.X, end.X);
        var top = Math.Min(start.Y, end.Y);
        var bottom = Math.Max(start.Y, end.Y);

        return right >= viewport.Origin.X
            && left <= viewport.Right
            && bottom >= viewport.Origin.Y
            && top <= viewport.Bottom;
    }

    private enum FeatureKind
    {
        Field,
        Scrub
    }

    private readonly record struct WorldFeatureRect(
        WorldMapPosition Position,
        double Width,
        double Height,
        FeatureKind Kind);

    private readonly record struct WorldRect(WorldMapPosition Position, double Width, double Height)
    {
        public bool Intersects(WorldMapViewport viewport)
        {
            return Position.X + Width >= viewport.Origin.X
                && Position.X <= viewport.Right
                && Position.Y + Height >= viewport.Origin.Y
                && Position.Y <= viewport.Bottom;
        }
    }
}
