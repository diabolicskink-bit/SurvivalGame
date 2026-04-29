#!/usr/bin/env python3
"""Generate the Colorado tactical atlas background and shared terrain-cost grid.

The generated files are intentionally deterministic: the visual atlas and the
gameplay terrain grid come from the same curated Colorado-shaped masks.
"""

from __future__ import annotations

import json
import math
import pathlib
from typing import Iterable, Sequence

from PIL import Image, ImageChops, ImageDraw, ImageFilter


REPO_ROOT = pathlib.Path(__file__).resolve().parents[2]
MAP_PATH = REPO_ROOT / "data/world_map/colorado.json"
ATLAS_PATH = REPO_ROOT / "data/world_map/colorado_atlas.png"
TERRAIN_PATH = REPO_ROOT / "data/world_map/colorado_terrain.generated.json"

GRID_WIDTH = 520
GRID_HEIGHT = 380
ATLAS_SCALE = 0.5

TERRAIN_LEGEND = {
    "G": {
        "kind": "ShortgrassPrairie",
        "displayName": "Shortgrass prairie",
        "speedMultiplier": 1.0,
        "fuelUseMultiplier": 1.0,
    },
    "H": {
        "kind": "HighPlains",
        "displayName": "High plains",
        "speedMultiplier": 0.95,
        "fuelUseMultiplier": 1.05,
    },
    "F": {
        "kind": "FrontRangeCorridor",
        "displayName": "Front Range corridor",
        "speedMultiplier": 1.05,
        "fuelUseMultiplier": 0.95,
    },
    "O": {
        "kind": "Foothills",
        "displayName": "Foothills",
        "speedMultiplier": 0.88,
        "fuelUseMultiplier": 1.12,
    },
    "M": {
        "kind": "MountainForest",
        "displayName": "Mountain forest",
        "speedMultiplier": 0.62,
        "fuelUseMultiplier": 1.35,
    },
    "A": {
        "kind": "AlpinePeaks",
        "displayName": "Alpine peaks",
        "speedMultiplier": 0.48,
        "fuelUseMultiplier": 1.55,
    },
    "V": {
        "kind": "MountainValley",
        "displayName": "Mountain valley",
        "speedMultiplier": 0.78,
        "fuelUseMultiplier": 1.18,
    },
    "T": {
        "kind": "WesternPlateau",
        "displayName": "Western plateau",
        "speedMultiplier": 0.9,
        "fuelUseMultiplier": 1.12,
    },
    "C": {
        "kind": "Canyonlands",
        "displayName": "Canyonlands",
        "speedMultiplier": 0.72,
        "fuelUseMultiplier": 1.32,
    },
    "D": {
        "kind": "DesertScrub",
        "displayName": "Desert scrub",
        "speedMultiplier": 0.86,
        "fuelUseMultiplier": 1.14,
    },
    "R": {
        "kind": "River",
        "displayName": "River corridor",
        "speedMultiplier": 0.42,
        "fuelUseMultiplier": 1.65,
    },
    "L": {
        "kind": "Reservoir",
        "displayName": "Reservoir",
        "speedMultiplier": 0.35,
        "fuelUseMultiplier": 1.8,
    },
}

BASE_COLORS = {
    "G": (102, 118, 67),
    "H": (122, 116, 72),
    "F": (76, 112, 72),
    "O": (78, 95, 63),
    "M": (48, 78, 56),
    "A": (150, 153, 142),
    "V": (111, 122, 76),
    "T": (137, 103, 64),
    "C": (148, 78, 50),
    "D": (119, 105, 78),
    "R": (48, 107, 130),
    "L": (38, 95, 126),
}

RANGE_SPINES = [
    (
        "Front Range",
        [(-105.72, 40.95), (-105.58, 40.30), (-105.42, 39.70), (-105.30, 39.05), (-105.20, 38.20)],
        0.22,
        0.95,
    ),
    (
        "Sawatch",
        [(-106.68, 39.45), (-106.50, 39.05), (-106.35, 38.55), (-106.15, 38.05)],
        0.27,
        1.10,
    ),
    (
        "Mosquito",
        [(-106.18, 39.45), (-106.05, 39.05), (-105.95, 38.65)],
        0.18,
        0.82,
    ),
    (
        "Sangre de Cristo",
        [(-105.74, 38.55), (-105.55, 38.05), (-105.38, 37.50), (-105.25, 37.05)],
        0.20,
        0.95,
    ),
    (
        "San Juan",
        [(-108.05, 37.15), (-107.65, 37.42), (-107.25, 37.70), (-106.82, 38.05)],
        0.31,
        1.08,
    ),
    (
        "Elk and West Elk",
        [(-107.48, 39.25), (-107.18, 39.05), (-106.75, 38.82)],
        0.24,
        0.92,
    ),
    (
        "Park and Gore",
        [(-106.55, 40.35), (-106.25, 39.98), (-106.05, 39.58)],
        0.28,
        0.82,
    ),
]

PEAKS = [
    (-106.45, 39.12, 0.23, 0.46),  # Mount Elbert / Massive area
    (-105.62, 40.25, 0.20, 0.40),  # Longs Peak
    (-105.04, 38.84, 0.18, 0.34),  # Pikes Peak
    (-107.08, 39.07, 0.20, 0.38),  # Maroon Bells / Elk
    (-107.78, 37.84, 0.23, 0.42),  # San Juan high country
    (-106.93, 38.87, 0.18, 0.30),  # Crested Butte
]

RIVERS = [
    {
        "name": "Colorado River",
        "width": 0.020,
        "points": [
            (-105.82, 40.27),
            (-106.22, 39.92),
            (-106.82, 39.65),
            (-107.35, 39.55),
            (-107.90, 39.45),
            (-108.55, 39.10),
            (-109.06, 39.10),
        ],
    },
    {
        "name": "Arkansas River",
        "width": 0.018,
        "points": [
            (-106.34, 39.25),
            (-106.13, 38.84),
            (-105.80, 38.55),
            (-105.24, 38.44),
            (-104.60, 38.28),
            (-103.80, 38.10),
            (-102.04, 38.12),
        ],
    },
    {
        "name": "South Platte River",
        "width": 0.017,
        "points": [
            (-105.55, 39.30),
            (-105.25, 39.55),
            (-104.99, 39.74),
            (-104.55, 40.05),
            (-104.05, 40.35),
            (-103.35, 40.72),
            (-102.04, 40.88),
        ],
    },
    {
        "name": "Rio Grande",
        "width": 0.017,
        "points": [
            (-107.05, 37.85),
            (-106.62, 37.72),
            (-106.25, 37.58),
            (-105.88, 37.45),
            (-105.55, 37.10),
        ],
    },
    {
        "name": "Gunnison River",
        "width": 0.017,
        "points": [
            (-106.90, 38.55),
            (-107.20, 38.47),
            (-107.65, 38.55),
            (-108.10, 38.72),
            (-108.55, 38.77),
        ],
    },
    {
        "name": "Yampa River",
        "width": 0.016,
        "points": [
            (-106.80, 40.45),
            (-107.10, 40.52),
            (-107.55, 40.48),
            (-108.20, 40.50),
            (-108.85, 40.55),
        ],
    },
    {
        "name": "Dolores River",
        "width": 0.014,
        "points": [
            (-107.88, 37.48),
            (-108.32, 37.65),
            (-108.70, 38.00),
            (-108.95, 38.45),
        ],
    },
    {
        "name": "San Juan River",
        "width": 0.014,
        "points": [
            (-107.02, 37.27),
            (-107.45, 37.20),
            (-108.05, 37.10),
            (-108.75, 37.02),
        ],
    },
]

RESERVOIRS = [
    ("Blue Mesa Reservoir", -107.20, 38.46, 0.36, 0.09, -6),
    ("Dillon Reservoir", -106.06, 39.60, 0.16, 0.09, 12),
    ("Pueblo Reservoir", -104.78, 38.27, 0.26, 0.08, -4),
    ("Horsetooth Reservoir", -105.17, 40.55, 0.07, 0.22, -12),
    ("Lake Granby", -105.84, 40.16, 0.20, 0.13, 8),
    ("McPhee Reservoir", -108.58, 37.53, 0.22, 0.11, -20),
    ("John Martin Reservoir", -102.93, 38.07, 0.24, 0.08, 4),
    ("Eleven Mile Reservoir", -105.52, 38.92, 0.16, 0.09, 6),
    ("Green Mountain Reservoir", -106.33, 39.88, 0.09, 0.15, -10),
    ("Navajo Reservoir", -107.55, 37.03, 0.26, 0.09, 10),
    ("Chatfield Reservoir", -105.07, 39.54, 0.07, 0.05, 0),
]

VALLEY_BOXES = [
    (-106.35, -105.35, 37.05, 38.18),  # San Luis Valley
    (-106.25, -105.38, 38.72, 39.34),  # South Park
    (-106.72, -105.72, 40.33, 40.96),  # North Park
    (-107.35, -106.65, 38.25, 38.75),  # Gunnison basin
]

CANYON_LINES = [
    [(-108.90, 39.05), (-108.55, 39.10), (-108.15, 39.22), (-107.90, 39.45)],
    [(-108.50, 38.75), (-108.10, 38.70), (-107.65, 38.55), (-107.25, 38.47)],
    [(-108.95, 38.45), (-108.70, 38.00), (-108.32, 37.65), (-107.88, 37.48)],
]


def main() -> None:
    with MAP_PATH.open("r", encoding="utf-8") as source:
        world_map = json.load(source)

    bounds = world_map["projection"]["bounds"]
    atlas_width = round(world_map["mapWidth"] * ATLAS_SCALE)
    atlas_height = round(world_map["mapHeight"] * ATLAS_SCALE)

    rows = build_terrain_rows(bounds)
    write_terrain_grid(rows, atlas_width, atlas_height)
    render_atlas(bounds, rows, atlas_width, atlas_height)


def build_terrain_rows(bounds: dict) -> list[str]:
    rows = []
    for row in range(GRID_HEIGHT):
        latitude = interpolate(bounds["maxLatitude"], bounds["minLatitude"], (row + 0.5) / GRID_HEIGHT)
        codes = []
        for column in range(GRID_WIDTH):
            longitude = interpolate(bounds["minLongitude"], bounds["maxLongitude"], (column + 0.5) / GRID_WIDTH)
            codes.append(classify_terrain(longitude, latitude))
        rows.append("".join(codes))

    return rows


def write_terrain_grid(rows: list[str], atlas_width: int, atlas_height: int) -> None:
    terrain = {
        "metadata": {
            "source": "Deterministic pseudo-accurate Colorado tactical atlas generator.",
            "atlasWidth": atlas_width,
            "atlasHeight": atlas_height,
            "cellSizeMapUnits": 20,
            "selection": (
                "V2 illustrated gameplay terrain grid with Colorado-specific rivers, "
                "reservoirs, plains, foothills, mountains, valleys, plateau, and canyon country."
            ),
        },
        "width": GRID_WIDTH,
        "height": GRID_HEIGHT,
        "legend": TERRAIN_LEGEND,
        "rows": rows,
    }
    TERRAIN_PATH.write_text(json.dumps(terrain, indent=2) + "\n", encoding="utf-8")


def render_atlas(bounds: dict, rows: list[str], atlas_width: int, atlas_height: int) -> None:
    base = Image.new("RGB", (GRID_WIDTH, GRID_HEIGHT))
    pixels = base.load()
    for y in range(GRID_HEIGHT):
        latitude = interpolate(bounds["maxLatitude"], bounds["minLatitude"], (y + 0.5) / GRID_HEIGHT)
        for x in range(GRID_WIDTH):
            longitude = interpolate(bounds["minLongitude"], bounds["maxLongitude"], (x + 0.5) / GRID_WIDTH)
            code = rows[y][x]
            pixels[x, y] = atlas_cell_color(code, longitude, latitude, x, y)

    image = base.resize((atlas_width, atlas_height), resample=Image.Resampling.BICUBIC)
    image = apply_hillshade(image, bounds, atlas_width, atlas_height)
    image = apply_paper_texture(image, atlas_width, atlas_height)
    draw = ImageDraw.Draw(image, "RGBA")

    draw_canyon_art(draw, bounds, atlas_width, atlas_height)
    draw_forest_art(draw, bounds, rows, atlas_width, atlas_height)
    draw_alpine_art(draw, bounds, atlas_width, atlas_height)
    draw_prairie_art(draw, bounds, rows, atlas_width, atlas_height)
    draw_rivers(draw, bounds, atlas_width, atlas_height)
    draw_reservoirs(draw, bounds, atlas_width, atlas_height)
    draw_state_border(draw, atlas_width, atlas_height)

    image = image.filter(ImageFilter.UnsharpMask(radius=1.2, percent=80, threshold=4))
    ATLAS_PATH.parent.mkdir(parents=True, exist_ok=True)
    image.save(ATLAS_PATH, optimize=True)


def atlas_cell_color(code: str, longitude: float, latitude: float, x: int, y: int) -> tuple[int, int, int]:
    base = BASE_COLORS[code]
    elevation = elevation_score(longitude, latitude)
    grain = deterministic_noise(x * 17, y * 17)
    broad = smooth_noise(longitude * 1.7, latitude * 1.9)
    ridge = math.sin((longitude + 106.8) * 42.0 + (latitude * 6.5)) * 0.5
    ridge += math.sin((longitude * 24.0) - (latitude * 19.0)) * 0.35

    shade = 0.94 + (grain * 0.07) + (broad * 0.055)
    if code in {"A", "M", "O"}:
        shade += elevation * 0.12 + ridge * 0.045
    elif code in {"C", "D", "T"}:
        shade += ridge * 0.035
    elif code in {"R", "L"}:
        shade = 0.95 + grain * 0.035

    return tuple(clamp_color(channel * shade) for channel in base)


def classify_terrain(longitude: float, latitude: float) -> str:
    if is_reservoir(longitude, latitude):
        return "L"

    if distance_to_any_river(longitude, latitude) < river_threshold(longitude, latitude):
        return "R"

    elevation = elevation_score(longitude, latitude)
    local_texture = smooth_noise(longitude * 3.1, latitude * 4.3) * 0.08
    noisy_elevation = elevation + local_texture

    if is_mountain_valley(longitude, latitude) and noisy_elevation < 0.72:
        return "V"

    if is_foothills(longitude, latitude, noisy_elevation) and noisy_elevation < 0.74:
        return "O"

    if noisy_elevation > 0.80:
        return "A"

    if noisy_elevation > 0.50:
        return "M"

    if is_foothills(longitude, latitude, noisy_elevation):
        return "O"

    if longitude <= -107.18:
        canyon = canyon_score(longitude, latitude)
        if canyon > 0.58:
            return "C"

        if latitude < 38.25 or (longitude < -108.45 and smooth_noise(longitude * 2.6, latitude * 2.8) > -0.10):
            return "D"

        return "T"

    if is_front_range_corridor(longitude, latitude):
        return "F"

    if latitude > 39.85 or longitude > -103.70 or (latitude < 38.20 and longitude > -103.50):
        return "H"

    return "G"


def elevation_score(longitude: float, latitude: float) -> float:
    score = 0.0
    for _, points, width, weight in RANGE_SPINES:
        distance = polyline_distance(longitude, latitude, points)
        score = max(score, weight * math.exp(-((distance / width) ** 2)))

    for peak_longitude, peak_latitude, radius, weight in PEAKS:
        distance = point_distance(longitude, latitude, peak_longitude, peak_latitude)
        peak_strength = min(1.0, 0.68 + weight)
        score = max(score, peak_strength * math.exp(-((distance / radius) ** 2)))

    return min(1.0, score)


def canyon_score(longitude: float, latitude: float) -> float:
    score = 0.0
    for line in CANYON_LINES:
        distance = polyline_distance(longitude, latitude, line)
        score = max(score, math.exp(-((distance / 0.12) ** 2)))

    score = max(score, 0.55 * math.exp(-((point_distance(longitude, latitude, -108.65, 39.05) / 0.45) ** 2)))
    score = max(score, 0.70 * math.exp(-((point_distance(longitude, latitude, -108.50, 37.35) / 0.38) ** 2)))
    return score


def is_reservoir(longitude: float, latitude: float) -> bool:
    return any(in_rotated_ellipse(longitude, latitude, entry) for entry in RESERVOIRS)


def is_mountain_valley(longitude: float, latitude: float) -> bool:
    if any(west <= longitude <= east and south <= latitude <= north for west, east, south, north in VALLEY_BOXES):
        return True

    valley_lines = [
        [(-106.34, 39.25), (-106.12, 38.82), (-105.80, 38.55), (-105.55, 38.42)],
        [(-106.90, 38.55), (-107.20, 38.47), (-107.65, 38.55)],
    ]
    return any(polyline_distance(longitude, latitude, line) < 0.08 for line in valley_lines)


def is_foothills(longitude: float, latitude: float, elevation: float) -> bool:
    east_slope = -105.48 <= longitude <= -105.02 and 37.85 <= latitude <= 40.95
    west_slope = -107.35 <= longitude <= -106.88 and 37.30 <= latitude <= 39.70
    return (east_slope or west_slope) and elevation > 0.16


def is_front_range_corridor(longitude: float, latitude: float) -> bool:
    western_edge = -105.08 + max(0.0, 38.4 - latitude) * 0.06
    eastern_edge = -104.36 + max(0.0, latitude - 40.0) * 0.10
    return western_edge <= longitude <= eastern_edge and 37.85 <= latitude <= 40.95


def distance_to_any_river(longitude: float, latitude: float) -> float:
    return min(polyline_distance(longitude, latitude, river["points"]) for river in RIVERS)


def river_threshold(longitude: float, latitude: float) -> float:
    for river in RIVERS:
        distance = polyline_distance(longitude, latitude, river["points"])
        if distance < river["width"] * 1.8:
            return river["width"]

    return 0.014


def apply_hillshade(image: Image.Image, bounds: dict, atlas_width: int, atlas_height: int) -> Image.Image:
    shade = Image.new("L", (GRID_WIDTH, GRID_HEIGHT))
    shade_pixels = shade.load()
    for y in range(GRID_HEIGHT):
        latitude = interpolate(bounds["maxLatitude"], bounds["minLatitude"], (y + 0.5) / GRID_HEIGHT)
        for x in range(GRID_WIDTH):
            longitude = interpolate(bounds["minLongitude"], bounds["maxLongitude"], (x + 0.5) / GRID_WIDTH)
            center = elevation_score(longitude, latitude)
            east = elevation_score(longitude + 0.025, latitude)
            south = elevation_score(longitude, latitude - 0.025)
            relief = ((center - east) * 110.0) + ((south - center) * 85.0)
            value = clamp_color(128 + relief + (center * 24.0))
            shade_pixels[x, y] = value

    shade = shade.resize((atlas_width, atlas_height), resample=Image.Resampling.BICUBIC)
    shade = shade.filter(ImageFilter.GaussianBlur(radius=2.1))

    shadow = Image.new("RGBA", image.size, (12, 18, 16, 0))
    shadow.putalpha(shade.point(lambda value: max(0, 126 - value) * 2))
    highlight = Image.new("RGBA", image.size, (236, 232, 210, 0))
    highlight.putalpha(shade.point(lambda value: max(0, value - 138) * 2))

    image = image.convert("RGBA")
    image.alpha_composite(shadow)
    image.alpha_composite(highlight)
    return image.convert("RGB")


def apply_paper_texture(image: Image.Image, atlas_width: int, atlas_height: int) -> Image.Image:
    noise = Image.effect_noise((atlas_width, atlas_height), 18).convert("L")
    noise = noise.point(lambda value: 118 + int((value - 128) * 0.16))
    texture = Image.merge("RGB", (noise, noise, noise))
    image = ImageChops.multiply(image, texture)
    warm = Image.new("RGB", image.size, (19, 15, 8))
    return Image.blend(image, warm, 0.035)


def draw_forest_art(
    draw: ImageDraw.ImageDraw,
    bounds: dict,
    rows: list[str],
    atlas_width: int,
    atlas_height: int,
) -> None:
    step = 18
    for y in range(7, atlas_height, step):
        grid_y = min(GRID_HEIGHT - 1, y * GRID_HEIGHT // atlas_height)
        for x in range(7, atlas_width, step):
            grid_x = min(GRID_WIDTH - 1, x * GRID_WIDTH // atlas_width)
            code = rows[grid_y][grid_x]
            if code not in {"M", "O"}:
                continue

            roll = deterministic_unit(x, y)
            threshold = 0.52 if code == "M" else 0.72
            if roll > threshold:
                continue

            jitter_x = int((deterministic_unit(x + 11, y) - 0.5) * 10)
            jitter_y = int((deterministic_unit(x, y + 13) - 0.5) * 10)
            px = x + jitter_x
            py = y + jitter_y
            size = 3 + int(deterministic_unit(x + 29, y + 31) * 4)
            color = (26, 56, 36, 90) if code == "M" else (43, 66, 39, 75)
            draw.polygon(
                [(px, py - size), (px - size, py + size), (px + size, py + size)],
                fill=color,
            )
            draw.line([(px, py + size), (px, py + size + 3)], fill=(35, 28, 18, 55), width=1)


def draw_alpine_art(draw: ImageDraw.ImageDraw, bounds: dict, atlas_width: int, atlas_height: int) -> None:
    for name, points, _, _ in RANGE_SPINES:
        screen_points = [project(bounds, lon, lat, atlas_width, atlas_height) for lon, lat in points]
        width = 5 if name in {"Sawatch", "San Juan", "Front Range"} else 3
        draw.line(screen_points, fill=(47, 56, 50, 82), width=width + 4, joint="curve")
        draw.line(screen_points, fill=(202, 205, 190, 72), width=width, joint="curve")

        for i in range(len(screen_points) - 1):
            x1, y1 = screen_points[i]
            x2, y2 = screen_points[i + 1]
            for t in (0.25, 0.50, 0.75):
                px = x1 + ((x2 - x1) * t)
                py = y1 + ((y2 - y1) * t)
                draw.line(
                    [(px, py), (px + 28, py + 13)],
                    fill=(230, 230, 214, 44),
                    width=2,
                )

    for lon, lat, _, _ in PEAKS:
        x, y = project(bounds, lon, lat, atlas_width, atlas_height)
        draw.polygon(
            [(x, y - 18), (x - 18, y + 16), (x + 18, y + 16)],
            fill=(215, 215, 200, 66),
        )
        draw.line([(x, y - 16), (x - 6, y + 7)], fill=(255, 255, 244, 92), width=2)
        draw.line([(x, y - 16), (x + 7, y + 8)], fill=(69, 76, 70, 52), width=2)


def draw_canyon_art(draw: ImageDraw.ImageDraw, bounds: dict, atlas_width: int, atlas_height: int) -> None:
    for line in CANYON_LINES:
        points = [project(bounds, lon, lat, atlas_width, atlas_height) for lon, lat in line]
        draw.line(points, fill=(93, 43, 28, 98), width=10, joint="curve")
        draw.line(points, fill=(198, 111, 62, 78), width=3, joint="curve")

        for i in range(len(points) - 1):
            x1, y1 = points[i]
            x2, y2 = points[i + 1]
            for index in range(10):
                t = (index + 0.5) / 10.0
                px = x1 + ((x2 - x1) * t)
                py = y1 + ((y2 - y1) * t)
                draw.line(
                    [(px - 16, py - 8), (px + 10, py + 8)],
                    fill=(78, 37, 24, 58),
                    width=2,
                )
                draw.line(
                    [(px + 14, py - 9), (px - 8, py + 7)],
                    fill=(230, 142, 84, 42),
                    width=1,
                )


def draw_prairie_art(
    draw: ImageDraw.ImageDraw,
    bounds: dict,
    rows: list[str],
    atlas_width: int,
    atlas_height: int,
) -> None:
    step = 28
    for y in range(12, atlas_height, step):
        grid_y = min(GRID_HEIGHT - 1, y * GRID_HEIGHT // atlas_height)
        for x in range(12, atlas_width, step):
            grid_x = min(GRID_WIDTH - 1, x * GRID_WIDTH // atlas_width)
            code = rows[grid_y][grid_x]
            if code not in {"G", "H", "D", "T", "F", "V"}:
                continue

            if deterministic_unit(x + 5, y + 7) > 0.34:
                continue

            length = 8 + int(deterministic_unit(x + 19, y + 23) * 11)
            color = (233, 220, 154, 30) if code in {"G", "H", "V"} else (86, 59, 38, 35)
            draw.line([(x - length, y), (x + length, y + 2)], fill=color, width=1)


def draw_rivers(draw: ImageDraw.ImageDraw, bounds: dict, atlas_width: int, atlas_height: int) -> None:
    for river in RIVERS:
        points = [project(bounds, lon, lat, atlas_width, atlas_height) for lon, lat in river["points"]]
        major = river["name"] in {"Colorado River", "Arkansas River", "South Platte River"}
        casing_width = 9 if major else 7
        fill_width = 4 if major else 3
        draw.line(points, fill=(18, 48, 59, 132), width=casing_width, joint="curve")
        draw.line(points, fill=(65, 135, 160, 170), width=fill_width, joint="curve")
        draw.line(points, fill=(143, 203, 215, 64), width=1, joint="curve")


def draw_reservoirs(draw: ImageDraw.ImageDraw, bounds: dict, atlas_width: int, atlas_height: int) -> None:
    for reservoir in RESERVOIRS:
        polygon = reservoir_polygon(bounds, reservoir, atlas_width, atlas_height)
        draw.polygon(polygon, fill=(31, 94, 125, 218))
        draw.line(polygon + [polygon[0]], fill=(147, 184, 174, 105), width=2)
        inner = shrink_polygon(polygon, 0.86)
        draw.line(inner + [inner[0]], fill=(88, 153, 177, 82), width=1)


def draw_state_border(draw: ImageDraw.ImageDraw, atlas_width: int, atlas_height: int) -> None:
    draw.rectangle(
        [(0, 0), (atlas_width - 1, atlas_height - 1)],
        outline=(191, 201, 158, 82),
        width=5,
    )


def project(bounds: dict, longitude: float, latitude: float, width: int, height: int) -> tuple[float, float]:
    x = (longitude - bounds["minLongitude"]) / (bounds["maxLongitude"] - bounds["minLongitude"]) * width
    y = (bounds["maxLatitude"] - latitude) / (bounds["maxLatitude"] - bounds["minLatitude"]) * height
    return (x, y)


def reservoir_polygon(
    bounds: dict,
    reservoir: tuple[str, float, float, float, float, float],
    atlas_width: int,
    atlas_height: int,
) -> list[tuple[float, float]]:
    name, center_lon, center_lat, lon_radius, lat_radius, angle_degrees = reservoir
    del name
    points = []
    angle = math.radians(angle_degrees)
    for index in range(28):
        theta = index / 28.0 * math.tau
        wobble = 0.78 + (0.16 * math.sin(theta * 3.0 + center_lon)) + (0.10 * math.sin(theta * 5.0 + center_lat))
        dx = math.cos(theta) * lon_radius * wobble
        dy = math.sin(theta) * lat_radius * wobble
        lon = center_lon + (dx * math.cos(angle) - dy * math.sin(angle))
        lat = center_lat + (dx * math.sin(angle) + dy * math.cos(angle))
        points.append(project(bounds, lon, lat, atlas_width, atlas_height))

    return points


def shrink_polygon(points: list[tuple[float, float]], scale: float) -> list[tuple[float, float]]:
    center_x = sum(point[0] for point in points) / len(points)
    center_y = sum(point[1] for point in points) / len(points)
    return [
        (center_x + ((x - center_x) * scale), center_y + ((y - center_y) * scale))
        for x, y in points
    ]


def in_rotated_ellipse(
    longitude: float,
    latitude: float,
    reservoir: tuple[str, float, float, float, float, float],
) -> bool:
    _, center_lon, center_lat, lon_radius, lat_radius, angle_degrees = reservoir
    angle = math.radians(-angle_degrees)
    dx = longitude - center_lon
    dy = latitude - center_lat
    rx = dx * math.cos(angle) - dy * math.sin(angle)
    ry = dx * math.sin(angle) + dy * math.cos(angle)
    wobble = 1.0 + (0.08 * smooth_noise(longitude * 16.0, latitude * 16.0))
    return ((rx / (lon_radius * wobble)) ** 2) + ((ry / (lat_radius * wobble)) ** 2) <= 1.0


def polyline_distance(
    longitude: float,
    latitude: float,
    points: Sequence[tuple[float, float]],
) -> float:
    return min(
        segment_distance(longitude, latitude, points[index], points[index + 1])
        for index in range(len(points) - 1)
    )


def segment_distance(
    longitude: float,
    latitude: float,
    start: tuple[float, float],
    end: tuple[float, float],
) -> float:
    scale = math.cos(math.radians(latitude))
    px = longitude * scale
    py = latitude
    ax = start[0] * scale
    ay = start[1]
    bx = end[0] * scale
    by = end[1]
    dx = bx - ax
    dy = by - ay
    length_squared = (dx * dx) + (dy * dy)
    if length_squared <= 0:
        return math.sqrt(((px - ax) ** 2) + ((py - ay) ** 2))

    t = max(0.0, min(1.0, (((px - ax) * dx) + ((py - ay) * dy)) / length_squared))
    cx = ax + (dx * t)
    cy = ay + (dy * t)
    return math.sqrt(((px - cx) ** 2) + ((py - cy) ** 2))


def point_distance(longitude: float, latitude: float, target_longitude: float, target_latitude: float) -> float:
    scale = math.cos(math.radians(latitude))
    return math.sqrt((((longitude - target_longitude) * scale) ** 2) + ((latitude - target_latitude) ** 2))


def smooth_noise(x: float, y: float) -> float:
    return (
        math.sin((x * 2.31) + (y * 1.17))
        + (0.55 * math.sin((x * 5.13) - (y * 3.47)))
        + (0.35 * math.sin((x * 11.11) + (y * 7.73)))
    ) / 1.90


def deterministic_noise(x: int, y: int) -> float:
    return deterministic_unit(x, y) - 0.5


def deterministic_unit(x: int, y: int) -> float:
    value = (x * 374761393 + y * 668265263) & 0xFFFFFFFF
    value = (value ^ (value >> 13)) * 1274126177 & 0xFFFFFFFF
    return (value & 0xFFFF) / 65535.0


def interpolate(start: float, end: float, amount: float) -> float:
    return start + ((end - start) * amount)


def clamp_color(value: float) -> int:
    return max(0, min(255, round(value)))


if __name__ == "__main__":
    main()
