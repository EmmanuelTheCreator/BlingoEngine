# XMED Header and Byte Offsets

This document consolidates the latest findings on Macromedia Director `.xmed` styled-text members.  It focuses on how to locate
text, font, and style data without scanning the entire payload, relying instead on the header directory that anchors each block.

---

## 1. Header Directory

Every XMED file begins with an ASCII directory made up of comma-separated rows:

```
<TYPE:4>,<OFFSET:8>,<COUNT:8>
```

- **TYPE** – Four ASCII digits that identify the block kind.  Known value: `0008` for the font table.
- **OFFSET** – Eight ASCII hexadecimal digits indicating where the block starts in the file.
- **COUNT** – Eight ASCII hexadecimal digits specifying how many records are allocated for that block.

Rows are terminated by the first `0x00` byte.  There is no explicit header-length field; the reader must parse rows until it
encounters the NUL terminator.

### Example

For `Text_Multi_Line_Multi_Style_13.xmed.bin` one of the header rows is:

```
0008,000005B0,00000010
```

This means the font table (`TYPE = 0008`) starts at offset `0x05B0` (decimal 1456) and contains 16 records.  Jumping to `0x05B0`
reveals the first `"40,"` font record.

The header also embeds the text length near its end.  The sequence `00 31 32 41 2C` corresponds to the ASCII string `"12A,"`,
which decodes to `0x12A = 298`.  The actual text block starts immediately after this field, so the characters occupy bytes
`[0x0F5 .. 0x0F5 + 298)` in this sample.

---

## 2. Block Types Anchored by the Header

### 2.1 Text Block

- Text content is plain ASCII (or MacRoman in legacy casts) stored as a contiguous slice.
- The header-provided length determines the number of bytes to read.  Consumers should not scan for terminating characters; the
  header length is authoritative.

### 2.2 Font Table (`TYPE = 0008`)

- Each record begins with the ASCII prefix `"40,"` (`0x34 0x30 0x2C`).
- A single byte indicates the length of the font name.
- The font name follows as UTF-8 bytes.
- Records are padded with NUL bytes until the next `"40,"` sequence.
- Iterate exactly `COUNT` times; empty slots report a zero length.

The parser should capture the absolute offset of each record along with its decoded name to aid cross-references from style blocks.

### 2.3 Run Map Table (Multi-style Text)

Multi-style members include a mapping table made of 20-digit ASCII sequences.  Each sequence is split into five 4-digit fields
interpreted as hexadecimal numbers:

| Field | Meaning | Notes |
|------:|---------|-------|
| F1 | Unknown | Often increments with the run index. |
| F2 | Unknown | Reserved / frequently zero. |
| F3 | Run length | Number of characters in the text block for this run. |
| F4 | Unknown | Appears to relate to offsets; currently unused. |
| F5 | Style ID | Links to a style descriptor block. |

Example records from `Text_Multi_Line_Multi_Style_13.xmed.bin`:

```
0004 0000 0029 0000 0008
0005 0000 001F 0000 0006
0006 0000 0273 0000 000B
0007 0000 0070 0000 0003
0008 0000 0366 0000 0005
```

The F3 values (hexadecimal) describe the character count for each line/run, while F5 selects the descriptor that defines the
formatting for that span.

### 2.4 Style Descriptor Blocks

Descriptor blocks reside at offsets referenced by other directory rows (exact TYPE codes are still under investigation).  Each
block associates a style ID with typography and layout attributes.  Observed fields include:

- **StyleId** – ASCII digits matching the IDs referenced from the run map (F5).
- **FontName** – Back-reference to one of the `"40,"` font records.
- **FontPx** – Pixel height of the typeface.  In inspected samples this matches the value shown in Director (e.g., Arial 12 px).
- **Flags** – Style bits (bold, italic, underline, etc.).
- **Align** – Alignment byte decoded from the style flags table below.
- **LetterSpacing / LineSpacing / ColorIndex** – Optional integers when the descriptor overrides those properties.

Descriptors typically live around offsets `0x16A0` and beyond in the provided assets, but consumers should always consult the
header directory rather than assuming fixed addresses.

### 2.5 Color Tables

Colour tables appear as ASCII hexadecimal values (e.g., `FFFF...`) and are also located via header pointers.  Style descriptors
reference entries through their color index field.

---

## 3. Mapping Runs to Text

The run map ties slices of the text block to style descriptors.  Each decoded entry reports how many characters to consume (`F3`)
and which descriptor (`F5`) supplies styling overrides.  In `Text_Multi_Line_Multi_Style_13.xmed.bin` the five entries reference
the descriptors for the red Arial line, the yellow Tahoma line, the green Terminal line, the orange Tahoma line, and the final
centred red Arial line shown in the sample asset.

Accumulating the F3 lengths provides absolute character offsets into the text block, so each run can be isolated without scanning
for delimiters.

---

## 4. Style and Flag Bytes

Legacy research identified the bytes at `0x001C` and `0x001D` as global style and alignment flags.  These values still apply to the
base descriptor that multi-style runs inherit from.

| Bit | Value | Description |
|----:|------:|-------------|
| 0 | 0x01 | Bold |
| 1 | 0x02 | Italic |
| 2 | 0x04 | Underline |
| 3 | 0x08 | Strikeout |
| 4 | 0x10 | Subscript |
| 5 | 0x20 | Superscript |
| 6 | 0x40 | Tabbed field |
| 7 | 0x80 | Editable field |

Alignment and extra flags live in the following byte (`0x001D`):

| Value | Meaning |
|------:|---------|
| 0x00 | Centered (default) |
| 0x1A | Left alignment |
| 0x15 | Right alignment |
| 0x19 | Wrap disabled |
| 0x10 | Tab character present |

---

## 5. Width, Spacing, and Margins

Although descriptor tables now provide authoritative values, the legacy offsets remain useful when examining raw dumps:

- **0x0018** → Field width in twips (`7A 17 00 00` ≈ 4 inches in `Text_Wider_Width4.cst`).
- **0x003C** → Line spacing.
- **0x0040** → Font size (little-endian 32-bit integer).
- **0x004C** → Text length (little-endian 32-bit integer, superseded by header length for modern parsing).
- **0x04DA** → Left margin.
- **0x04DE** → Right margin.
- **0x04E2** → First-line indent.

Spacing-before and spacing-after bytes also appear near `0x0CAE` and `0x1970` in the provided captures, aligning with descriptor
fields observed via the header pointers.

---

## 6. Summary

- The header directory is the authoritative map of an XMED file.  Always use its offsets and counts to locate blocks.
- Text, font records, run maps, and style descriptors can be decoded without scanning the payload.
- Run maps consist of ASCII hexadecimal fields that describe run lengths and link to style descriptors.
- Style descriptors convey font, alignment, spacing, colour, and flag information; they in turn reference font table entries.
- Legacy absolute offsets are still valuable for sanity checks but should be treated as corroborating evidence rather than primary
  pointers.

