# Font Mapping Resources

Director movies populate the stage font list by combining the movie archive's `VWFM`/`Fmap` byte tables with optional cross-platform `FXmp` scripts before assigning fonts through `FontMapEntry` records.[engines/director/cast.cpp (approx. lines 689-719)][engines/director/fonts.cpp (approx. lines 32-441)][engines/director/cast.h (approx. lines 72-79)] `Cast::loadCast()` also primes the Mac font manager with `loadFonts()` so the subsequent lookups can register each name with a platform font id.[engines/director/cast.cpp (approx. lines 689-695)]

## Step 1 – Director 2–3 `VWFM` font maps

`Cast::loadFontMap()` reads the classic font directory: a count of font ids, a packed array of 16-bit identifiers, and a Pascal string block with the font names.[engines/director/fonts.cpp (approx. lines 32-63)] Each name is passed to `MacFontManager::registerFontName()` so `_fontMap[id]->toFont` tracks the runtime font id that text members will use.

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x0000..0x0001` | 2 bytes | Number of font entries. ScummVM multiplies this count by two and adds two to find the start of the string block.[engines/director/fonts.cpp (approx. lines 38-40)] |
| `0x0002..0x0001 + 2 * count` | `2 * count` bytes | Sequential 16-bit font ids; after reading each id the loader jumps into the string block to fetch the associated name.[engines/director/fonts.cpp (approx. lines 42-47)] |

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `stringBlock+0x00` | 1 byte | Pascal string length (`size`) for the current font name.[engines/director/fonts.cpp (approx. lines 48-52)] |
| `stringBlock+0x01..stringBlock+0x00+size` | `size` bytes | ASCII font name registered with the window manager; the cursor advances so the next id resolves to the following string.[engines/director/fonts.cpp (approx. lines 51-62)] |

## Step 2 – Director 4+ `Fmap` records

Later projectors provide a richer `Fmap` chunk with explicit platform tags, size remapping flags, and a shared string table.[engines/director/fonts.cpp (approx. lines 66-110)] `Cast::loadFontMapV4()` first captures the header describing the table and string block boundaries before iterating over `entriesUsed` records.

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x0000..0x0003` | 4 bytes | `mapLength`, the size of the body table containing offsets and platform ids.[engines/director/fonts.cpp (approx. lines 72-75)] |
| `0x0004..0x0007` | 4 bytes | `namesLength`; combined with `mapLength` to compute `namesStart`, the base of the string table.[engines/director/fonts.cpp (approx. lines 72-75)] |
| `bodyStart+0x00..bodyStart+0x03` | 4 bytes | Reserved word logged but unused (`unk1`).[engines/director/fonts.cpp (approx. lines 77-83)] |
| `bodyStart+0x04..bodyStart+0x07` | 4 bytes | Reserved (`unk2`).[engines/director/fonts.cpp (approx. lines 77-83)] |
| `bodyStart+0x08..bodyStart+0x0B` | 4 bytes | `entriesUsed`, the number of live rows the loop will parse.[engines/director/fonts.cpp (approx. lines 79-85)] |
| `bodyStart+0x0C..bodyStart+0x0F` | 4 bytes | `entriesTotal`, total slots including unused entries.[engines/director/fonts.cpp (approx. lines 79-83)] |
| `bodyStart+0x10..bodyStart+0x1B` | 12 bytes | Three additional reserved words (`unk3`–`unk5`) preserved for completeness.[engines/director/fonts.cpp (approx. lines 81-83)] |

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `entry+0x00..entry+0x03` | 4 bytes | Offset into the names block; ScummVM seeks to `namesStart + nameOffset` before reading the string.[engines/director/fonts.cpp (approx. lines 85-92)] |
| `entry+0x04..entry+0x05` | 2 bytes | Platform tag converted with `platformFromID()` to distinguish Mac vs. Windows font ids.[engines/director/fonts.cpp (approx. lines 94-100)] |
| `entry+0x06..entry+0x07` | 2 bytes | Cast font id associated with the name and platform.[engines/director/fonts.cpp (approx. lines 95-109)] |

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `namesStart+offset..+offset+0x03` | 4 bytes | String length for the font name referenced by the entry.[engines/director/fonts.cpp (approx. lines 88-92)] |
| `namesStart+offset+0x04..+0x04+length-1` | `length` bytes | UTF-8/ASCII font name; reused directly on Mac or remapped with `_fontXPlatformMap` on Windows.[engines/director/fonts.cpp (approx. lines 88-107)] |

Windows entries reuse any cross-platform metadata that `FXmp` supplied so glyph remapping and size overrides remain intact.[engines/director/fonts.cpp (approx. lines 99-104)]

## Step 3 – `FXmp` cross-platform font scripts

`Cast::loadFXmp()` streams ASCII configuration lines that either remap character codes between Mac and Windows or provide alternate font and size mappings.[engines/director/fonts.cpp (approx. lines 250-441)] `readFXmpLine()` tokenizes each line into words, integers, colons, arrows, and quoted strings, enforcing the `Mac:`/`Win:` prefixes before acting on the payload.

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<platform letters>` | 3 bytes | ASCII word `Mac` or `Win` marking the source platform for this directive.[engines/director/fonts.cpp (approx. lines 266-279)] |
| `3A` (`:`) | 1 byte | Colon separator consumed after the platform token.[engines/director/fonts.cpp (approx. lines 281-283)] |
| `<optional font name>` | variable | Either a bare word or quoted string naming the source font; if absent, the line describes character remapping.[engines/director/fonts.cpp (approx. lines 287-299)] |
| `3D 3E` (`=>`) | 2 bytes | Arrow delimiter between source and target tokens.[engines/director/fonts.cpp (approx. lines 294-299)] |
| `<target platform letters>` | 3 bytes | Destination platform (`Win` for Mac sources, `Mac` for Windows sources).[engines/director/fonts.cpp (approx. lines 300-313)] |
| `3A` (`:`) | 1 byte | Second colon before the mapping payload.[engines/director/fonts.cpp (approx. lines 315-319)] |
| `<payload bytes>` | variable | Either `fromChar => toChar` integer pairs for character remaps or a target font name followed by optional `Map All|None` and size translation pairs.[engines/director/fonts.cpp (approx. lines 321-418)] |
| `0D 0A` or `0D` | 1–2 bytes | Line ending that signals the parser to return to `loadFXmp()` for the next directive.[engines/director/fonts.cpp (approx. lines 232-260)] |

Character lines populate `_macCharsToWin` or `_winCharsToMac`, while font lines allocate a `FontXPlatformInfo` record that stores the destination font name, whether characters should be remapped, and a dictionary of size substitutions.[engines/director/fonts.cpp (approx. lines 344-438)] When a Windows `Fmap` entry matches an `FXmp` font name, the loader copies these overrides into the final `FontMapEntry` so text members render with the correct glyph metrics on both platforms.[engines/director/fonts.cpp (approx. lines 99-109)][engines/director/cast.h (approx. lines 72-79)]
