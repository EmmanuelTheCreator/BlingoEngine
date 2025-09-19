# Legacy Text and Field Members

[_Back to documentation overview_](README.md)

These notes consolidate the byte layouts used by Director text and field cast members prior to the
latest MX projectors. The information mirrors the behaviour implemented in
`BlLegacyTextReader` and `BlLegacyFieldReader`: both readers surface raw `STXT` and `XMED`
payloads together with their detected container formats, leaving the higher layers free to interpret
fonts, style runs, and field flags. Tables below record every header byte consumed by historic
projector versions so that older resources remain compatible and the modern MX `XMED` layout can be
decoded without cross-referencing additional files.

## Resource Overview

- Director 2 and 3 export plain `STXT` streams. The cast-member header carries alignment, border,
  colour, and rectangle metadata while the companion `STXT` chunk holds the text itself.
- Director 4 through Director 10 extend the header with scrolling and maximum-height fields, but the
  text bytes continue to live in `STXT` chunks unless the movie opts into the styled `XMED` format.
- Styled text (`XMED`) keeps the same cast-member header and stores per-character formatting in a
  sidecar chunk. Modern offsets and parsing notes appear in the "Director 11+ Styled Text" section
  below.
- Bit 0 of the flag word/byte marks editable fields. Bit 1 enables auto-tab behaviour and bit 2
  disables word wrapping. Later records keep these semantics so fields can be identified after the
  raw bytes are loaded.

## Director 2 Layout (version &lt; 0x300)

| Hex Bytes | Length | Description |
| --- | --- | --- |
| `<border size>` | 1 byte | Frame thickness around the text box. |
| `<gutter size>` | 1 byte | Interior padding between the border and the text content. |
| `<box shadow>` | 1 byte | Drop-shadow distance recorded for legacy renderers. |
| `<text type>` | 1 byte | Cast subtype such as static text, scrolling text, or editable field. |
| `<text alignment>` | 2 bytes | Signed QuickDraw alignment word. |
| `<background red>` | 2 bytes | High-precision red component of the background colour. |
| `<background green>` | 2 bytes | Green component of the background colour. |
| `<background blue>` | 2 bytes | Blue component of the background colour. |
| `<pad2>` | 2 bytes | Reserved word, usually zero. Non-zero values indicate unrecognised data. |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit values describing the initial rectangle. |
| `<pad3>` | 2 bytes | Additional reserved word maintained for compatibility. |
| `<text shadow>` | 1 byte | Legacy drop-shadow depth applied to the glyphs themselves. |
| `<text flags>` | 1 byte | Low nibble stores field/text flags; high bits are left unused. |
| `<total text height>` | 2 bytes | Authored text height used when sizing widgets. |

Director 2 exports log a warning when the reserved words carry non-zero data or when the high five
bits of the flag byte are set, because only the low nibble is understood by older engines.

## Director 3 Layout (0x300 ≤ version &lt; 0x400)

| Hex Bytes | Length | Description |
| --- | --- | --- |
| `<border size>` | 1 byte | Same border thickness stored by Director 2. |
| `<gutter size>` | 1 byte | Interior padding value retained from earlier exports. |
| `<box shadow>` | 1 byte | Drop-shadow distance for the text box. |
| `<text type>` | 1 byte | Cast subtype selector identical to the Director 2 layout. |
| `<text alignment>` | 2 bytes | Signed alignment word read before colour data. |
| `<background red>` | 2 bytes | Red component of the background colour. |
| `<background green>` | 2 bytes | Green component of the background colour. |
| `<background blue>` | 2 bytes | Blue component of the background colour. |
| `<pad2>` | 2 bytes | Reserved word that remains in the stream. |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit values for the initial rectangle. |
| `<pad3>` | 2 bytes | Additional reserved word preserved for parity with Director 2. |
| `<text flags>` | 2 bytes | Flag word: bit 0 marks editable fields, bit 1 enables auto-tab, bit 2 disables wrapping. |
| `<total text height>` | 2 bytes | Authored height value retained by the editor. |

## Director 4–10 Layout (0x400 ≤ version &lt; 0x1100)

| Hex Bytes | Length | Description |
| --- | --- | --- |
| `<border size>` | 1 byte | Border thickness identical to earlier versions. |
| `<gutter size>` | 1 byte | Padding between the border and the text. |
| `<box shadow>` | 1 byte | Drop-shadow offset for the bounding box. |
| `<text type>` | 1 byte | Member subtype (static text, scrolling text, button text, etc.). |
| `<text alignment>` | 2 bytes | Signed alignment word (−1 for right alignment). |
| `<background red>` | 2 bytes | Red component of the background colour. |
| `<background green>` | 2 bytes | Green component of the background colour. |
| `<background blue>` | 2 bytes | Blue component of the background colour. |
| `<scroll offset>` | 2 bytes | Initial scroll position stored for scrolling text. |
| `<rect.top>`..`<rect.right>` | 8 bytes | Initial rectangle describing the cast-member bounds. |
| `<max height>` | 2 bytes | Maximum height used when wrapping multi-line text. |
| `<text shadow>` | 1 byte | Per-glyph drop-shadow depth reintroduced in Director 4. |
| `<text flags>` | 1 byte | Flag byte: bit 0 toggles editable fields, bit 1 enables auto-tab, bit 2 disables wrapping. |
| `<text height>` | 2 bytes | Preferred text height for layout calculations. |

## Button Text Extension

When these bytes are parsed for button cast members, an extra big-endian 16-bit value follows the
standard header. The value identifies the button type, counting from 1 in the authoring tool. The
runtime subtracts one so the type can be used as a zero-based enum.

## Locating STXT and XMED Resources

The header bytes above never include the author-facing text. That data lives in a companion resource:
older projectors reuse the cast-member ID to point at an `STXT` chunk, while Director 4 and later
keep an explicit child link. Styled movies replace or augment the `STXT` child with an `XMED`
resource that tracks font runs, colour tables, and letter spacing. Any payload read from `STXT` or
`XMED` is passed through unchanged by the legacy readers so experiments can continue even though the
latest MX-era format is still under investigation.

## Director 11+ Styled Text (`XMED`) Layout

Modern Director releases store per-character formatting inside an `XMED` stream. The chunk begins
with an ASCII directory that announces the offsets and capacities for each data block before listing
the text runs and style descriptors.

### Header Directory

Each directory token is twenty characters long and follows the pattern `<TYPE:4>,<OFFSET:8>,<COUNT:8>`.

- All three numbers are ASCII hex strings.
- Tokens continue until the first `0x00` terminator byte.
- Example: `0008,000005B0,00000010` describes type `0008` at offset `0x05B0` with space for sixteen
  records.

Known directory types observed so far:

- `0002` — Text block and length marker.
- `0004`–`0007` — Run-map rows stored as ASCII digits.
- `0008` — Font-name table built from `"40,"` prefixes.

### Text Block

The text length is recorded as ASCII digits immediately before the text bytes (e.g. `"12A,"` for
length `0x12A`). The actual text bytes follow directly after the comma and use Latin-1 encoding. The
length value matches the header field at offset `0x004C`.

### Font-Name Table (`TYPE 0008`)

Jump to the offset announced by the directory and iterate exactly `COUNT` records. Each entry starts
with the ASCII marker `"40,"`, followed by a one-byte length, the font name itself, and optional NUL
padding.

### Run and Style Map (`TYPE 0004`–`0007`)

Run-map rows consist of 20 ASCII digits divided into five groups of four characters:

```
F1 F2 F3 F4 F5
```

- `F1` — Run start index.
- `F2` and `F4` — Additional flags still under investigation.
- `F3` — Length of the run in characters.
- `F5` — Style descriptor identifier.

Rows should be sorted by `F1` to reconstruct the styled text in order.

### Style Descriptor Blocks

Style descriptors appear after the directory. Each block begins with a style byte and an alignment
byte, continues with four ASCII digits that represent the descriptor ID, and then references the
font-name table via the `"40,"` marker. The byte following `"40,"` acts as a colour index. Font names
are stored as Latin-1 strings terminated by a NUL byte. Descriptor IDs are unique within the stream;
the first instance of an ID should be retained when duplicates appear.

### Header Flag Bytes

Offsets `0x001C` and `0x001D` store style and alignment information that the reader applies to the
base run before descriptor overrides are processed.

#### Style Flags at `0x001C`

| Bit | Mask | Meaning |
| ---:|:----:|---------|
| 0 | `0x01` | Bold |
| 1 | `0x02` | Italic |
| 2 | `0x04` | Underline |
| 3 | `0x08` | Strikeout |
| 4 | `0x10` | Subscript |
| 5 | `0x20` | Superscript |
| 6 | `0x40` | Tabbed-field marker |
| 7 | `0x80` | Editable field flag (applies to the entire member) |

#### Alignment Flags at `0x001D`

| Bits | Mask | Meaning |
|:----:|:----:|---------|
| 0–1 | `0b00000011` | Alignment core (`00` Centre, `01` Right, `10` Left, `11` Justified) |
| 3 | `0x08` | Disable word wrapping |
| 4 | `0x10` | Tabs present |

### Metrics and Pointers

| Offset | Size | Purpose |
|------:|-----:|---------|
| `0x0018` | 4 bytes | Field width in twips |
| `0x003C` | 4 bytes | Line spacing |
| `0x0040` | 4 bytes | Default font size (clamped to `ushort`) |
| `0x004C` | 4 bytes | Declared text length |

Paragraph and margin information is recorded deeper inside the chunk:

| Offset | Description |
|------:|-------------|
| `0x04DA` | Left margin |
| `0x04DE` | Right margin |
| `0x04E2` | First-line indent |

Additional segments observed in the sample set include colour tables and authoring metadata:

| Offset | Description |
|------:|-------------|
| `0x0622` | Colour table header (`FFFF0000000600040001`) |
| `0x0983` | Base font name string |
| `0x0CAE` | Paragraph spacing before the text |
| `0x0EF7` | Cast-member name |
| `0x1354` | Secondary colour table for multi-style content |
| `0x1970` | Paragraph spacing after the text |

### Parsing Outline

1. Read the directory until the first `0x00` byte and store each `<type, offset, count>` triple.
2. Resolve the text block announced by `TYPE 0002`, parse the ASCII length, and slice the Latin-1
   text bytes.
3. Load the font table referenced by `TYPE 0008` to map colour indices to font names.
4. Iterate the run-map entries (`TYPE 0004`–`0007`) and convert each 20-digit line into numeric run
   metadata.
5. Scan for descriptor blocks, apply style and alignment bytes, and attach font overrides using the
   font table.
6. Merge the base style flags from offsets `0x001C`/`0x001D` with the descriptor overrides to build
   the final styled runs.

### Validation Tips

- Compare run lengths from the map with the actual text to catch truncated ranges.
- Ensure descriptor IDs reference known font records.
- Decode the alignment bits and check that justified text uses value `0x03`.

### Observed Values

- Multi-line samples produced run lengths of 38, 70, 72, 70, and 44 characters.
- Font-table entries often appear with the marker `"40,05,Arial"` and sixteen allocated slots.
- Alignment flag combinations recorded in the sample set include `0x00`, `0x10`, `0x15`, `0x1A`,
  `0x19`, and `0x03`.

