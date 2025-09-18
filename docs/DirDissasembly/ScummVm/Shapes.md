# Shape Cast Members

This note documents how ScummVM's Director engine decodes shape cast members across Director versions. Each section lists the bytes pulled from the resource stream together with their meaning so the QuickDraw-style geometry, colors, and ink are easy to audit.

## Step 1 – Director 2–3 shape records (version < 0x400)

`ShapeCastMember` parses early Director movies by reading a short header with the shape type, initial rect, pattern, and QuickDraw colors. Director 2/3 store color bytes as signed values, so the loader normalizes them back into 0–255 before mapping them through the engine palette.[engines/director/castmember/shape.cpp (approx. lines 31-52)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<shape type>` | 2 bytes | QuickDraw shape enumeration pulled with `readUint16BE()`.[engines/director/castmember/shape.cpp (approx. lines 43-51)] |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit coordinates for `_initialRect`, read via `Movie::readRect()`.[engines/director/castmember/shape.cpp (approx. lines 43-51)][engines/director/movie.cpp (approx. lines 276-284)] |
| `<pattern id>` | 2 bytes | Pattern resource or hatch index applied when filling the shape.[engines/director/castmember/shape.cpp (approx. lines 44-52)] |
| `<foreground color>` | 1 byte | Signed color component mapped through `transformColor((128 + byte) & 0xff)`.[engines/director/castmember/shape.cpp (approx. lines 46-50)] |
| `<background color>` | 1 byte | Signed background value normalized with the same helper.[engines/director/castmember/shape.cpp (approx. lines 46-50)] |
| `<fill type>` | 1 byte | QuickDraw fill/ink flags; the lower six bits become the `InkType`.[engines/director/castmember/shape.cpp (approx. lines 49-51)] |
| `<line thickness>` | 1 byte | Pen size in pixels stored in the record.[engines/director/castmember/shape.cpp (approx. lines 49-52)] |
| `<line direction>` | 1 byte | QuickDraw pattern orientation byte preserved for rendering.[engines/director/castmember/shape.cpp (approx. lines 50-52)] |

## Step 2 – Director 4–10 shape records (0x400 ≤ version < 0x1100)

Later Director builds keep the same byte layout but emit unsigned RGB components instead of the signed -128…127 values used by older Mac projectors. ScummVM therefore reads the same fields in the same order without the 128 offset before transforming them into palette indices.[engines/director/castmember/shape.cpp (approx. lines 53-62)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<shape type>` | 2 bytes | Shape enumeration value copied verbatim from the resource.[engines/director/castmember/shape.cpp (approx. lines 53-62)] |
| `<rect.top>`..`<rect.right>` | 8 bytes | Initial bounding rectangle for the shape sprite.[engines/director/castmember/shape.cpp (approx. lines 53-62)][engines/director/movie.cpp (approx. lines 276-284)] |
| `<pattern id>` | 2 bytes | QuickDraw fill pattern identifier carried forward from the resource.[engines/director/castmember/shape.cpp (approx. lines 55-62)] |
| `<foreground color>` | 1 byte | Unsigned 0–255 color byte passed to `transformColor()`.[engines/director/castmember/shape.cpp (approx. lines 57-60)] |
| `<background color>` | 1 byte | Unsigned QuickDraw background component read alongside the foreground value.[engines/director/castmember/shape.cpp (approx. lines 57-60)] |
| `<fill type>` | 1 byte | Fill style and ink flags; the low 6 bits map to Director's ink modes.[engines/director/castmember/shape.cpp (approx. lines 59-60)] |
| `<line thickness>` | 1 byte | Pen width stored for outline drawing.[engines/director/castmember/shape.cpp (approx. lines 59-62)] |
| `<line direction>` | 1 byte | Direction byte preserved for patterned strokes.[engines/director/castmember/shape.cpp (approx. lines 60-62)] |

## Step 3 – Director 11+ placeholders (version ≥ 0x1100)

Director MX 2004 and newer shapes are not implemented yet. When the version falls into this range, ScummVM logs a stub warning and substitutes default values so the engine can keep running even though the shape data is ignored.[engines/director/castmember/shape.cpp (approx. lines 63-71)]

Together these tables show that shapes never gained additional metadata inside the cast stream—only the encoding of the color bytes changed with Director 4. Ink mode selection, pattern IDs, and line settings are still delivered through the same compact record across classic Director versions.
