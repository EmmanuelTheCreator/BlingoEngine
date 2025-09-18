# XMED Byte Offsets

This page summarises the known byte offsets for Director XMED text casts.
File-by-file comparisons can be found in [XMED_FileComparisons.md](XMED_FileComparisons.md).

## Style and Flag Bytes

The byte at `0x001C` stores style flags. Bits can be combined:

| Bit | Value | Description |
|----:|-------|-------------|
|0|0x01|Bold|
|1|0x02|Italic|
|2|0x04|Underline|
|3|0x08|Strikeout|
|4|0x10|Subscript|
|5|0x20|Superscript|
|6|0x40|Tabbed field|
|7|0x80|Editable field|

The following byte at `0x001D` encodes alignment and additional flags. Known values:

| Value | Meaning |
|------:|---------|
|0x00|Centered (default)|
|0x1A|Left alignment|
|0x15|Right alignment|
|0x19|Wrap disabled|
|0x10|Tab character present|

## Width, Spacing and Margins

Offset `0x0018` holds the field width (twips). For example the file `Text_Wider_Width4.cst` stores `7A 17 00 00`, roughly four inches, instead of the `2C 00 00 00` value in `Text_Hallo.cst`.

Line spacing is stored at `0x003C`; font size at `0x0040`. Margins and indent bytes appear near `0x04DA`.

## Width, Spacing and Margins

- **0x0018** → Field width (twips).
- **0x003C** → Line spacing.
- **0x0040** → Font size (int32 LE).
- **0x004C** → Text length (int32 LE).  
- **0x04DA** → Left margin.
- **0x04DE** → Right margin.
- **0x04E2** → First indent.

---

## Color and Font Data

- **0x0622** → Color table (start).
- **0x0983** → Font name string (ASCII, NUL terminated).
- **0x0CAE** → Spacing before paragraph.
- **0x0EF7** → Member name string.
- **0x1354** → Second color table (multi-style).
- **0x1970** → Spacing after paragraph.

### Style Blocks

- Multi-style casts: descriptor blocks at ~0x16A8+.  
  Each contains style/flag bytes, ASCII style ID, one-byte color index, and font name.  
- Single-style casts: same structure with only one block.

Mapping tables reference descriptors via 20‑digit ASCII entries.  
The last ID field selects which descriptor overrides the parent style values.

To chck: a **one‑byte color index** and the font name.  The color index selects one of the values from the table near the start of the file. Single-style files use the same layout with just one block.


