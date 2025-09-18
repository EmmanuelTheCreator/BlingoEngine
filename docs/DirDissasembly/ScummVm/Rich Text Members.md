# Rich Text Cast Members

Director 5 introduced rich text cast members that bundle styled Paige editor content alongside a rendered bitmap. ScummVM currently supports versions 5 through 10, initializing geometry, colors, and scroll state directly from the cast member stream before loading the attached RTE resources.[engines/director/castmember/richtext.cpp (approx. lines 34-138)]

## Step 1 – Director 5–10 rich text header (0x500 ≤ version < 0x1100)

When a movie targets Director 5 or later, the constructor consumes a fixed header that seeds both layout rectangles and Paige rendering hints. Older files log a stub because the legacy RTE format has not been decoded yet.[engines/director/castmember/richtext.cpp (approx. lines 37-70)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<initialRect.top>`..`<initialRect.right>` | 8 bytes | `Movie::readRect()` pulls four signed 16-bit coordinates for `_initialRect`, matching the authoring bounds.[engines/director/castmember/richtext.cpp (approx. lines 42-43)][engines/director/movie.cpp (approx. lines 234-239)] |
| `<boundingRect.top>`..`<boundingRect.right>` | 8 bytes | A second `Movie::readRect()` call records `_boundingRect`, used for sprite scaling.[engines/director/castmember/richtext.cpp (approx. lines 42-43)][engines/director/movie.cpp (approx. lines 234-239)] |
| `<antialias flag>` | 1 byte | Stores `_antialiasFlag`, toggling Paige font smoothing.[engines/director/castmember/richtext.cpp (approx. line 44)] |
| `<crop flags>` | 1 byte | Bitfield copied into `_cropFlags` to control clipping in Paige output.[engines/director/castmember/richtext.cpp (approx. line 45)] |
| `<scroll position>` | 2 bytes | Big-endian word assigned to `_scrollPos`, preserving the editor’s vertical scroll.[engines/director/castmember/richtext.cpp (approx. line 46)] |
| `<antialias font size>` | 2 bytes | Big-endian size hint used when anti-aliasing Paige glyphs.[engines/director/castmember/richtext.cpp (approx. line 47)] |
| `<display height>` | 2 bytes | Big-endian maximum display height stored in `_displayHeight`.[engines/director/castmember/richtext.cpp (approx. line 48)] |
| `<foreground pad>` | 1 byte | Reserved byte skipped before pulling RGB components for `_foreColor`.[engines/director/castmember/richtext.cpp (approx. lines 49-54)] |
| `<foreground red>` | 1 byte | High-precision red component collected for the Paige bitmap.[engines/director/castmember/richtext.cpp (approx. lines 51-54)] |
| `<foreground green>` | 1 byte | Green foreground component read from the stream.[engines/director/castmember/richtext.cpp (approx. lines 51-54)] |
| `<foreground blue>` | 1 byte | Blue component completing `_foreColor`.[engines/director/castmember/richtext.cpp (approx. lines 51-54)] |
| `<background red word>` | 2 bytes | Big-endian word whose high byte forms the red background component; Director stores colors in 16-bit quickdraw slots so ScummVM shifts them down.[engines/director/castmember/richtext.cpp (approx. lines 55-58)] |
| `<background green word>` | 2 bytes | Big-endian word whose high byte feeds `_bgColor`’s green channel.[engines/director/castmember/richtext.cpp (approx. lines 55-58)] |
| `<background blue word>` | 2 bytes | Big-endian word whose high byte completes the background color.[engines/director/castmember/richtext.cpp (approx. lines 55-58)] |

## Step 2 – Resolving RTE child resources

After the header is decoded, `RichTextCastMember::load()` walks the cast member’s child list to locate the three RTE payloads: editor formatting (RTE0), Unicode/plain text (RTE1), and the prerendered bitmap (RTE2).[engines/director/castmember/richtext.cpp (approx. lines 90-135)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<'R','T','E','0'>` | 4 bytes | Child tag that identifies Paige editor data. ScummVM currently logs a warning because authoring metadata is not consumed.[engines/director/castmember/richtext.cpp (approx. lines 100-117)] |
| `<'R','T','E','1'>` | 4 bytes | Tag pointing to the Unicode/plain text payload; `Common::U32String` imports the data using the platform encoding.[engines/director/castmember/richtext.cpp (approx. lines 118-123)] |
| `<'R','T','E','2'>` | 4 bytes | Tag for the cached bitmap. The engine builds a `Picture` surface via `RTE2::createSurface()` to display rich text in-game.[engines/director/castmember/richtext.cpp (approx. lines 124-135)] |

If any of these child resources are missing the loader warns, but the cast member is still marked as loaded so subsequent accesses do not retry the enumeration.[engines/director/castmember/richtext.cpp (approx. lines 112-137)]
