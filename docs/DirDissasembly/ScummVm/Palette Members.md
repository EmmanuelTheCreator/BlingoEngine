# Palette Cast Members

This reference explains how ScummVM recognises palette cast entries and where it sources the actual color tables for Director movies. The version notes below list every byte the loader inspects while wiring a palette to the runtime palette manager.

## Step 1 – Director 2–3 palette records in `VWCR`

Early movies store palette metadata inside the `VWCR` directory. Palette entries only include the common header bytes, so ScummVM forwards an empty payload to the palette constructor and immediately loads the external CLUT resource.[engines/director/cast.cpp (approx. lines 1374-1443)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<entry size>` | 1 byte | Total entry size including the type byte; Director emits the value `0x02` for palettes, leaving no room for inline payload.[engines/director/cast.cpp (approx. lines 1379-1396)] |
| `<cast type>` | 1 byte | Always `kCastPalette` (`0x08`) for palette entries; it triggers the palette cast constructor.[engines/director/cast.cpp (approx. lines 1399-1434)] |
| `<flags1>` | 1 byte (optional) | Rare in practice for palettes; when present the loader preserves it even though the current implementation ignores the value.[engines/director/cast.cpp (approx. lines 1388-1394)] |

## Step 2 – Director 4–5 `CASt` payloads

With `CASt` resources the palette cast member still ships without inline data. After the header bytes are consumed, the loader merely hexdumps the stream (for debugging) and defers to the linked `CLUT` resource.[engines/director/cast.cpp (approx. lines 1494-1513)][engines/director/castmember/palette.cpp (approx. lines 31-36)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<cast data size>` | 2 bytes | Declares the total data-section length, which Director sets to `0x0001` so the only consumed byte is the cast type already read by the dispatcher.[engines/director/cast.cpp (approx. lines 1494-1501)][engines/director/castmember/palette.cpp (approx. lines 138-147)] |
| `<cast payload>` | 0 bytes | No palette fields follow the header; the constructor simply records the cast type and waits for `load()` to fetch the CLUT.[engines/director/castmember/palette.cpp (approx. lines 31-36)] |

## Step 3 – Director 5–6 `CASt` payloads

Director 5 increases the header size but still omits inline palette data. ScummVM therefore skips the cast-data block entirely and jumps straight to the CLUT lookup.[engines/director/cast.cpp (approx. lines 1508-1513)][engines/director/castmember/palette.cpp (approx. lines 138-149)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<cast data size>` | 4 bytes | Stored as zero for Director 5 movies, signalling that the palette carries no inline payload.[engines/director/cast.cpp (approx. lines 1508-1513)][engines/director/castmember/palette.cpp (approx. lines 140-147)] |
| `<cast payload>` | 0 bytes | All palette state comes from the CLUT resource referenced via the cast’s child list.[engines/director/castmember/palette.cpp (approx. lines 96-129)] |

## Step 4 – CLUT resource layout

Actual palette colours live in the `CLUT` resource. ScummVM reads 16-bit Macintosh colour components, skipping the low byte that Director left unused, and registers the palette with the global director once the table is decoded.[engines/director/cast.cpp (approx. lines 1341-1369)][engines/director/castmember/palette.cpp (approx. lines 116-129)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<CLUT red>` | 2 bytes | Big-endian colour component; ScummVM keeps only the high byte as the 8-bit red value.[engines/director/cast.cpp (approx. lines 1359-1361)] |
| `<CLUT green>` | 2 bytes | Big-endian green component reduced to the high byte.[engines/director/cast.cpp (approx. lines 1363-1364)] |
| `<CLUT blue>` | 2 bytes | Big-endian blue component reduced to the high byte.[engines/director/cast.cpp (approx. lines 1366-1367)] |

