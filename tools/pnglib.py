import struct, zlib


def read_png(path):
    """(width, height, rows) where rows[y] is a bytearray of RGBA."""
    data = open(path, 'rb').read()
    pos, idat, pal, trns = 8, [], [], b''
    while pos < len(data):
        n, = struct.unpack('>I', data[pos:pos + 4])
        kind, body = data[pos + 4:pos + 8], data[pos + 8:pos + 8 + n]
        if kind == b'IHDR':
            w, h, depth, ctype, _, _, interlace = struct.unpack('>IIBBBBB', body)
            assert depth == 8 and interlace == 0, (depth, interlace)
        elif kind == b'PLTE':
            pal = [body[i:i + 3] for i in range(0, len(body), 3)]
        elif kind == b'tRNS':
            trns = body
        elif kind == b'IDAT':
            idat.append(body)
        pos += 12 + n
    bpp = {6: 4, 2: 3, 3: 1, 4: 2, 0: 1}[ctype]
    raw = zlib.decompress(b''.join(idat))
    stride = w * bpp
    rows, prev, i = [], bytearray(stride), 0
    for _ in range(h):
        f = raw[i]
        line = bytearray(raw[i + 1:i + 1 + stride])
        i += 1 + stride
        if f == 1:
            for x in range(bpp, stride):
                line[x] = (line[x] + line[x - bpp]) & 255
        elif f == 2:
            for x in range(stride):
                line[x] = (line[x] + prev[x]) & 255
        elif f == 3:
            for x in range(stride):
                a = line[x - bpp] if x >= bpp else 0
                line[x] = (line[x] + ((a + prev[x]) >> 1)) & 255
        elif f == 4:
            for x in range(stride):
                a = line[x - bpp] if x >= bpp else 0
                b = prev[x]
                c = prev[x - bpp] if x >= bpp else 0
                p = a + b - c
                pa, pb, pc = abs(p - a), abs(p - b), abs(p - c)
                line[x] = (line[x] + (a if pa <= pb and pa <= pc else b if pb <= pc else c)) & 255
        rows.append(line)
        prev = line
    if ctype != 6:
        out = []
        for line in rows:
            rgba = bytearray(w * 4)
            for x in range(w):
                if ctype == 3:
                    k = line[x]
                    rgba[x * 4:x * 4 + 3] = pal[k]
                    rgba[x * 4 + 3] = trns[k] if k < len(trns) else 255
                elif ctype == 2:
                    rgba[x * 4:x * 4 + 3] = line[x * 3:x * 3 + 3]
                    rgba[x * 4 + 3] = 255
                elif ctype == 4:
                    g = line[x * 2]
                    rgba[x * 4:x * 4 + 4] = bytes((g, g, g, line[x * 2 + 1]))
                else:
                    g = line[x]
                    rgba[x * 4:x * 4 + 4] = bytes((g, g, g, 255))
            out.append(rgba)
        rows = out
    return w, h, rows


def write_png(path, w, h, rows):
    """rows: list of bytearray RGBA, written unfiltered."""
    raw = b''.join(b'\x00' + bytes(r) for r in rows)

    def chunk(kind, body):
        return struct.pack('>I', len(body)) + kind + body + struct.pack('>I', zlib.crc32(kind + body) & 0xffffffff)

    png = b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 6, 0, 0, 0))
    png += chunk(b'IDAT', zlib.compress(raw, 9)) + chunk(b'IEND', b'')
    open(path, 'wb').write(png)


def alpha_mask(w, rows, threshold=16):
    """Per-row int bitsets, bit x set where alpha > threshold."""
    masks = []
    for line in rows:
        alpha = line[3::4]
        m = 0
        for x, a in enumerate(alpha):
            if a > threshold:
                m |= 1 << x
        masks.append(m)
    return masks
