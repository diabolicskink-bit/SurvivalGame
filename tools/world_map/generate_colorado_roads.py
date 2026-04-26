#!/usr/bin/env python3
"""Generate the curated Colorado world-map road layer from Colorado GIS services."""

from __future__ import annotations

import json
import math
import pathlib
import time
import urllib.parse
import urllib.request


SERVICE_ROOT = "https://gis.colorado.gov/public/rest/services/OIT/Colorado_State_Basemap/MapServer"
INTERSTATE_LAYER = 36
US_HIGHWAY_LAYER = 37
STATE_HIGHWAY_LAYER = 38
HIGHWAY_SEGMENT_LAYER = 39
OUTPUT_RELATIVE_PATH = pathlib.Path("data/world_map/colorado_roads.generated.json")
SIMPLIFY_TOLERANCE_DEGREES = 0.003
PAGE_SIZE = 2000
REPO_ROOT = pathlib.Path(__file__).resolve().parents[2]
OUTPUT_PATH = REPO_ROOT / OUTPUT_RELATIVE_PATH


TARGET_ROUTES = [
    {"id": "i_25", "displayName": "I-25", "kind": "interstate", "priority": 1, "layer": INTERSTATE_LAYER, "label": 25, "defaultLaneCount": 4, "defaultSurfaceWidthFeet": 72},
    {"id": "i_70", "displayName": "I-70", "kind": "interstate", "priority": 1, "layer": INTERSTATE_LAYER, "label": 70, "defaultLaneCount": 4, "defaultSurfaceWidthFeet": 72},
    {"id": "i_76", "displayName": "I-76", "kind": "interstate", "priority": 1, "layer": INTERSTATE_LAYER, "label": 76, "defaultLaneCount": 4, "defaultSurfaceWidthFeet": 64},
    {"id": "us_36", "displayName": "US-36", "kind": "us_highway", "priority": 2, "layer": US_HIGHWAY_LAYER, "label": 36, "defaultLaneCount": 4, "defaultSurfaceWidthFeet": 64},
    {"id": "us_50", "displayName": "US-50", "kind": "us_highway", "priority": 2, "layer": US_HIGHWAY_LAYER, "label": 50, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 32},
    {"id": "us_160", "displayName": "US-160", "kind": "us_highway", "priority": 2, "layer": US_HIGHWAY_LAYER, "label": 160, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 32},
    {"id": "us_285", "displayName": "US-285", "kind": "us_highway", "priority": 2, "layer": US_HIGHWAY_LAYER, "label": 285, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 32},
    {"id": "us_550", "displayName": "US-550", "kind": "us_highway", "priority": 2, "layer": US_HIGHWAY_LAYER, "label": 550, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 28},
    {"id": "us_40", "displayName": "US-40", "kind": "us_highway", "priority": 2, "layer": US_HIGHWAY_LAYER, "label": 40, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 32},
    {"id": "us_24", "displayName": "US-24", "kind": "us_highway", "priority": 2, "layer": US_HIGHWAY_LAYER, "label": 24, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 32},
    {"id": "us_34", "displayName": "US-34", "kind": "us_highway", "priority": 2, "layer": US_HIGHWAY_LAYER, "label": 34, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 32},
    {"id": "us_491", "displayName": "US-491", "kind": "us_highway", "priority": 2, "layer": US_HIGHWAY_LAYER, "label": 491, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 28},
    {"id": "us_287", "displayName": "US-287", "kind": "us_highway", "priority": 2, "layer": US_HIGHWAY_LAYER, "label": 287, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 32},
    {"id": "us_385", "displayName": "US-385", "kind": "us_highway", "priority": 2, "layer": US_HIGHWAY_LAYER, "label": 385, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 28},
    {"id": "co_82", "displayName": "CO-82", "kind": "state_highway", "priority": 3, "layer": STATE_HIGHWAY_LAYER, "label": 82, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 28},
    {"id": "co_9", "displayName": "CO-9", "kind": "state_highway", "priority": 3, "layer": STATE_HIGHWAY_LAYER, "label": 9, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 28},
    {"id": "co_14", "displayName": "CO-14", "kind": "state_highway", "priority": 3, "layer": STATE_HIGHWAY_LAYER, "label": 14, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 28},
    {"id": "co_145", "displayName": "CO-145", "kind": "state_highway", "priority": 3, "layer": STATE_HIGHWAY_LAYER, "label": 145, "defaultLaneCount": 2, "defaultSurfaceWidthFeet": 28},
]


def main() -> None:
    roads = []
    for target in TARGET_ROUTES:
        geometry_features = query_features(
            target["layer"],
            f"LABEL = {target['label']}",
            "ROUTE,REFPT,ENDREFPT,ROUTESIGN,LABEL,Shape_Length",
            return_geometry=True,
        )
        stat_features = query_stat_features(target)
        road = build_road(target, geometry_features, stat_features)
        roads.append(road)

    output = {
        "metadata": {
            "source": "Colorado State Basemap ArcGIS MapServer",
            "sourceUrl": SERVICE_ROOT,
            "geometryLayers": {
                "interstates": f"{SERVICE_ROOT}/{INTERSTATE_LAYER}",
                "usHighways": f"{SERVICE_ROOT}/{US_HIGHWAY_LAYER}",
                "stateHighways": f"{SERVICE_ROOT}/{STATE_HIGHWAY_LAYER}",
            },
            "metadataLayer": f"{SERVICE_ROOT}/{HIGHWAY_SEGMENT_LAYER}",
            "simplificationToleranceDegrees": SIMPLIFY_TOLERANCE_DEGREES,
            "selection": "Curated major Colorado routes used by the V1 world-map road layer.",
        },
        "roads": roads,
    }

    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT_PATH.write_text(json.dumps(output, indent=2) + "\n", encoding="utf-8")


def query_stat_features(target: dict) -> list[dict]:
    if target["kind"] == "interstate":
        return []

    route_sign = "U.S." if target["kind"] == "us_highway" else "SH"
    route_prefix = f"{target['label']:03d}"
    return query_features(
        HIGHWAY_SEGMENT_LAYER,
        f"ROUTESIGN = '{route_sign}' AND ROUTE LIKE '{route_prefix}%'",
        "ROUTE,ROUTESIGN,THRULNQTY,THRULNWD,AADT,FUNCCLASS,PRIDLCLASS,ALIAS",
        return_geometry=False,
    )


def query_features(layer: int, where: str, out_fields: str, return_geometry: bool) -> list[dict]:
    features: list[dict] = []
    offset = 0
    while True:
        params = {
            "where": where,
            "outFields": out_fields,
            "returnGeometry": "true" if return_geometry else "false",
            "f": "geojson" if return_geometry else "json",
            "resultRecordCount": PAGE_SIZE,
            "resultOffset": offset,
        }
        if return_geometry:
            params["outSR"] = "4326"

        url = f"{SERVICE_ROOT}/{layer}/query?{urllib.parse.urlencode(params)}"
        with urllib.request.urlopen(url, timeout=60) as response:
            payload = json.loads(response.read().decode("utf-8"))

        page = payload.get("features", [])
        features.extend(page)
        if len(page) < PAGE_SIZE:
            break

        offset += len(page)
        time.sleep(0.1)

    return features


def build_road(target: dict, geometry_features: list[dict], stat_features: list[dict]) -> dict:
    segments = []
    sorted_features = sorted(
        geometry_features,
        key=lambda feature: (
            feature.get("properties", {}).get("REFPT") or 0,
            feature.get("properties", {}).get("ENDREFPT") or 0,
            feature.get("properties", {}).get("ROUTE") or "",
        ),
    )
    for feature in sorted_features:
        properties = feature.get("properties", {})
        if should_skip_minor_variant(properties):
            continue

        for segment in extract_segments(feature.get("geometry")):
            simplified = simplify(segment, SIMPLIFY_TOLERANCE_DEGREES)
            if len(simplified) < 2:
                continue

            segments.append([
                {"longitude": round(point[0], 5), "latitude": round(point[1], 5)}
                for point in simplified
            ])

    if not segments:
        raise RuntimeError(f"No usable geometry returned for {target['displayName']}")

    lane_count = choose_lane_count(stat_features, target["defaultLaneCount"])
    surface_width_feet = choose_surface_width(stat_features, target["defaultSurfaceWidthFeet"])

    return {
        "id": target["id"],
        "displayName": target["displayName"],
        "kind": target["kind"],
        "priority": target["priority"],
        "laneCount": lane_count,
        "surfaceWidthFeet": surface_width_feet,
        "travelInfluenceRadius": travel_influence_radius(target["kind"], lane_count),
        "segments": segments,
    }


def should_skip_minor_variant(properties: dict) -> bool:
    refpt = number_or_none(properties.get("REFPT"))
    end_refpt = number_or_none(properties.get("ENDREFPT"))
    shape_length = number_or_none(properties.get("Shape_Length")) or number_or_none(properties.get("SHAPE_Length"))
    if refpt is None or end_refpt is None:
        return False

    ref_span = abs(end_refpt - refpt)
    if ref_span < 0.75 and shape_length is not None and shape_length < 1800:
        return True

    return False


def extract_segments(geometry: dict | None) -> list[list[tuple[float, float]]]:
    if not geometry:
        return []

    geometry_type = geometry.get("type")
    coordinates = geometry.get("coordinates") or []
    if geometry_type == "LineString":
        return [to_points(coordinates)]

    if geometry_type == "MultiLineString":
        return [to_points(segment) for segment in coordinates]

    return []


def to_points(coordinates: list) -> list[tuple[float, float]]:
    return [
        (float(point[0]), float(point[1]))
        for point in coordinates
        if isinstance(point, list) and len(point) >= 2
    ]


def simplify(points: list[tuple[float, float]], tolerance: float) -> list[tuple[float, float]]:
    if len(points) <= 2:
        return points

    keep = [False] * len(points)
    keep[0] = True
    keep[-1] = True
    simplify_range(points, 0, len(points) - 1, tolerance, keep)
    return [point for point, should_keep in zip(points, keep) if should_keep]


def simplify_range(points: list[tuple[float, float]], start: int, end: int, tolerance: float, keep: list[bool]) -> None:
    best_distance = 0.0
    best_index = start
    for index in range(start + 1, end):
        distance = perpendicular_distance(points[index], points[start], points[end])
        if distance > best_distance:
            best_distance = distance
            best_index = index

    if best_distance <= tolerance:
        return

    keep[best_index] = True
    simplify_range(points, start, best_index, tolerance, keep)
    simplify_range(points, best_index, end, tolerance, keep)


def perpendicular_distance(
    point: tuple[float, float],
    start: tuple[float, float],
    end: tuple[float, float],
) -> float:
    dx = end[0] - start[0]
    dy = end[1] - start[1]
    length_squared = (dx * dx) + (dy * dy)
    if length_squared == 0:
        return math.dist(point, start)

    t = (((point[0] - start[0]) * dx) + ((point[1] - start[1]) * dy)) / length_squared
    t = max(0.0, min(1.0, t))
    nearest = (start[0] + (t * dx), start[1] + (t * dy))
    return math.dist(point, nearest)


def choose_lane_count(features: list[dict], fallback: int) -> int:
    lanes = [
        int(value)
        for feature in features
        if (value := feature.get("attributes", {}).get("THRULNQTY")) is not None and int(value) > 0
    ]
    if not lanes:
        return fallback

    return max(1, min(8, max(lanes)))


def choose_surface_width(features: list[dict], fallback: float) -> float:
    widths = []
    for feature in features:
        attributes = feature.get("attributes", {})
        lanes = number_or_none(attributes.get("THRULNQTY"))
        lane_width = number_or_none(attributes.get("THRULNWD")) or 12.0
        if lanes is not None and lanes > 0:
            widths.append(lanes * lane_width)

    if not widths:
        return fallback

    return round(max(widths), 1)


def travel_influence_radius(kind: str, lane_count: int) -> int:
    if kind == "interstate":
        return 136 + max(0, lane_count - 4) * 8

    if kind == "us_highway":
        return 112 + max(0, lane_count - 2) * 6

    if kind == "state_highway":
        return 88 + max(0, lane_count - 2) * 6

    return 72


def number_or_none(value: object) -> float | None:
    if value is None:
        return None

    try:
        return float(value)
    except (TypeError, ValueError):
        return None


if __name__ == "__main__":
    main()
