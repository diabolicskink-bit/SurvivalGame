using System;
using System.Linq;
using Godot;
using SurvivalGame.Domain;

public partial class WorldMapView : Control
{
    private static readonly Color BackgroundColor = new(0.035f, 0.047f, 0.054f);
    private static readonly Color MapColor = new(0.13f, 0.19f, 0.14f);
    private static readonly Color BorderColor = new(0.48f, 0.57f, 0.45f);
    private static readonly Color DestinationColor = new(0.91f, 0.81f, 0.46f);
    private static readonly Color PartyColor = new(0.92f, 0.94f, 0.90f);
    private static readonly Color InterstateColor = new(0.82f, 0.74f, 0.55f, 0.94f);
    private static readonly Color UsHighwayColor = new(0.66f, 0.69f, 0.60f, 0.90f);
    private static readonly Color StateHighwayColor = new(0.50f, 0.56f, 0.49f, 0.86f);
    private static readonly Color RoadCasingColor = new(0.07f, 0.085f, 0.07f, 0.90f);
    private static readonly Color RoadLabelColor = new(0.85f, 0.88f, 0.78f, 0.88f);
    private static readonly Color RoadLabelShadowColor = new(0.03f, 0.04f, 0.03f, 0.90f);
    private static readonly Color CityColor = new(0.86f, 0.87f, 0.78f);
    private static readonly Color LandmarkColor = new(0.90f, 0.76f, 0.42f);
    private static readonly Color LocalSiteColor = new(0.98f, 0.63f, 0.36f);

    private WorldMapTravelState? _travelState;
    private WorldMapDefinition? _worldMap;

    public event Action<WorldMapPosition>? DestinationSelected;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
    }

    public void Configure(WorldMapTravelState travelState, WorldMapDefinition worldMap)
    {
        _travelState = travelState;
        _worldMap = worldMap;
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

        if (_travelState is null || _worldMap is null)
        {
            DrawRect(mapRect, BorderColor, filled: false, width: 2.0f);
            return;
        }

        var viewport = CreateCurrentViewport();
        DrawTerrainRegions(mapRect, viewport);
        DrawRoads(mapRect, viewport);
        DrawRect(mapRect, BorderColor, filled: false, width: 2.0f);
        DrawPointsOfInterest(mapRect, viewport);

        if (_travelState.Destination is { } destination)
        {
            DrawDestination(destination, mapRect, viewport);
        }

        DrawPartyMarker(MapToScreen(_travelState.Position, mapRect, viewport));
    }

    private void DrawTerrainRegions(Rect2 mapRect, WorldMapViewport viewport)
    {
        if (_worldMap is null)
        {
            return;
        }

        foreach (var region in _worldMap.TerrainRegions)
        {
            var points = new Vector2[region.Points.Count];
            for (var i = 0; i < region.Points.Count; i++)
            {
                points[i] = MapToScreen(region.Points[i], mapRect, viewport);
            }

            DrawColoredPolygon(points, ColorFromHex(region.MapColor, MapColor));
        }
    }

    private void DrawRoads(Rect2 mapRect, WorldMapViewport viewport)
    {
        if (_worldMap is null)
        {
            return;
        }

        var roads = _worldMap.Roads
            .OrderByDescending(road => road.Priority)
            .ToArray();

        foreach (var road in roads)
        {
            DrawRoadPass(road, RoadCasingColor, RoadFillWidth(road) + RoadCasingWidth(road), mapRect, viewport);
        }

        foreach (var road in roads)
        {
            DrawRoadPass(road, RoadFillColor(road), RoadFillWidth(road), mapRect, viewport);
        }

        DrawRoadLabels(mapRect, viewport);
    }

    private void DrawRoadPass(
        WorldMapRoad road,
        Color color,
        float width,
        Rect2 mapRect,
        WorldMapViewport viewport)
    {
        foreach (var segment in road.Segments)
        {
            for (var i = 0; i < segment.Points.Count - 1; i++)
            {
                DrawRoadSegment(segment.Points[i], segment.Points[i + 1], color, width, mapRect, viewport);
            }
        }
    }

    private void DrawRoadSegment(
        WorldMapPosition start,
        WorldMapPosition end,
        Color color,
        float width,
        Rect2 mapRect,
        WorldMapViewport viewport)
    {
        if (!TryClipSegmentToViewport(start, end, viewport, out var clippedStart, out var clippedEnd))
        {
            return;
        }

        var screenStart = MapToScreen(clippedStart, mapRect, viewport);
        var screenEnd = MapToScreen(clippedEnd, mapRect, viewport);
        DrawLine(screenStart, screenEnd, color, width);
    }

    private void DrawRoadLabels(Rect2 mapRect, WorldMapViewport viewport)
    {
        if (_worldMap is null)
        {
            return;
        }

        var drawn = 0;
        foreach (var road in _worldMap.Roads.OrderBy(road => road.Priority))
        {
            if (road.Priority > 2 || drawn >= 8)
            {
                continue;
            }

            if (!TryFindRoadLabelPosition(road, viewport, out var labelPosition))
            {
                continue;
            }

            var screenPosition = MapToScreen(labelPosition, mapRect, viewport);
            var labelOffset = new Vector2(8, -5);
            DrawString(
                ThemeDB.FallbackFont,
                screenPosition + labelOffset + new Vector2(1, 1),
                road.DisplayName,
                HorizontalAlignment.Left,
                width: 80,
                fontSize: 11,
                modulate: RoadLabelShadowColor
            );
            DrawString(
                ThemeDB.FallbackFont,
                screenPosition + labelOffset,
                road.DisplayName,
                HorizontalAlignment.Left,
                width: 80,
                fontSize: 11,
                modulate: RoadLabelColor
            );
            drawn++;
        }
    }

    private static bool TryFindRoadLabelPosition(
        WorldMapRoad road,
        WorldMapViewport viewport,
        out WorldMapPosition position)
    {
        foreach (var segment in road.Segments)
        {
            for (var i = 0; i < segment.Points.Count - 1; i++)
            {
                if (!TryClipSegmentToViewport(
                    segment.Points[i],
                    segment.Points[i + 1],
                    viewport,
                    out var clippedStart,
                    out var clippedEnd))
                {
                    continue;
                }

                position = new WorldMapPosition(
                    (clippedStart.X + clippedEnd.X) / 2.0,
                    (clippedStart.Y + clippedEnd.Y) / 2.0
                );
                return true;
            }
        }

        position = default;
        return false;
    }

    private void DrawPointsOfInterest(Rect2 mapRect, WorldMapViewport viewport)
    {
        if (_worldMap is null)
        {
            return;
        }

        foreach (var site in _worldMap.PointsOfInterest.OrderByDescending(site => site.LabelPriority))
        {
            if (viewport.Contains(site.Position))
            {
                DrawSiteMarker(site, mapRect, viewport);
            }
        }
    }

    private void DrawSiteMarker(WorldMapPointOfInterest site, Rect2 mapRect, WorldMapViewport viewport)
    {
        var point = MapToScreen(site.Position, mapRect, viewport);
        var color = site.Category switch
        {
            WorldMapPointCategory.City => CityColor,
            WorldMapPointCategory.LocalSite => LocalSiteColor,
            _ => LandmarkColor
        };
        var radius = site.Category == WorldMapPointCategory.City ? 4.0f : 6.0f;

        if (site.Category == WorldMapPointCategory.Landmark)
        {
            var diamond = new[]
            {
                point + new Vector2(0, -radius),
                point + new Vector2(radius, 0),
                point + new Vector2(0, radius),
                point + new Vector2(-radius, 0)
            };
            DrawColoredPolygon(diamond, color);
        }
        else
        {
            DrawCircle(point, radius, color);
        }

        if (site.Category == WorldMapPointCategory.LocalSite)
        {
            DrawCircle(point, radius + 3.0f, new Color(color.R, color.G, color.B, 0.28f));
        }

        if (ShouldDrawLabel(site))
        {
            DrawString(
                ThemeDB.FallbackFont,
                point + new Vector2(9, -7),
                site.DisplayName,
                HorizontalAlignment.Left,
                width: 190,
                fontSize: site.LabelPriority <= 1 ? 14 : 12,
                modulate: new Color(0.88f, 0.91f, 0.82f)
            );
        }
    }

    private static bool ShouldDrawLabel(WorldMapPointOfInterest site)
    {
        return site.Category == WorldMapPointCategory.LocalSite
            || site.LabelPriority <= 2;
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
        var definition = _worldMap ?? PrototypeWorldMapSites.Definition;
        var focus = _travelState?.Position ?? definition.StartPosition;

        return WorldMapViewport.Create(
            definition.MapWidth,
            definition.MapHeight,
            definition.VisibleWidth,
            definition.VisibleHeight,
            focus
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

    private static Vector2 ClipPointToRect(Vector2 point, Rect2 rect)
    {
        return new Vector2(
            Mathf.Clamp(point.X, rect.Position.X, rect.Position.X + rect.Size.X),
            Mathf.Clamp(point.Y, rect.Position.Y, rect.Position.Y + rect.Size.Y)
        );
    }

    private static bool TryClipSegmentToViewport(
        WorldMapPosition start,
        WorldMapPosition end,
        WorldMapViewport viewport,
        out WorldMapPosition clippedStart,
        out WorldMapPosition clippedEnd)
    {
        clippedStart = start;
        clippedEnd = end;

        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var t0 = 0.0;
        var t1 = 1.0;

        if (!ClipLine(-dx, start.X - viewport.Origin.X, ref t0, ref t1)
            || !ClipLine(dx, viewport.Right - start.X, ref t0, ref t1)
            || !ClipLine(-dy, start.Y - viewport.Origin.Y, ref t0, ref t1)
            || !ClipLine(dy, viewport.Bottom - start.Y, ref t0, ref t1))
        {
            return false;
        }

        clippedStart = new WorldMapPosition(start.X + (t0 * dx), start.Y + (t0 * dy));
        clippedEnd = new WorldMapPosition(start.X + (t1 * dx), start.Y + (t1 * dy));
        return true;
    }

    private static bool ClipLine(double denominator, double numerator, ref double t0, ref double t1)
    {
        if (Math.Abs(denominator) < double.Epsilon)
        {
            return numerator >= 0;
        }

        var t = numerator / denominator;
        if (denominator < 0)
        {
            if (t > t1)
            {
                return false;
            }

            if (t > t0)
            {
                t0 = t;
            }
        }
        else
        {
            if (t < t0)
            {
                return false;
            }

            if (t < t1)
            {
                t1 = t;
            }
        }

        return true;
    }

    private static Color RoadFillColor(WorldMapRoad road)
    {
        return road.Kind switch
        {
            WorldMapRoadKind.Interstate => InterstateColor,
            WorldMapRoadKind.UsHighway => UsHighwayColor,
            WorldMapRoadKind.StateHighway => StateHighwayColor,
            _ => StateHighwayColor
        };
    }

    private static float RoadFillWidth(WorldMapRoad road)
    {
        var laneBonus = Math.Min(4, Math.Max(0, road.LaneCount - 2)) * 0.7f;
        return road.Kind switch
        {
            WorldMapRoadKind.Interstate => 5.6f + laneBonus,
            WorldMapRoadKind.UsHighway => 4.0f + laneBonus,
            WorldMapRoadKind.StateHighway => 2.8f + (laneBonus * 0.65f),
            _ => 2.4f
        };
    }

    private static float RoadCasingWidth(WorldMapRoad road)
    {
        return road.Kind == WorldMapRoadKind.Interstate ? 2.6f : 2.0f;
    }

    private static Color ColorFromHex(string hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return fallback;
        }

        var value = hex.Trim().TrimStart('#');
        if (value.Length != 6
            || !int.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out var rgb))
        {
            return fallback;
        }

        return new Color(
            ((rgb >> 16) & 0xff) / 255.0f,
            ((rgb >> 8) & 0xff) / 255.0f,
            (rgb & 0xff) / 255.0f,
            0.92f
        );
    }
}
