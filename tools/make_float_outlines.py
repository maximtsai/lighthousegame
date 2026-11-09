"""Make the yellow hover outlines for the Day 4 floating bodies and mermaid at the pier.

For each "... float dock.png" sheet below, traces a ~2px outline around the outside of each 660x400
frame's shape (small gaps bridged, holes filled) and writes "... float dock outline.png" next to it,
with the same import settings and frame slicing. PierScene points at those frames by ID, so a rerun
keeps each outline's existing guid.

Run from anywhere:  python tools/make_float_outlines.py
"""
import os, sys, time, uuid
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pnglib import read_png, write_png, alpha_mask

FOLDER = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "Assets", "Art", "Backgrounds", "dockdark")
CELL_W, CELL_H = 660, 400
YELLOW = bytes((255, 208, 0, 255))  # sampled from mermaid side view yellow outline.png


def dilate_rows(rows, width, eight):
    full = (1 << width) - 1
    out = []
    for y, m in enumerate(rows):
        up = rows[y - 1] if y > 0 else 0
        down = rows[y + 1] if y + 1 < len(rows) else 0
        d = m | ((m << 1) & full) | (m >> 1) | up | down
        if eight:
            for n in (up, down):
                d |= ((n << 1) & full) | (n >> 1)
        out.append(d)
    return out


def dilate(rows, width, radius):
    for i in range(radius):
        rows = dilate_rows(rows, width, i % 2 == 0)  # alternate square and plus steps for a rounder grow
    return rows


def erode(rows, width, radius):
    full = (1 << width) - 1
    inv = dilate([~r & full for r in rows], width, radius)
    return [~r & full for r in inv]


def fill_holes(rows, width):
    """Treat transparent areas not connected to the cell's edge as part of the shape."""
    full = (1 << width) - 1
    free = [~r & full for r in rows]
    edge = (1 | (1 << (width - 1)))
    reach = [free[y] if y in (0, len(rows) - 1) else free[y] & edge for y in range(len(rows))]
    while True:
        grown = [g & free[y] for y, g in enumerate(dilate_rows(reach, width, False))]
        if grown == reach:
            break
        reach = grown
    return [~reach[y] & full for y in range(len(rows))]


def silhouette(cell, width, bridge=5):
    """The outer shape: small gaps (water lines, between strands of hair) bridged, holes filled."""
    closed = erode(dilate(cell, width, bridge), width, bridge)
    closed = [c | m for c, m in zip(closed, cell)]
    return fill_holes(closed, width)


def outline_sheet(name):
    src = os.path.join(FOLDER, name + ".png")
    t0 = time.time()
    w, h, rows = read_png(src)
    masks = alpha_mask(w, rows)
    out = [bytearray(w * 4) for _ in range(h)]
    cell_full = (1 << CELL_W) - 1
    count = 0
    for cy in range(h // CELL_H):
        for cx in range(w // CELL_W):
            shift = cx * CELL_W
            cell = [(masks[cy * CELL_H + y] >> shift) & cell_full for y in range(CELL_H)]
            if not any(cell):
                continue
            shape = silhouette(cell, CELL_W)
            grown = dilate(shape, CELL_W, 2)  # about 2 px, rounded
            for y in range(CELL_H):
                ring = grown[y] & ~shape[y]
                row = out[cy * CELL_H + y]
                while ring:
                    low = ring & -ring
                    x = shift + low.bit_length() - 1
                    row[x * 4:x * 4 + 4] = YELLOW
                    ring ^= low
                    count += 1
    dst = os.path.join(FOLDER, name + " outline.png")
    write_png(dst, w, h, out)

    # Same import settings and frame slicing as the sheet it traces, so frame N lines up with frame N
    meta = open(src + ".meta", encoding="utf-8", newline="").read()  # keep its line endings
    lines = meta.split("\n")
    guid = uuid.uuid4().hex
    if os.path.exists(dst + ".meta"):  # keep the guid PierScene already points at
        for l in open(dst + ".meta", encoding="utf-8"):
            if l.startswith("guid: "):
                guid = l.split()[1]
    lines = [f"guid: {guid}" + ("\r" if l.endswith("\r") else "") if l.startswith("guid: ") else l for l in lines]
    meta = "\n".join(lines).replace(name + "_", name + " outline_")
    open(dst + ".meta", "w", encoding="utf-8", newline="").write(meta)
    print(f"{name}: {count} outline pixels, {time.time() - t0:.0f}s")


if __name__ == "__main__":
    for n in ["corpse 1 float dock", "corpse 2 float dock", "corpse 3 float dock", "corpse 4 float dock",
              "mermaid float dock"]:
        outline_sheet(n)
