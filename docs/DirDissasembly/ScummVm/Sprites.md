# Sprite Channel Records

ScummVM populates on-stage sprite state by slicing each frame’s channel block and decoding the per-sprite bytes in `Frame::readSpriteD*()` and the helper `readSpriteDataD*()` routines.[engines/director/frame.cpp (approx. lines 301-2126)] `Sprite` itself tracks ink, cast membership, behaviour lists, and puppet flags, so the loader conditionally skips bytes when a sprite is puppet-controlled or auto-puppet is active.[engines/director/sprite.cpp (approx. lines 30-164)][engines/director/frame.h (approx. lines 49-62)]

## Step 1 – Director 2 sprite channels (kSprChannelSizeD2 = 0x10)

Early Mac/Windows movies store 16 bytes per sprite. Colours are Mac signed bytes that ScummVM remaps into 0–255, and QuickDraw shapes reserve the cast member slot for a pattern id.[engines/director/frame.cpp (approx. lines 325-424)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x00` | 1 byte | Script id (cast member) assigned to the sprite channel.[engines/director/frame.cpp (approx. lines 328-330)] |
| `0x01` | 1 byte | Sprite type; set to inactive when 0. Skipped for puppet sprites.[engines/director/frame.cpp (approx. lines 331-338)] |
| `0x02` | 1 byte | Foreground colour (signed byte xor 0x80). Puppet sprites leave the previous colour.[engines/director/frame.cpp (approx. lines 339-345)] |
| `0x03` | 1 byte | Background colour (signed byte xor 0x80).[engines/director/frame.cpp (approx. lines 346-353)] |
| `0x04` | 1 byte | Line thickness; upper bit masked off.[engines/director/frame.cpp (approx. lines 354-360)] |
| `0x05` | 1 byte | Ink flags: low 6 bits ink mode, bit 6 trails, bit 7 stretch.[engines/director/frame.cpp (approx. lines 361-371)] |
| `0x06`..`0x07` | 2 bytes | Cast member id or QuickDraw pattern for shapes. Puppet sprites discard the word.[engines/director/frame.cpp (approx. lines 372-383)] |
| `0x08`..`0x09` | 2 bytes | Top coordinate (`_startPoint.y`).[engines/director/frame.cpp (approx. lines 386-391)] |
| `0x0A`..`0x0B` | 2 bytes | Left coordinate (`_startPoint.x`).[engines/director/frame.cpp (approx. lines 392-398)] |
| `0x0C`..`0x0D` | 2 bytes | Height in pixels.[engines/director/frame.cpp (approx. lines 400-405)] |
| `0x0E`..`0x0F` | 2 bytes | Width in pixels; zero or negative dimensions collapse the sprite to zero size.[engines/director/frame.cpp (approx. lines 406-424)] |

## Step 2 – Director 4 sprite channels (kSprChannelSizeD4 = 0x14)

Director 4 promotes most fields to unsigned values, stores both script and cast ids, and adds colour-code/blend bytes at the end of the record.[engines/director/frame.cpp (approx. lines 676-798)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x00` | 1 byte | Script id (cast member) linked to the sprite.[engines/director/frame.cpp (approx. lines 680-682)] |
| `0x01` | 1 byte | Sprite type; puppet sprites skip and keep their previous value.[engines/director/frame.cpp (approx. lines 683-689)] |
| `0x02` | 1 byte | Foreground colour (0–255). Puppet sprites skip this field.[engines/director/frame.cpp (approx. lines 690-697)] |
| `0x03` | 1 byte | Background colour.[engines/director/frame.cpp (approx. lines 698-703)] |
| `0x04` | 1 byte | Thickness.[engines/director/frame.cpp (approx. lines 704-710)] |
| `0x05` | 1 byte | Ink flags (mode/trails/stretch).[engines/director/frame.cpp (approx. lines 711-720)] |
| `0x06`..`0x07` | 2 bytes | Cast member id or QuickDraw pattern for shape sprites.[engines/director/frame.cpp (approx. lines 723-732)] |
| `0x08`..`0x09` | 2 bytes | Top coordinate.[engines/director/frame.cpp (approx. lines 734-739)] |
| `0x0A`..`0x0B` | 2 bytes | Left coordinate.[engines/director/frame.cpp (approx. lines 741-746)] |
| `0x0C`..`0x0D` | 2 bytes | Height.[engines/director/frame.cpp (approx. lines 748-753)] |
| `0x0E`..`0x0F` | 2 bytes | Width.[engines/director/frame.cpp (approx. lines 755-760)] |
| `0x10`..`0x11` | 2 bytes | Script id (16-bit) reused when behaviours fire.[engines/director/frame.cpp (approx. lines 762-764)] |
| `0x12` | 1 byte | Colour-code flags: lower nibble stage colour, bit 6 editable, bit 7 moveable.[engines/director/frame.cpp (approx. lines 765-779)] |
| `0x13` | 1 byte | Blend amount for inks that support opacity.[engines/director/frame.cpp (approx. lines 780-786)] |

## Step 3 – Director 5 sprite channels (kSprChannelSizeD5 = 0x18)

Director 5 adds signed cast-library ids for both the sprite and its script, and appends a separate thickness byte after the blend amount.[engines/director/frame.cpp (approx. lines 1047-1172)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x00` | 1 byte | Sprite type (skipped for puppet sprites).[engines/director/frame.cpp (approx. lines 1049-1056)] |
| `0x01` | 1 byte | Ink flags and mode.[engines/director/frame.cpp (approx. lines 1057-1066)] |
| `0x02`..`0x03` | 2 bytes | Cast library id (signed). Puppet and auto-puppet sprites keep the old library id.[engines/director/frame.cpp (approx. lines 1068-1074)] |
| `0x04`..`0x05` | 2 bytes | Cast member id (inherits previous cast lib when not explicitly set).[engines/director/frame.cpp (approx. lines 1075-1083)] |
| `0x06`..`0x07` | 2 bytes | Script cast library id.[engines/director/frame.cpp (approx. lines 1084-1088)] |
| `0x08`..`0x09` | 2 bytes | Script member id.[engines/director/frame.cpp (approx. lines 1089-1093)] |
| `0x0A` | 1 byte | Foreground colour.[engines/director/frame.cpp (approx. lines 1094-1100)] |
| `0x0B` | 1 byte | Background colour.[engines/director/frame.cpp (approx. lines 1101-1106)] |
| `0x0C`..`0x0D` | 2 bytes | Top coordinate.[engines/director/frame.cpp (approx. lines 1108-1113)] |
| `0x0E`..`0x0F` | 2 bytes | Left coordinate.[engines/director/frame.cpp (approx. lines 1114-1119)] |
| `0x10`..`0x11` | 2 bytes | Height.[engines/director/frame.cpp (approx. lines 1122-1127)] |
| `0x12`..`0x13` | 2 bytes | Width.[engines/director/frame.cpp (approx. lines 1129-1134)] |
| `0x14` | 1 byte | Colour-code flags (editable, moveable, RGB markers).[engines/director/frame.cpp (approx. lines 1136-1149)] |
| `0x15` | 1 byte | Blend amount.[engines/director/frame.cpp (approx. lines 1152-1157)] |
| `0x16` | 1 byte | Thickness byte (retained even when inks ignore it).[engines/director/frame.cpp (approx. lines 1159-1164)] |
| `0x17` | 1 byte | Reserved padding.[engines/director/frame.cpp (approx. lines 1165-1168)] |

## Step 4 – Director 6 sprite channels (kSprChannelSizeD6 = 0x18)

Director 6 reuses the 24-byte footprint but repurposes the early words for sprite-detail indices that link into Afterburner detail tables. `Score::loadFrameSpriteDetails()` uses `_spriteListIdx` to fetch behaviours and names.[engines/director/frame.cpp (approx. lines 1473-1631)][engines/director/score.cpp (approx. lines 1897-2007)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x00` | 1 byte | Sprite type (skipped for puppet sprites).[engines/director/frame.cpp (approx. lines 1505-1510)] |
| `0x01` | 1 byte | Ink flags; skipped if auto-puppet ink is active.[engines/director/frame.cpp (approx. lines 1511-1519)] |
| `0x02` | 1 byte | Foreground colour unless auto-puppet fore-colour is set.[engines/director/frame.cpp (approx. lines 1523-1528)] |
| `0x03` | 1 byte | Background colour with the same auto-puppet guard.[engines/director/frame.cpp (approx. lines 1530-1535)] |
| `0x04`..`0x05` | 2 bytes | Cast library id; skipped when puppet or auto-puppet-cast is enabled.[engines/director/frame.cpp (approx. lines 1537-1543)] |
| `0x06`..`0x07` | 2 bytes | Cast member id.[engines/director/frame.cpp (approx. lines 1545-1551)] |
| `0x08`..`0x0B` | 4 bytes | Primary sprite-detail index (32-bit) pointing into the detail stream.[engines/director/frame.cpp (approx. lines 1553-1558)] |
| `0x0C`..`0x0D` | 2 bytes | Low-word shadow for `_spriteListIdx`; Afterburner diffs may stream only these bytes.[engines/director/frame.cpp (approx. lines 1560-1565)] |
| `0x0E`..`0x0F` | 2 bytes | Top coordinate (skipped when auto-puppet location is set).[engines/director/frame.cpp (approx. lines 1567-1572)] |
| `0x10`..`0x11` | 2 bytes | Left coordinate (auto-puppet aware).[engines/director/frame.cpp (approx. lines 1574-1579)] |
| `0x12`..`0x13` | 2 bytes | Height (auto-puppet height skip).[engines/director/frame.cpp (approx. lines 1581-1586)] |
| `0x14`..`0x15` | 2 bytes | Width (auto-puppet width skip).[engines/director/frame.cpp (approx. lines 1588-1593)] |
| `0x16` | 1 byte | Colour-code flags (skipped when auto-puppet moveable is set).[engines/director/frame.cpp (approx. lines 1595-1609)] |
| `0x17` | 1 byte | Blend amount (puppet sprites skip).[engines/director/frame.cpp (approx. lines 1611-1616)] |
| `0x18` | 1 byte | Thickness byte (puppet sprites skip).[engines/director/frame.cpp (approx. lines 1618-1623)] |
| `0x19` | 1 byte | Reserved padding.[engines/director/frame.cpp (approx. lines 1625-1631)] |

## Step 5 – Director 7 sprite channels (kSprChannelSizeD7 = 0x30)

Director 7 doubles the record to 48 bytes, adding per-channel RGB triples, 32-bit rotation/skew angles, and behaviour padding. Auto-puppet guards continue to suppress updates when the stage is under puppet control.[engines/director/frame.cpp (approx. lines 1924-2125)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x00` | 1 byte | Sprite type (auto-puppet aware).[engines/director/frame.cpp (approx. lines 1956-1963)] |
| `0x01` | 1 byte | Ink flags.[engines/director/frame.cpp (approx. lines 1965-1974)] |
| `0x02` | 1 byte | Foreground colour (skipped by auto-puppet fore-colour).[engines/director/frame.cpp (approx. lines 1976-1981)] |
| `0x03` | 1 byte | Background colour.[engines/director/frame.cpp (approx. lines 1983-1988)] |
| `0x04`..`0x05` | 2 bytes | Cast library id (auto-puppet aware).[engines/director/frame.cpp (approx. lines 1990-1996)] |
| `0x06`..`0x07` | 2 bytes | Cast member id.[engines/director/frame.cpp (approx. lines 1998-2004)] |
| `0x08`..`0x0B` | 4 bytes | Sprite-detail index (32-bit) for behaviour/name records.[engines/director/frame.cpp (approx. lines 2006-2011)] |
| `0x0C`..`0x0D` | 2 bytes | Low-word shadow for `_spriteListIdx`; partial updates may only touch these bytes.[engines/director/frame.cpp (approx. lines 2013-2018)] |
| `0x0E`..`0x0F` | 2 bytes | Top coordinate (auto-puppet aware).[engines/director/frame.cpp (approx. lines 2020-2025)] |
| `0x10`..`0x11` | 2 bytes | Left coordinate.[engines/director/frame.cpp (approx. lines 2027-2032)] |
| `0x12`..`0x13` | 2 bytes | Height.[engines/director/frame.cpp (approx. lines 2034-2038)] |
| `0x14`..`0x15` | 2 bytes | Width.[engines/director/frame.cpp (approx. lines 2035-2039)] |
| `0x16` | 1 byte | Colour-code flags.[engines/director/frame.cpp (approx. lines 2040-2049)] |
| `0x17` | 1 byte | Blend amount.[engines/director/frame.cpp (approx. lines 2050-2055)] |
| `0x18` | 1 byte | Thickness byte.[engines/director/frame.cpp (approx. lines 2056-2061)] |
| `0x19` | 1 byte | Sprite flags (Director 7-only).[engines/director/frame.cpp (approx. lines 2062-2080)] |
| `0x1A` | 1 byte | Foreground green component (auto-puppet aware).[engines/director/frame.cpp (approx. lines 2081-2086)] |
| `0x1B` | 1 byte | Background green component.[engines/director/frame.cpp (approx. lines 2088-2093)] |
| `0x1C` | 1 byte | Foreground blue component.[engines/director/frame.cpp (approx. lines 2095-2100)] |
| `0x1D` | 1 byte | Background blue component.[engines/director/frame.cpp (approx. lines 2102-2107)] |
| `0x1E`..`0x21` | 4 bytes | Rotation angle in fixed-point units.[engines/director/frame.cpp (approx. lines 2109-2114)] |
| `0x22`..`0x23` | 2 bytes | Lower half of the rotation word used by some projectors.[engines/director/frame.cpp (approx. lines 2110-2114)] |
| `0x24`..`0x27` | 4 bytes | Skew angle.[engines/director/frame.cpp (approx. lines 2115-2117)] |
| `0x28`..`0x2F` | 8 bytes | Reserved alignment bytes; logged when non-zero.[engines/director/frame.cpp (approx. lines 2118-2124)] |

## Step 6 – Behaviour and name lookup (Director 6+)

For Director 6 and later, any non-zero `_spriteListIdx` triggers a lookup in the Afterburner detail list. Entry `spriteListIdx` supplies a `SpriteInfo` record, `spriteListIdx + 1` lists behaviour initialisers, and `spriteListIdx + 2` stores the sprite name string.[engines/director/score.cpp (approx. lines 1897-2007)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `detailIndex*4`..`+3` | 4 bytes | Offset into the detail stream; even indices map to `SpriteInfo`, odd indices to behaviours, and the next entry to the name string.[engines/director/score.cpp (approx. lines 1778-2007)] |

