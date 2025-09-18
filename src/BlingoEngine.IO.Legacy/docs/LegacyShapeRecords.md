# Legacy Shape Records

[← Back to documentation home](README.md)

Legacy Director movies store shape cast members as compact QuickDraw records nested inside
version-specific `CASt` or `VWCR` containers. This note consolidates the byte layouts recovered
from early projector builds so the engine can slice out the 17-byte shape payload without relying
on external tooling. The information below merges historical research with fresh captures from
classic Macintosh and Windows movies to ensure compatibility across every Director generation.

## Cast wrappers by Director version

Director uses three different wrappers around the shape payload before the 17 data bytes become
visible. The tables below summarise the header bytes so you can pinpoint the start of the QuickDraw
record when parsing a `CASt` resource.

### Director 2–3 `VWCR` entries (version < 0x400)

| Hex Bytes | Length | Description |
| --- | --- | --- |
| `<entry size>` | 1 byte | Number of bytes that belong to the entry, including the cast type and optional flag byte. |
| `<cast type>` | 1 byte | Legacy cast enumeration; the value `8` identifies shape members. |
| `<flags1>` | 1 byte (optional) | Present when the entry size exceeds one byte. Director stored miscellaneous per-cast flags here, so loaders preserve the value even though shapes ignore it. |
| `<cast payload>` | `entry size` − consumed bytes | Raw bytes passed to the shape reader. They end immediately before the next `VWCR` entry. |

### Director 4 `CASt` headers (0x400 ≤ version < 0x500)

| Hex Bytes | Length | Description |
| --- | --- | --- |
| `<cast data size>` | 2 bytes | Big-endian size of the cast-data section, including the type byte and optional `flags1`. |
| `<cast info size>` | 4 bytes | Big-endian size of the metadata block that trails the cast data. |
| `<cast type>` | 1 byte | Legacy cast enumeration (`8` for shapes). |
| `<flags1>` | 1 byte (optional) | Preserved when the cast-data size exceeds one byte. Text-based members consume this flag; shapes simply carry it forward. |
| `<cast data payload>` | `cast data size` − consumed bytes | Version-specific data forwarded to the shape reader. |
| `<cast info payload>` | `cast info size` bytes | Optional metadata (names, timestamps, author data) loaded separately by the cast table. |

### Director 5–10 `CASt` headers (0x500 ≤ version < 0x1100)

| Hex Bytes | Length | Description |
| --- | --- | --- |
| `<cast type>` | 4 bytes | Four-character tag written in big endian. The value `0x00000008` maps to shape members. |
| `<cast info size>` | 4 bytes | Big-endian byte count for the info block that follows immediately after the header. |
| `<cast data size>` | 4 bytes | Big-endian byte count for the cast-data section stored after the info block. |
| `<cast info payload>` | `cast info size` bytes | Optional metadata strings consumed by the cast loader. |
| `<cast data payload>` | `cast data size` bytes | Bytes forwarded to the shape reader. |

Director MX (11+) retained the same high-level header but changed the body semantics. Contemporary
movies frequently stub the shape payload or leave it empty. The reader therefore treats Director 11
records as placeholders until their exact layout is confirmed.

> ⚠️ **Assumption:** This placeholder handling is based on projector dumps captured so far. The
> documentation and loader will be revised as soon as we recover a Director 11+ sample that stores a
> fully populated payload.

## Shape payload layout

Once the wrapper bytes are removed, every classic projector version emits the same 17-byte QuickDraw
record. The only behavioural change across releases is how the colour components should be
interpreted.

### Director 2–3 – Signed QuickDraw colours

| Hex Bytes | Length | Description |
| --- | --- | --- |
| `<shape type>` | 2 bytes | QuickDraw shape enumeration. Director writes the value in big endian. |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit coordinates describing the initial bounding rectangle. |
| `<pattern id>` | 2 bytes | Fill pattern or hatch identifier applied to the shape. |
| `<foreground colour>` | 1 byte | Signed QuickDraw colour component. Add 128 and mask to `0xFF` before mapping it through the palette. |
| `<background colour>` | 1 byte | Signed background colour processed with the same normalisation. |
| `<fill type>` | 1 byte | Fill and ink flags. The lower six bits map directly to Director's ink modes. |
| `<line thickness>` | 1 byte | Pen size in pixels. |
| `<line direction>` | 1 byte | QuickDraw line-direction value used when drawing patterned strokes. |

### Director 4–10 – Unsigned QuickDraw colours

| Hex Bytes | Length | Description |
| --- | --- | --- |
| `<shape type>` | 2 bytes | Shape enumeration copied verbatim from the cast resource. |
| `<rect.top>`..`<rect.right>` | 8 bytes | Signed QuickDraw rectangle that defines the initial bounds of the shape sprite. |
| `<pattern id>` | 2 bytes | Pattern identifier or resource index applied to the fill brush. |
| `<foreground colour>` | 1 byte | Unsigned 0–255 colour byte passed straight to the palette transform. |
| `<background colour>` | 1 byte | Unsigned QuickDraw background value. |
| `<fill type>` | 1 byte | Fill and ink flags; the low six bits pick the ink mode. |
| `<line thickness>` | 1 byte | Pen width for the outline. |
| `<line direction>` | 1 byte | Line-direction byte preserved for patterned strokes. |

### Director 11 and newer – Placeholder data

Director MX 2004 and subsequent releases no longer expose a documented 17-byte payload. Projector
builds often substitute default values or omit the cast data entirely. When a movie advertises a
Director 11+ version marker, the loader keeps whatever bytes were supplied (even an empty buffer) and
classifies the format as a placeholder so the caller can opt into experimental processing.

## Colour interpretation

Classic Mac projectors store signed QuickDraw colours even though palette indices eventually operate
in an unsigned 0–255 range. Director 2 and 3 therefore require normalisation using the expression
`(value + 128) & 0xFF`. Starting with Director 4, both foreground and background components are stored
as unsigned bytes and may be forwarded directly to the palette transform.

## Reader implementation notes

- `BlLegacyShapeReader` inflates classic and Afterburner-compressed `CASt` entries, extracts the
  cast-data segment using the wrappers above, and then scans for a 17-byte window that contains a
  plausible QuickDraw rectangle. This approach tolerates optional `flags1` bytes that precede the
  payload in early Director versions.
- The reader honours the movie's endianness markers when validating rectangle coordinates so that
  mixed-platform Director movies continue to parse correctly.
- Every record is preserved exactly as it appears on disk. The `BlLegacyShape` data class exposes
  the 17 raw bytes along with the best-effort format classification so higher-level systems can
  reconstruct ink modes, patterns, and colour schemes as needed.

These notes ensure the implementation keeps byte-for-byte parity with historical Director projectors
while remaining ready for future discoveries about Director MX-era resources.
