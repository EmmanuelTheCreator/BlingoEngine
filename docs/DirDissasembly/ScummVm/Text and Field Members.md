# Text and Field Cast Members

ScummVM represents both static text and editable field cast members with `TextCastMember`. The constructor receives a flag byte from the score (region/ink hints) and then walks the cast stream to pull per-version layout and formatting data. The tables below record the exact bytes consumed so it is easy to distinguish which records produce plain text blocks versus editable fields.

## Step 1 – Director 2 text and field records (version < 0x300)

Early Director movies pack most formatting into a short header: border metrics, background color triplets, and legacy flags. Director 2 keeps a dedicated byte for the drop shadow and another for unknown flag bits; ScummVM warns when unused high bits are set because only the low nibble is understood.[engines/director/castmember/text.cpp (approx. lines 73-104)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<border size>` | 1 byte | Frame thickness around the text box.[engines/director/castmember/text.cpp (approx. lines 75-99)] |
| `<gutter size>` | 1 byte | Padding between the border and text content.[engines/director/castmember/text.cpp (approx. lines 75-99)] |
| `<box shadow>` | 1 byte | Drop-shadow distance stored for legacy rendering.[engines/director/castmember/text.cpp (approx. lines 75-99)] |
| `<text type>` | 1 byte | Cast member subtype such as fixed or scrolling text.[engines/director/castmember/text.cpp (approx. lines 78-99)] |
| `<text alignment>` | 2 bytes | QuickDraw alignment word read with `readUint16()`.[engines/director/castmember/text.cpp (approx. lines 78-99)] |
| `<background red>` | 2 bytes | Background color component (`_bgpalinfo1`).[engines/director/castmember/text.cpp (approx. lines 80-99)] |
| `<background green>` | 2 bytes | Second palette component (`_bgpalinfo2`).[engines/director/castmember/text.cpp (approx. lines 80-99)] |
| `<background blue>` | 2 bytes | Third palette component (`_bgpalinfo3`).[engines/director/castmember/text.cpp (approx. lines 80-99)] |
| `<pad2>` | 2 bytes | Reserved word that is usually zero; non-zero values trigger a warning.[engines/director/castmember/text.cpp (approx. lines 84-103)] |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit values for `_initialRect`.[engines/director/castmember/text.cpp (approx. lines 95-104)][engines/director/movie.cpp (approx. lines 276-284)] |
| `<pad3>` | 2 bytes | Additional reserved word retained for compatibility.[engines/director/castmember/text.cpp (approx. lines 95-104)] |
| `<text shadow>` | 1 byte | Stored only in Director 2; copied into `_textShadow`.[engines/director/castmember/text.cpp (approx. lines 98-99)] |
| `<text flags>` | 1 byte | Legacy flag byte; high bits (`0xf8`) are not interpreted.[engines/director/castmember/text.cpp (approx. lines 98-101)] |
| `<total text height>` | 2 bytes | Authoring-time text height used to size widgets.[engines/director/castmember/text.cpp (approx. lines 100-104)] |

## Step 2 – Director 3 text and field records (0x300 ≤ version < 0x400)

Director 3 keeps the same header fields but replaces the single-byte flag with a 16-bit word. Bit 0 marks the cast as editable (a field), bit 1 enables auto-tab, and bit 2 disables word wrapping.[engines/director/castmember/text.cpp (approx. lines 105-111)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<border size>` | 1 byte | Identical border metric carried over from Director 2 exports.[engines/director/castmember/text.cpp (approx. lines 75-108)] |
| `<gutter size>` | 1 byte | Interior padding value copied unchanged from legacy records.[engines/director/castmember/text.cpp (approx. lines 75-108)] |
| `<box shadow>` | 1 byte | Drop-shadow depth for the box; `_textShadow` itself remains at the constructor default here.[engines/director/castmember/text.cpp (approx. lines 75-110)] |
| `<text type>` | 1 byte | Cast subtype selector preserved from the Director 2 layout.[engines/director/castmember/text.cpp (approx. lines 75-108)] |
| `<text alignment>` | 2 bytes | Alignment word read with `readUint16()` before the palette entries.[engines/director/castmember/text.cpp (approx. lines 78-108)] |
| `<background red>` | 2 bytes | `_bgpalinfo1` component copied verbatim.[engines/director/castmember/text.cpp (approx. lines 80-108)] |
| `<background green>` | 2 bytes | `_bgpalinfo2` component copied verbatim.[engines/director/castmember/text.cpp (approx. lines 80-108)] |
| `<background blue>` | 2 bytes | `_bgpalinfo3` component copied verbatim.[engines/director/castmember/text.cpp (approx. lines 80-108)] |
| `<pad2>` | 2 bytes | Reserved word preceding the rect, still observed in exports.[engines/director/castmember/text.cpp (approx. lines 105-110)] |
| `<rect.top>`..`<rect.right>` | 8 bytes | `_initialRect` coordinates read via `Movie::readRect()`.[engines/director/castmember/text.cpp (approx. lines 105-110)][engines/director/movie.cpp (approx. lines 276-284)] |
| `<pad3>` | 2 bytes | Additional reserved word retained for parity with Director 2.[engines/director/castmember/text.cpp (approx. lines 105-110)] |
| `<text flags>` | 2 bytes | Flag word controlling editable, auto-tab, and wrap behavior; `_editable` is set when bit 0 is high.[engines/director/castmember/text.cpp (approx. lines 108-110)] |
| `<total text height>` | 2 bytes | Documented text height that informs layout widgets.[engines/director/castmember/text.cpp (approx. lines 109-111)] |

## Step 3 – Director 4–10 text and field records (0x400 ≤ version < 0x1100)

Later versions extend the record with a scroll offset, a maximum box height, and an 8-bit flag byte. The low bit still marks editable fields, so ScummVM toggles `_type` between text and field behavior based on that flag.[engines/director/castmember/text.cpp (approx. lines 120-145)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<border size>` | 1 byte | Border thickness identical to earlier versions.[engines/director/castmember/text.cpp (approx. lines 121-145)] |
| `<gutter size>` | 1 byte | Interior padding between the border and text.[engines/director/castmember/text.cpp (approx. lines 121-145)] |
| `<box shadow>` | 1 byte | Drop-shadow offset for the text box.[engines/director/castmember/text.cpp (approx. lines 121-145)] |
| `<text type>` | 1 byte | Member subtype (static, scrolling, field, button text, etc.).[engines/director/castmember/text.cpp (approx. lines 124-138)] |
| `<text alignment>` | 2 bytes | Signed alignment word; values include -1 for right-align.[engines/director/castmember/text.cpp (approx. lines 125-126)] |
| `<background red>` | 2 bytes | `_bgpalinfo1`, the red palette entry.[engines/director/castmember/text.cpp (approx. lines 127-145)] |
| `<background green>` | 2 bytes | `_bgpalinfo2`, the green component.[engines/director/castmember/text.cpp (approx. lines 127-145)] |
| `<background blue>` | 2 bytes | `_bgpalinfo3`, the blue component.[engines/director/castmember/text.cpp (approx. lines 127-145)] |
| `<scroll offset>` | 2 bytes | `_scroll` field storing the initial scroll position.[engines/director/castmember/text.cpp (approx. lines 129-133)] |
| `<rect.top>`..`<rect.right>` | 8 bytes | Cast member bounds read through `Movie::readRect()`.[engines/director/castmember/text.cpp (approx. lines 134-140)][engines/director/movie.cpp (approx. lines 276-284)] |
| `<max height>` | 2 bytes | Maximum box height recorded for multi-line text.[engines/director/castmember/text.cpp (approx. lines 134-140)] |
| `<text shadow>` | 1 byte | Stored shadow depth; Director 4 reintroduces this byte.[engines/director/castmember/text.cpp (approx. lines 136-140)] |
| `<text flags>` | 1 byte | Editable/auto-tab/word-wrap bitfield; bit 0 sets `_editable` so the cast acts as a field.[engines/director/castmember/text.cpp (approx. lines 137-139)] |
| `<text height>` | 2 bytes | Preferred text height for layout inside the box.[engines/director/castmember/text.cpp (approx. lines 139-145)] |

## Step 4 – Button text records

When the same routine is invoked for button cast members, the engine consumes a final big-endian word to pick the `ButtonType`. Director stores button types starting at 1, so ScummVM subtracts one after reading the value.[engines/director/castmember/text.cpp (approx. lines 151-154)]

## Step 5 – Loading text content from STXT resources

The byte streams above only hold formatting. Actual text lives in STXT child resources, which are located differently per version: Director 4 and later enumerate child chunks, while older movies reuse the cast ID. `TextCastMember::load()` resolves that STXT resource and imports the text along with font and style runs, which is why the constructor seeds `_fontId` with 1 and leaves most formatting defaults until `importStxt()` executes.[engines/director/castmember/text.cpp (approx. lines 612-639)]

These layouts make it straightforward to spot editable fields: either the Director 3 flag word or the Director 4+ flag byte has bit 0 set, prompting ScummVM to enable field behaviors such as `kTheEditable` and chunk-level text accessors once the STXT payload is loaded.[engines/director/castmember/text.cpp (approx. lines 108-139)][engines/director/castmember/text.cpp (approx. lines 646-700)]
