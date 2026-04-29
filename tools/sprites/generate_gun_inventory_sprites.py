#!/usr/bin/env python3
"""Generate transparent inventory sprites for prototype firearms."""

from __future__ import annotations

import hashlib
import pathlib
from dataclasses import dataclass
from typing import Callable

from PIL import Image, ImageDraw, ImageFilter


REPO_ROOT = pathlib.Path(__file__).resolve().parents[2]
SPRITE_DIR = REPO_ROOT / "data/sprites/items"
SCALE = 4


@dataclass(frozen=True)
class SpriteSpec:
    filename: str
    width: int
    height: int
    draw: Callable[[ImageDraw.ImageDraw], None]


def main() -> None:
    SPRITE_DIR.mkdir(parents=True, exist_ok=True)
    for spec in specs():
        image = Image.new("RGBA", (spec.width * SCALE, spec.height * SCALE), (0, 0, 0, 0))
        draw = ImageDraw.Draw(image, "RGBA")
        spec.draw(draw)
        image = image.filter(ImageFilter.UnsharpMask(radius=0.6 * SCALE, percent=70, threshold=3))
        image = image.resize((spec.width, spec.height), Image.Resampling.LANCZOS)
        image = trim_alpha(image, padding=8)
        output_path = SPRITE_DIR / spec.filename
        image.save(output_path, optimize=True)
        write_import_file(output_path)


def specs() -> list[SpriteSpec]:
    return [
        SpriteSpec("item_pistol_9mm.png", 420, 260, draw_pistol),
        SpriteSpec("item_hunting_rifle.png", 780, 300, draw_hunting_rifle),
        SpriteSpec("item_shotgun_12_gauge.png", 780, 300, draw_shotgun),
    ]


def draw_pistol(draw: ImageDraw.ImageDraw) -> None:
    s = SCALE
    shadow(draw, [(66, 152), (330, 152), (356, 174), (92, 184)], blur=False)

    outline = (19, 20, 19, 255)
    steel_dark = (42, 45, 45, 255)
    steel = (79, 85, 84, 255)
    steel_light = (129, 135, 130, 230)
    grip = (70, 49, 39, 255)
    grip_light = (122, 76, 50, 210)

    rounded(draw, 62, 86, 270, 125, 11, outline)
    rounded(draw, 78, 94, 262, 116, 7, steel)
    rect(draw, 262, 98, 347, 115, outline)
    rect(draw, 270, 102, 336, 111, steel_dark)
    rect(draw, 334, 98, 356, 118, outline)
    rect(draw, 340, 103, 352, 113, steel)

    rect(draw, 76, 125, 202, 150, outline)
    rect(draw, 88, 128, 196, 143, steel_dark)
    rounded(draw, 158, 138, 192, 178, 7, outline)
    rounded(draw, 166, 143, 185, 169, 4, (31, 33, 33, 255))
    rounded(draw, 96, 123, 144, 162, 6, outline)
    rounded(draw, 106, 127, 137, 153, 4, steel)

    polygon(draw, [(130, 145), (190, 145), (205, 228), (142, 228)], outline)
    polygon(draw, [(143, 153), (180, 153), (192, 216), (154, 216)], grip)
    for y in range(162, 212, 14):
        line(draw, 151, y, 185, y - 3, (38, 28, 24, 170), 2)
        line(draw, 155, y + 5, 186, y + 2, grip_light, 1)

    line(draw, 90, 101, 244, 101, steel_light, 3)
    line(draw, 91, 132, 181, 132, (120, 125, 122, 160), 2)
    rounded(draw, 205, 121, 244, 141, 6, outline)
    rounded(draw, 214, 126, 235, 136, 3, (35, 36, 35, 255))
    line(draw, 156, 164, 186, 218, (22, 17, 15, 135), 2)


def draw_hunting_rifle(draw: ImageDraw.ImageDraw) -> None:
    outline = (18, 19, 17, 255)
    barrel = (43, 47, 46, 255)
    barrel_light = (103, 110, 104, 220)
    wood = (116, 72, 38, 255)
    wood_dark = (67, 42, 27, 255)
    wood_light = (174, 102, 50, 185)
    brass = (139, 112, 54, 255)

    shadow(draw, [(52, 158), (702, 147), (731, 165), (78, 180)], blur=False)

    polygon(draw, [(48, 137), (142, 106), (230, 111), (236, 152), (84, 174)], outline)
    polygon(draw, [(63, 139), (148, 116), (218, 120), (223, 144), (88, 162)], wood)
    polygon(draw, [(52, 143), (101, 129), (92, 165), (61, 170)], wood_dark)
    for x in range(104, 218, 25):
        line(draw, x, 127, x + 35, 121, wood_light, 2)
        line(draw, x - 4, 151, x + 32, 143, (66, 39, 24, 145), 2)

    rounded(draw, 216, 111, 332, 150, 8, outline)
    rounded(draw, 228, 118, 323, 141, 5, (48, 52, 50, 255))
    rect(draw, 325, 123, 392, 144, outline)
    rect(draw, 335, 127, 389, 139, (63, 66, 63, 255))

    polygon(draw, [(372, 128), (531, 112), (552, 136), (403, 153)], outline)
    polygon(draw, [(386, 130), (520, 119), (536, 133), (402, 144)], wood)
    for x in range(404, 514, 30):
        line(draw, x, 133, x + 42, 127, wood_light, 2)
        line(draw, x - 8, 143, x + 34, 137, (68, 41, 24, 145), 2)

    rect(draw, 530, 118, 718, 130, outline)
    rect(draw, 540, 122, 707, 126, barrel)
    rect(draw, 710, 115, 739, 133, outline)
    rect(draw, 716, 120, 735, 128, barrel)
    line(draw, 546, 121, 704, 121, barrel_light, 2)

    rounded(draw, 290, 74, 492, 99, 9, outline)
    rounded(draw, 306, 80, 476, 92, 5, (29, 33, 32, 255))
    rect(draw, 334, 98, 356, 119, outline)
    rect(draw, 427, 97, 448, 119, outline)
    line(draw, 314, 84, 468, 84, (109, 116, 110, 120), 2)

    rounded(draw, 258, 145, 287, 197, 7, outline)
    rounded(draw, 266, 151, 280, 185, 4, wood_dark)
    arc(draw, 293, 142, 356, 200, 180, 356, outline, 5)
    line(draw, 330, 150, 344, 178, (31, 32, 30, 230), 4)
    ellipse(draw, 356, 129, 374, 143, outline)
    ellipse(draw, 361, 133, 369, 139, brass)


def draw_shotgun(draw: ImageDraw.ImageDraw) -> None:
    outline = (18, 19, 17, 255)
    metal = (48, 52, 50, 255)
    metal_light = (113, 120, 113, 210)
    wood = (126, 78, 39, 255)
    wood_dark = (72, 43, 24, 255)
    wood_light = (183, 105, 51, 170)

    shadow(draw, [(47, 164), (701, 153), (732, 170), (74, 187)], blur=False)

    polygon(draw, [(45, 142), (138, 111), (224, 116), (228, 151), (83, 178)], outline)
    polygon(draw, [(61, 144), (144, 121), (212, 124), (215, 144), (89, 164)], wood)
    polygon(draw, [(47, 149), (94, 136), (86, 170), (59, 174)], wood_dark)
    for x in range(104, 206, 24):
        line(draw, x, 132, x + 34, 126, wood_light, 2)
        line(draw, x - 3, 154, x + 34, 146, (68, 38, 23, 145), 2)

    rounded(draw, 215, 116, 354, 153, 8, outline)
    rounded(draw, 226, 123, 344, 144, 5, metal)
    rect(draw, 340, 123, 427, 144, outline)
    rect(draw, 350, 127, 418, 139, (61, 65, 62, 255))

    rounded(draw, 392, 109, 530, 151, 8, outline)
    rounded(draw, 404, 118, 518, 142, 6, wood)
    for x in range(418, 510, 18):
        line(draw, x, 121, x + 12, 141, (80, 45, 24, 140), 2)
        line(draw, x + 7, 120, x + 19, 140, wood_light, 1)

    rect(draw, 514, 113, 720, 126, outline)
    rect(draw, 524, 117, 711, 122, metal)
    rect(draw, 510, 141, 710, 152, outline)
    rect(draw, 520, 144, 699, 148, (39, 42, 41, 255))
    rect(draw, 708, 110, 738, 129, outline)
    rect(draw, 715, 116, 734, 125, metal)
    line(draw, 526, 116, 707, 116, metal_light, 2)
    line(draw, 523, 144, 695, 144, (92, 98, 93, 185), 1)

    rounded(draw, 258, 149, 292, 207, 7, outline)
    rounded(draw, 267, 156, 284, 193, 4, wood_dark)
    arc(draw, 294, 144, 358, 204, 180, 356, outline, 5)
    line(draw, 332, 151, 347, 181, (30, 31, 30, 230), 4)
    rect(draw, 352, 139, 386, 148, outline)
    rect(draw, 360, 142, 381, 145, (112, 117, 111, 210))


def trim_alpha(image: Image.Image, padding: int) -> Image.Image:
    alpha = image.getchannel("A")
    bbox = alpha.getbbox()
    if bbox is None:
        return image

    left = max(0, bbox[0] - padding)
    top = max(0, bbox[1] - padding)
    right = min(image.width, bbox[2] + padding)
    bottom = min(image.height, bbox[3] + padding)
    return image.crop((left, top, right, bottom))


def write_import_file(image_path: pathlib.Path) -> None:
    source = f"res://data/sprites/items/{image_path.name}"
    source_hash = hashlib.md5(source.encode("utf-8")).hexdigest()
    uid = "uid://" + hashlib.sha1(source.encode("utf-8")).hexdigest()[:13]
    import_text = f"""[remap]

importer="texture"
type="CompressedTexture2D"
uid="{uid}"
path="res://.godot/imported/{image_path.name}-{source_hash}.ctex"
metadata={{
"vram_texture": false
}}

[deps]

source_file="{source}"
dest_files=["res://.godot/imported/{image_path.name}-{source_hash}.ctex"]

[params]

compress/mode=0
compress/high_quality=false
compress/lossy_quality=0.7
compress/uastc_level=0
compress/rdo_quality_loss=0.0
compress/hdr_compression=1
compress/normal_map=0
compress/channel_pack=0
mipmaps/generate=false
mipmaps/limit=-1
roughness/mode=0
roughness/src_normal=""
process/channel_remap/red=0
process/channel_remap/green=1
process/channel_remap/blue=2
process/channel_remap/alpha=3
process/fix_alpha_border=true
process/premult_alpha=false
process/normal_map_invert_y=false
process/hdr_as_srgb=false
process/hdr_clamp_exposure=false
process/size_limit=0
detect_3d/compress_to=1
"""
    image_path.with_suffix(image_path.suffix + ".import").write_text(import_text, encoding="utf-8")


def shadow(draw: ImageDraw.ImageDraw, points: list[tuple[int, int]], blur: bool) -> None:
    del blur
    polygon(draw, points, (0, 0, 0, 52))


def rect(draw: ImageDraw.ImageDraw, x1: int, y1: int, x2: int, y2: int, fill: tuple[int, int, int, int]) -> None:
    draw.rectangle(scale_rect(x1, y1, x2, y2), fill=fill)


def rounded(
    draw: ImageDraw.ImageDraw,
    x1: int,
    y1: int,
    x2: int,
    y2: int,
    radius: int,
    fill: tuple[int, int, int, int],
) -> None:
    draw.rounded_rectangle(scale_rect(x1, y1, x2, y2), radius=radius * SCALE, fill=fill)


def polygon(draw: ImageDraw.ImageDraw, points: list[tuple[int, int]], fill: tuple[int, int, int, int]) -> None:
    draw.polygon([(x * SCALE, y * SCALE) for x, y in points], fill=fill)


def line(
    draw: ImageDraw.ImageDraw,
    x1: int,
    y1: int,
    x2: int,
    y2: int,
    fill: tuple[int, int, int, int],
    width: int,
) -> None:
    draw.line([(x1 * SCALE, y1 * SCALE), (x2 * SCALE, y2 * SCALE)], fill=fill, width=width * SCALE)


def arc(
    draw: ImageDraw.ImageDraw,
    x1: int,
    y1: int,
    x2: int,
    y2: int,
    start: int,
    end: int,
    fill: tuple[int, int, int, int],
    width: int,
) -> None:
    draw.arc(scale_rect(x1, y1, x2, y2), start=start, end=end, fill=fill, width=width * SCALE)


def ellipse(draw: ImageDraw.ImageDraw, x1: int, y1: int, x2: int, y2: int, fill: tuple[int, int, int, int]) -> None:
    draw.ellipse(scale_rect(x1, y1, x2, y2), fill=fill)


def scale_rect(x1: int, y1: int, x2: int, y2: int) -> tuple[int, int, int, int]:
    return (x1 * SCALE, y1 * SCALE, x2 * SCALE, y2 * SCALE)


if __name__ == "__main__":
    main()
