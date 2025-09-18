# Score Resource and Frame Loading

ScummVM decodes Director timeline data from `VWSC` score resources and fan-out tables that describe per-frame sprite state. `Score::loadFrames()` pulls the score stream into a memory reader, applies version-specific headers, and then feeds every frame record into `Frame::readChannel()` to populate the main channel state and sprite channels.[engines/director/score.cpp (approx. lines 1740-2117)][engines/director/frame.cpp (approx. lines 154-2194)]

## Step 1 – Director 2 and 3 `VWSC` header (version < 0x400)

Legacy Mac files expose only a 32-bit length before the first frame payload. ScummVM records the bound and assumes the classic 30-channel stage width.[engines/director/score.cpp (approx. lines 1756-1760)][engines/director/frame.h (approx. lines 49-52)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x0000`..`0x0003` | 4 bytes | Total score stream length mirrored into `_framesStreamSize` to guard `readOneFrame()`.[engines/director/score.cpp (approx. lines 1756-1759)] |

## Step 2 – Director 6+ detail preamble (0x600 ≤ version < 0x1100)

Afterburner-enabled scores prepend a descriptor for the per-sprite detail list. ScummVM reads the overall score length, the embedded detail table version, and the offset where the detail list begins.[engines/director/score.cpp (approx. lines 1761-1769)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x0000`..`0x0003` | 4 bytes | `_framesStreamSize` bound for the rest of the reader.[engines/director/score.cpp (approx. lines 1761-1763)] |
| `0x0004`..`0x0007` | 4 bytes | Detail table version stored as `ver`; logged for debugging.[engines/director/score.cpp (approx. lines 1762-1767)] |
| `0x0008`..`0x000B` | 4 bytes | Offset to the detail list; `Score` seeks here to parse sprite metadata tables.[engines/director/score.cpp (approx. lines 1764-1769)] |

## Step 3 – Director 6+ detail list header

At the detail list start, the loader reads the entry count, the pointer table width, and the maximum payload size for sprite detail blobs before capturing every offset.[engines/director/score.cpp (approx. lines 1769-1795)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `listStart+0x00`..`listStart+0x03` | 4 bytes | `numEntries` covering sprite info, behaviours, and names.[engines/director/score.cpp (approx. lines 1769-1776)] |
| `listStart+0x04`..`listStart+0x07` | 4 bytes | `listSize` (count of 32-bit offsets) used to compute `_indexStart`.[engines/director/score.cpp (approx. lines 1769-1779)] |
| `listStart+0x08`..`listStart+0x0B` | 4 bytes | `maxDataLen` hint for largest detail blob.[engines/director/score.cpp (approx. lines 1770-1775)] |
| `listStart+0x0C`..`listStart+0x0C+4*numEntries-1` | `4 * numEntries` bytes | Offset table; each value is added to `_frameDataOffset` to locate individual sprite detail records.[engines/director/score.cpp (approx. lines 1778-1795)] |

## Step 4 – Director 4–10 `VWSC` header (0x400 ≤ version < 0x1100)

All Director 4+ exports place a fixed header ahead of the frame data. ScummVM captures the size bounds, the advertised frame count, and the channel geometry so it can pre-size the `Frame` caches.[engines/director/score.cpp (approx. lines 1804-1826)][engines/director/frame.h (approx. lines 52-62)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x0000`..`0x0003` | 4 bytes | `_framesStreamSize`, the total size budget for channel data.[engines/director/score.cpp (approx. lines 1804-1806)] |
| `0x0004`..`0x0007` | 4 bytes | `_frame1Offset`, location of the first frame payload.[engines/director/score.cpp (approx. lines 1805-1807)] |
| `0x0008`..`0x000B` | 4 bytes | Claimed `_numOfFrames`; ScummVM recomputes the true count when caching frames.[engines/director/score.cpp (approx. lines 1806-1868)] |
| `0x000C`..`0x000D` | 2 bytes | `_framesVersion` differentiating Director 5 (`≤ 7`), Director 6 (`8-13`), and newer exports.[engines/director/score.cpp (approx. lines 1807-1818)] |
| `0x000E`..`0x000F` | 2 bytes | `_spriteRecordSize` for each channel entry in the frame records.[engines/director/score.cpp (approx. lines 1808-1810)] |
| `0x0010`..`0x0011` | 2 bytes | `_numChannels` allocated in the score file.[engines/director/score.cpp (approx. lines 1808-1810)] |
| `0x0012`..`0x0013` | 2 bytes | Optional `numChannelsDisplayed`; Director 5 defaults to 48, Director 6 to 120, otherwise read explicitly.[engines/director/score.cpp (approx. lines 1812-1820)] |
| `0x0014`..`0x0015` | 2 bytes | Padding skipped for older frame versions (`≤ 13`).[engines/director/score.cpp (approx. lines 1814-1821)] |

## Step 5 – Frame index entries (Director 6+)

When seeking to a particular frame, `Score::seekToMemberInList()` pulls a pair of offsets from the index table so it can locate the compressed frame payload inside `_frameDataOffset`.[engines/director/score.cpp (approx. lines 2010-2023)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `index[frame]*4`..`+3` | 4 bytes | Relative frame start (`off`) added to `_frameDataOffset`.[engines/director/score.cpp (approx. lines 2016-2022)] |
| `index[frame]*4+4`..`+7` | 4 bytes | Next frame start; difference yields the current frame size used for logging.[engines/director/score.cpp (approx. lines 2016-2021)] |


## Step 6 – Frame channel dispatch

`Frame::readChannel()` slices each frame payload using the version-specific main and sprite channel sizes defined in `frame.h`, then forwards the bytes to the correct per-version readers.[engines/director/frame.cpp (approx. lines 115-174)][engines/director/frame.h (approx. lines 49-62)] Offsets less than the main channel size refresh the control block, while later offsets land on individual sprite channels handled by `readSpriteD*()`.[engines/director/frame.cpp (approx. lines 154-174)]

| Version Range | Main Channel Bytes | Sprite Channel Bytes | Reader |
| --- | --- | --- | --- |
| `< 0x400` | `0x20` (`kMainChannelSizeD2`) | `0x10` (`kSprChannelSizeD2`) | `readChannelD2()` dispatching to `readMainChannelsD2()`/`readSpriteD2()`.[engines/director/frame.cpp (approx. lines 118-173)][engines/director/frame.h (approx. lines 49-50)] |
| `0x400–0x4FF` | `0x28` (`kMainChannelSizeD4`) | `0x14` (`kSprChannelSizeD4`) | `readChannelD4()` forwarding to the Director 4 readers.[engines/director/frame.cpp (approx. lines 120-433)][engines/director/frame.h (approx. lines 52-53)] |
| `0x500–0x5FF` | `0x30` (`kMainChannelSizeD5`) | `0x18` (`kSprChannelSizeD5`) | `readChannelD5()` for Director 5 payloads.[engines/director/frame.cpp (approx. lines 122-833)][engines/director/frame.h (approx. lines 55-56)] |
| `0x600–0x6FF` | `0x90` (`kMainChannelSizeD6`) | `0x18` (`kSprChannelSizeD6`) | `readChannelD6()` parsing Afterburner detail indices.[engines/director/frame.cpp (approx. lines 124-1205)][engines/director/frame.h (approx. lines 58-59)] |
| `0x700–0x10FF` | `0x120` (`kMainChannelSizeD7`) | `0x30` (`kSprChannelSizeD7`) | `readChannelD7()` for Director 7+ extended records.[engines/director/frame.cpp (approx. lines 126-2125)][engines/director/frame.h (approx. lines 61-62)] |

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x0000..mainSize-1` | `mainSize` bytes | Control block routed to `readMainChannelsD*()`; partial updates adjust `offset` and `size` so only modified fields are consumed.[engines/director/frame.cpp (approx. lines 154-162)] |
| `mainSize + spriteIndex * sprSize .. + sprSize - 1` | `sprSize` bytes per sprite | Sprite channel payload forwarded to `readSpriteD*()` with puppet guards and Afterburner detail lookups applied after the data is cached.[engines/director/frame.cpp (approx. lines 162-174)][engines/director/score.cpp (approx. lines 1897-2007)] |

## Step 7 – Frame payloads and channel descriptors


Each frame begins with a 16-bit size followed by channel records. Director 2–3 compress channel descriptors into bytes, while Director 4+ promotes both the size and offset to 16-bit values.[engines/director/score.cpp (approx. lines 2074-2111)][engines/director/frame.h (approx. lines 49-56)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `frame+0x0000`..`frame+0x0001` | 2 bytes | `frameSize`; ScummVM subtracts 2 and loops until all channel records are consumed.[engines/director/score.cpp (approx. lines 2074-2095)] |

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `frame+0x0002`..`+0x0003` | 1 byte each | Director 2–3: channel payload byte-width and offset, both multiplied by 2 for word alignment before dispatching `readChannelD2()`.[engines/director/score.cpp (approx. lines 2092-2099)][engines/director/frame.cpp (approx. lines 154-173)] |
| `frame+0x0002`..`+0x0005` | 2 bytes each | Director 4+: 16-bit channel size and offset passed verbatim into `readChannelD4()`/`readChannelD5()`/`readChannelD6()`/`readChannelD7()`.[engines/director/score.cpp (approx. lines 2092-2104)][engines/director/frame.cpp (approx. lines 433-1680)] |


## Step 8 – Director 2 main channel layout (kMainChannelSizeD2 = 0x20)


The first 32 bytes encode score-level controls, palette animation, and tempo. ScummVM matches each offset inside `Frame::readMainChannelsD2()`.[engines/director/frame.cpp (approx. lines 176-299)][engines/director/frame.h (approx. lines 49-50)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x00` | 1 byte | `actionId` cast member selector for frame scripts.[engines/director/frame.cpp (approx. lines 189-191)] |
| `0x01` | 1 byte | `soundType1` (0x17 sampled cast, 0x16 MIDI menu id).[engines/director/frame.cpp (approx. lines 192-195)] |
| `0x02` | 1 byte | Transition flags; bit 7 toggles whole-stage fades and the low bits encode duration in quarter-seconds.[engines/director/frame.cpp (approx. lines 196-205)] |
| `0x03` | 1 byte | `transChunkSize` for wiped areas.[engines/director/frame.cpp (approx. lines 206-208)] |
| `0x04` | 1 byte | Frame tempo; cached when between 1 and 120.[engines/director/frame.cpp (approx. lines 209-213)] |
| `0x05` | 1 byte | Transition type enum.[engines/director/frame.cpp (approx. lines 214-216)] |
| `0x06`..`0x07` | 2 bytes | `sound1` cast member id.[engines/director/frame.cpp (approx. lines 217-219)] |
| `0x08`..`0x09` | 2 bytes | `sound2` cast member id.[engines/director/frame.cpp (approx. lines 220-222)] |
| `0x0A` | 1 byte | `soundType2` (sampled/MIDI).[engines/director/frame.cpp (approx. lines 223-225)] |
| `0x0B` | 1 byte | `skipFrameFlag` controlling tempo skipping.[engines/director/frame.cpp (approx. lines 226-228)] |
| `0x0C` | 1 byte | `blend` amount for transitions.[engines/director/frame.cpp (approx. lines 229-231)] |
| `0x0D` | 1 byte | Unknown byte logged when non-zero.[engines/director/frame.cpp (approx. lines 232-236)] |
| `0x0E`..`0x0F` | 2 bytes | Reserved words logged when populated.[engines/director/frame.cpp (approx. lines 237-241)] |
| `0x10`..`0x11` | 2 bytes | Palette cast id; negatives point to external cast libraries.[engines/director/frame.cpp (approx. lines 244-256)] |
| `0x12` | 1 byte | Palette first colour index (Mac signed to unsigned conversion).[engines/director/frame.cpp (approx. lines 257-260)] |
| `0x13` | 1 byte | Palette last colour index.[engines/director/frame.cpp (approx. lines 257-260)] |
| `0x14` | 1 byte | Palette flags controlling cycling, fades, auto reverse, and overtime.[engines/director/frame.cpp (approx. lines 262-270)] |
| `0x15` | 1 byte | Palette speed.[engines/director/frame.cpp (approx. lines 262-270)] |
| `0x16`..`0x17` | 2 bytes | Palette frame count for cycling loops.[engines/director/frame.cpp (approx. lines 271-274)] |
| `0x18`..`0x19` | 2 bytes | Palette cycle count.[engines/director/frame.cpp (approx. lines 275-277)] |
| `0x1A`..`0x1F` | 6 bytes | Reserved palette bytes logged if non-zero.[engines/director/frame.cpp (approx. lines 278-284)] |


## Step 9 – Director 4 main channel layout (kMainChannelSizeD4 = 0x28)


Director 4 extends the header with colour chips for the control channels, an explicit script id, and palette control bytes.[engines/director/frame.cpp (approx. lines 433-608)][engines/director/frame.h (approx. lines 52-53)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x00` | 1 byte | Unknown; warnings log unexpected values.[engines/director/frame.cpp (approx. lines 468-474)] |
| `0x01` | 1 byte | `soundType1`.[engines/director/frame.cpp (approx. lines 475-476)] |
| `0x02` | 1 byte | Transition flags/duration (same encoding as Director 2).[engines/director/frame.cpp (approx. lines 477-485)] |
| `0x03` | 1 byte | `transChunkSize`.[engines/director/frame.cpp (approx. lines 486-488)] |
| `0x04` | 1 byte | Frame tempo.[engines/director/frame.cpp (approx. lines 489-493)] |
| `0x05` | 1 byte | Transition type enum.[engines/director/frame.cpp (approx. lines 494-496)] |
| `0x06`..`0x07` | 2 bytes | `sound1` cast id.[engines/director/frame.cpp (approx. lines 497-499)] |
| `0x08`..`0x09` | 2 bytes | `sound2` cast id.[engines/director/frame.cpp (approx. lines 500-502)] |
| `0x0A` | 1 byte | `soundType2`.[engines/director/frame.cpp (approx. lines 503-505)] |
| `0x0B` | 1 byte | `skipFrameFlag`.[engines/director/frame.cpp (approx. lines 506-508)] |
| `0x0C` | 1 byte | `blend`.[engines/director/frame.cpp (approx. lines 509-511)] |
| `0x0D` | 1 byte | Tempo channel colour chip.[engines/director/frame.cpp (approx. lines 512-514)] |
| `0x0E` | 1 byte | Sound1 channel colour chip.[engines/director/frame.cpp (approx. lines 515-517)] |
| `0x0F` | 1 byte | Sound2 channel colour chip.[engines/director/frame.cpp (approx. lines 518-520)] |
| `0x10`..`0x11` | 2 bytes | Script action id cast member.[engines/director/frame.cpp (approx. lines 521-523)] |
| `0x12` | 1 byte | Script channel colour chip.[engines/director/frame.cpp (approx. lines 524-526)] |
| `0x13` | 1 byte | Transition channel colour chip.[engines/director/frame.cpp (approx. lines 527-529)] |
| `0x14`..`0x25` | 18 bytes | Palette block (cast id, cycling range, flags, timing, style, colour-code).[engines/director/frame.cpp (approx. lines 531-588)] |
| `0x26` | 1 byte | Logged unknown byte.[engines/director/frame.cpp (approx. lines 571-575)] |
| `0x27`..`0x29` | 3 bytes | Logged unknown words/dword.[engines/director/frame.cpp (approx. lines 576-584)] |
| `0x2A` | 1 byte | Palette colour code (score swatch index).[engines/director/frame.cpp (approx. lines 585-588)] |
| `0x2B` | 1 byte | Logged unknown byte.[engines/director/frame.cpp (approx. lines 589-592)] |


## Step 10 – Director 5 main channel layout (kMainChannelSizeD5 = 0x30)


Director 5 writes 16-bit cast library ids for every control channel and expands the palette record to include style, colour code, and padding bytes.[engines/director/frame.cpp (approx. lines 829-980)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x00`..`0x01` | 2 bytes | Script action cast library.[engines/director/frame.cpp (approx. lines 864-867)] |
| `0x02`..`0x03` | 2 bytes | Script action member id.[engines/director/frame.cpp (approx. lines 868-870)] |
| `0x04`..`0x05` | 2 bytes | Sound1 cast library.[engines/director/frame.cpp (approx. lines 871-874)] |
| `0x06`..`0x07` | 2 bytes | Sound1 member id.[engines/director/frame.cpp (approx. lines 875-876)] |
| `0x08`..`0x09` | 2 bytes | Sound2 cast library.[engines/director/frame.cpp (approx. lines 877-879)] |
| `0x0A`..`0x0B` | 2 bytes | Sound2 member id.[engines/director/frame.cpp (approx. lines 880-882)] |
| `0x0C`..`0x0D` | 2 bytes | Transition cast library.[engines/director/frame.cpp (approx. lines 883-885)] |
| `0x0E`..`0x0F` | 2 bytes | Transition member id.[engines/director/frame.cpp (approx. lines 886-888)] |
| `0x10` | 1 byte | Tempo channel colour chip.[engines/director/frame.cpp (approx. lines 889-891)] |
| `0x11` | 1 byte | Sound1 colour chip.[engines/director/frame.cpp (approx. lines 892-894)] |
| `0x12` | 1 byte | Sound2 colour chip.[engines/director/frame.cpp (approx. lines 895-897)] |
| `0x13` | 1 byte | Script colour chip.[engines/director/frame.cpp (approx. lines 898-900)] |
| `0x14` | 1 byte | Transition colour chip.[engines/director/frame.cpp (approx. lines 901-903)] |
| `0x15` | 1 byte | Tempo value with 1–120 cache.[engines/director/frame.cpp (approx. lines 904-908)] |
| `0x16`..`0x17` | 2 bytes | Alignment padding logged if non-zero.[engines/director/frame.cpp (approx. lines 909-913)] |
| `0x18`..`0x19` | 2 bytes | Palette cast library (signed).[engines/director/frame.cpp (approx. lines 916-918)] |
| `0x1A`..`0x1B` | 2 bytes | Palette member id (signed).[engines/director/frame.cpp (approx. lines 919-923)] |
| `0x1C` | 1 byte | Palette speed.[engines/director/frame.cpp (approx. lines 924-933)] |
| `0x1D` | 1 byte | Palette flags (cycling/over-time bits).[engines/director/frame.cpp (approx. lines 924-933)] |
| `0x1E` | 1 byte | Palette first colour (Mac-style offset).[engines/director/frame.cpp (approx. lines 934-937)] |
| `0x1F` | 1 byte | Palette last colour.[engines/director/frame.cpp (approx. lines 934-937)] |
| `0x20`..`0x21` | 2 bytes | Palette frame count.[engines/director/frame.cpp (approx. lines 938-940)] |
| `0x22`..`0x23` | 2 bytes | Palette cycle count.[engines/director/frame.cpp (approx. lines 941-943)] |
| `0x24` | 1 byte | Palette fade target.[engines/director/frame.cpp (approx. lines 944-946)] |
| `0x25` | 1 byte | Palette delay.[engines/director/frame.cpp (approx. lines 947-949)] |
| `0x26` | 1 byte | Palette style selector.[engines/director/frame.cpp (approx. lines 950-952)] |
| `0x27` | 1 byte | Palette colour code swatch id.[engines/director/frame.cpp (approx. lines 953-955)] |
| `0x28`..`0x2F` | 8 bytes | Logged padding; remains unimplemented.[engines/director/frame.cpp (approx. lines 956-965)] |

## Step 11 – Director 6 main channel layout (kMainChannelSizeD6 = 0x90)


Afterburner scores break the 144-byte header into six 24-byte blocks for script, tempo, transition, sound 2, sound 1, and palette channels. Each block carries cast ids, sprite detail pointers (with 16-bit low-word shadows for delta updates), colour chips, and padding.[engines/director/frame.cpp (approx. lines 1201-1412)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x00`..`0x01` | 2 bytes | Script action cast library.[engines/director/frame.cpp (approx. lines 1236-1239)] |
| `0x02`..`0x03` | 2 bytes | Script action member id.[engines/director/frame.cpp (approx. lines 1240-1242)] |
| `0x04`..`0x07` | 4 bytes | Script sprite-detail index stored as a 32-bit Afterburner pointer.[engines/director/frame.cpp (approx. lines 1243-1245)] |
| `0x06`..`0x07` | 2 bytes | Low-word shadow for the script detail index; partial updates may stream only these bytes.[engines/director/frame.cpp (approx. lines 1245-1248)] |
| `0x08` | 1 byte | Script channel colour chip.[engines/director/frame.cpp (approx. lines 1249-1251)] |
| `0x09`..`0x17` | 15 bytes | Alignment padding for the script block, logged when non-zero.[engines/director/frame.cpp (approx. lines 1252-1255)] |
| `0x18`..`0x1B` | 4 bytes | Tempo sprite-detail index pointer (32-bit).[engines/director/frame.cpp (approx. lines 1258-1261)] |
| `0x1A`..`0x1B` | 2 bytes | Low-word shadow for the tempo pointer to support 16-bit delta updates.[engines/director/frame.cpp (approx. lines 1261-1263)] |
| `0x1C`..`0x1D` | 2 bytes | Tempo flags (`tempoD6Flags`).[engines/director/frame.cpp (approx. lines 1264-1265)] |
| `0x1E` | 1 byte | Tempo value cached for puppet tempo when within 1–120 bpm.[engines/director/frame.cpp (approx. lines 1266-1271)] |
| `0x1F` | 1 byte | Tempo channel colour chip.[engines/director/frame.cpp (approx. lines 1271-1274)] |
| `0x20`..`0x2F` | 16 bytes | Alignment padding for the tempo block.[engines/director/frame.cpp (approx. lines 1275-1278)] |
| `0x30`..`0x31` | 2 bytes | Transition cast library id.[engines/director/frame.cpp (approx. lines 1282-1285)] |
| `0x32`..`0x33` | 2 bytes | Transition member id.[engines/director/frame.cpp (approx. lines 1285-1288)] |
| `0x34`..`0x37` | 4 bytes | Transition sprite-detail index (32-bit).[engines/director/frame.cpp (approx. lines 1288-1291)] |
| `0x36`..`0x37` | 2 bytes | Low-word shadow for the transition pointer.[engines/director/frame.cpp (approx. lines 1291-1293)] |
| `0x38` | 1 byte | Transition channel colour chip.[engines/director/frame.cpp (approx. lines 1294-1296)] |
| `0x39`..`0x47` | 15 bytes | Alignment padding for the transition block.[engines/director/frame.cpp (approx. lines 1297-1300)] |
| `0x48`..`0x49` | 2 bytes | Sound2 cast library id.[engines/director/frame.cpp (approx. lines 1303-1306)] |
| `0x4A`..`0x4B` | 2 bytes | Sound2 member id.[engines/director/frame.cpp (approx. lines 1306-1309)] |
| `0x4C`..`0x4F` | 4 bytes | Sound2 sprite-detail index (32-bit).[engines/director/frame.cpp (approx. lines 1309-1312)] |
| `0x4E`..`0x4F` | 2 bytes | Low-word shadow for the sound2 pointer.[engines/director/frame.cpp (approx. lines 1312-1314)] |
| `0x50` | 1 byte | Sound2 channel colour chip.[engines/director/frame.cpp (approx. lines 1315-1317)] |
| `0x51`..`0x5F` | 15 bytes | Alignment padding for the sound2 block.[engines/director/frame.cpp (approx. lines 1318-1320)] |
| `0x60`..`0x61` | 2 bytes | Sound1 cast library id.[engines/director/frame.cpp (approx. lines 1324-1327)] |
| `0x62`..`0x63` | 2 bytes | Sound1 member id.[engines/director/frame.cpp (approx. lines 1327-1330)] |
| `0x64`..`0x67` | 4 bytes | Sound1 sprite-detail index (32-bit).[engines/director/frame.cpp (approx. lines 1330-1333)] |
| `0x66`..`0x67` | 2 bytes | Low-word shadow for the sound1 pointer.[engines/director/frame.cpp (approx. lines 1333-1335)] |
| `0x68` | 1 byte | Sound1 channel colour chip.[engines/director/frame.cpp (approx. lines 1336-1338)] |
| `0x69`..`0x77` | 15 bytes | Alignment padding for the sound1 block.[engines/director/frame.cpp (approx. lines 1339-1342)] |
| `0x78`..`0x79` | 2 bytes | Palette cast library id (signed).[engines/director/frame.cpp (approx. lines 1345-1348)] |
| `0x7A`..`0x7B` | 2 bytes | Palette member id (signed).[engines/director/frame.cpp (approx. lines 1348-1351)] |
| `0x7C` | 1 byte | Palette speed.[engines/director/frame.cpp (approx. lines 1353-1362)] |
| `0x7D` | 1 byte | Palette flags (cycling, fade, overtime bits).[engines/director/frame.cpp (approx. lines 1353-1362)] |
| `0x7E` | 1 byte | Palette first colour (Mac-style offset).[engines/director/frame.cpp (approx. lines 1363-1366)] |
| `0x7F` | 1 byte | Palette last colour.[engines/director/frame.cpp (approx. lines 1363-1366)] |
| `0x80`..`0x81` | 2 bytes | Palette frame count.[engines/director/frame.cpp (approx. lines 1367-1369)] |
| `0x82`..`0x83` | 2 bytes | Palette cycle count.[engines/director/frame.cpp (approx. lines 1370-1372)] |
| `0x84` | 1 byte | Palette fade value.[engines/director/frame.cpp (approx. lines 1373-1375)] |
| `0x85` | 1 byte | Palette delay.[engines/director/frame.cpp (approx. lines 1376-1378)] |
| `0x86` | 1 byte | Palette style selector.[engines/director/frame.cpp (approx. lines 1379-1381)] |
| `0x87` | 1 byte | Palette colour code.[engines/director/frame.cpp (approx. lines 1382-1384)] |
| `0x88`..`0x8B` | 4 bytes | Palette sprite-detail index (32-bit).[engines/director/frame.cpp (approx. lines 1385-1387)] |
| `0x8A`..`0x8B` | 2 bytes | Low-word shadow for the palette detail pointer.[engines/director/frame.cpp (approx. lines 1387-1390)] |
| `0x8C`..`0x8F` | 4 bytes | Alignment padding for the palette block.[engines/director/frame.cpp (approx. lines 1391-1394)] |


## Step 12 – Director 7 main channel layout (kMainChannelSizeD7 = 0x120)


Director 7 doubles each 24-byte block to 48 bytes, extending the padding, keeping the low-word pointer shadows, and reserving space for additional alignment while keeping the same field order.[engines/director/frame.cpp (approx. lines 1659-1868)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x00`..`0x01` | 2 bytes | Script action cast library.[engines/director/frame.cpp (approx. lines 1695-1699)] |
| `0x02`..`0x03` | 2 bytes | Script action member id.[engines/director/frame.cpp (approx. lines 1698-1700)] |
| `0x04`..`0x07` | 4 bytes | Script sprite-detail index (32-bit).[engines/director/frame.cpp (approx. lines 1701-1704)] |
| `0x06`..`0x07` | 2 bytes | Low-word shadow for the script detail pointer.[engines/director/frame.cpp (approx. lines 1704-1706)] |
| `0x08` | 1 byte | Script channel colour chip.[engines/director/frame.cpp (approx. lines 1707-1709)] |
| `0x09`..`0x2F` | 39 bytes | Alignment padding for the script block; logged when non-zero.[engines/director/frame.cpp (approx. lines 1710-1713)] |
| `0x30`..`0x33` | 4 bytes | Tempo sprite-detail index (32-bit).[engines/director/frame.cpp (approx. lines 1715-1718)] |
| `0x32`..`0x33` | 2 bytes | Low-word shadow for the tempo pointer.[engines/director/frame.cpp (approx. lines 1718-1720)] |
| `0x34`..`0x35` | 2 bytes | Tempo flags (`tempoD6Flags`).[engines/director/frame.cpp (approx. lines 1719-1722)] |
| `0x36` | 1 byte | Tempo value cached when 1–120 bpm.[engines/director/frame.cpp (approx. lines 1721-1726)] |
| `0x37` | 1 byte | Tempo channel colour chip.[engines/director/frame.cpp (approx. lines 1726-1729)] |
| `0x38`..`0x5F` | 40 bytes | Alignment padding for the tempo block.[engines/director/frame.cpp (approx. lines 1729-1736)] |
| `0x60`..`0x61` | 2 bytes | Transition cast library id.[engines/director/frame.cpp (approx. lines 1738-1742)] |
| `0x62`..`0x63` | 2 bytes | Transition member id.[engines/director/frame.cpp (approx. lines 1742-1745)] |
| `0x64`..`0x67` | 4 bytes | Transition sprite-detail index (32-bit).[engines/director/frame.cpp (approx. lines 1745-1748)] |
| `0x66`..`0x67` | 2 bytes | Low-word shadow for the transition pointer.[engines/director/frame.cpp (approx. lines 1748-1750)] |
| `0x68` | 1 byte | Transition channel colour chip.[engines/director/frame.cpp (approx. lines 1750-1753)] |
| `0x69`..`0x8F` | 39 bytes | Alignment padding for the transition block.[engines/director/frame.cpp (approx. lines 1753-1757)] |
| `0x90`..`0x91` | 2 bytes | Sound2 cast library id.[engines/director/frame.cpp (approx. lines 1759-1763)] |
| `0x92`..`0x93` | 2 bytes | Sound2 member id.[engines/director/frame.cpp (approx. lines 1763-1766)] |
| `0x94`..`0x97` | 4 bytes | Sound2 sprite-detail index (32-bit).[engines/director/frame.cpp (approx. lines 1766-1770)] |
| `0x96`..`0x97` | 2 bytes | Low-word shadow for the sound2 pointer.[engines/director/frame.cpp (approx. lines 1770-1772)] |
| `0x98` | 1 byte | Sound2 channel colour chip.[engines/director/frame.cpp (approx. lines 1772-1775)] |
| `0x99`..`0xBF` | 39 bytes | Alignment padding for the sound2 block.[engines/director/frame.cpp (approx. lines 1775-1777)] |
| `0xC0`..`0xC1` | 2 bytes | Sound1 cast library id.[engines/director/frame.cpp (approx. lines 1780-1784)] |
| `0xC2`..`0xC3` | 2 bytes | Sound1 member id.[engines/director/frame.cpp (approx. lines 1784-1787)] |
| `0xC4`..`0xC7` | 4 bytes | Sound1 sprite-detail index (32-bit).[engines/director/frame.cpp (approx. lines 1787-1791)] |
| `0xC6`..`0xC7` | 2 bytes | Low-word shadow for the sound1 pointer.[engines/director/frame.cpp (approx. lines 1791-1793)] |
| `0xC8` | 1 byte | Sound1 channel colour chip.[engines/director/frame.cpp (approx. lines 1793-1796)] |
| `0xC9`..`0xEF` | 39 bytes | Alignment padding for the sound1 block.[engines/director/frame.cpp (approx. lines 1796-1799)] |
| `0xF0`..`0xF1` | 2 bytes | Palette cast library id (signed).[engines/director/frame.cpp (approx. lines 1802-1806)] |
| `0xF2`..`0xF3` | 2 bytes | Palette member id (signed).[engines/director/frame.cpp (approx. lines 1806-1809)] |
| `0xF4` | 1 byte | Palette speed.[engines/director/frame.cpp (approx. lines 1811-1815)] |
| `0xF5` | 1 byte | Palette flags (cycling/fade bits).[engines/director/frame.cpp (approx. lines 1811-1815)] |
| `0xF6` | 1 byte | Palette first colour (Mac offset).[engines/director/frame.cpp (approx. lines 1816-1819)] |
| `0xF7` | 1 byte | Palette last colour.[engines/director/frame.cpp (approx. lines 1816-1819)] |
| `0xF8`..`0xF9` | 2 bytes | Palette frame count.[engines/director/frame.cpp (approx. lines 1820-1824)] |
| `0xFA`..`0xFB` | 2 bytes | Palette cycle count.[engines/director/frame.cpp (approx. lines 1824-1827)] |
| `0xFC` | 1 byte | Palette fade value.[engines/director/frame.cpp (approx. lines 1828-1832)] |
| `0xFD` | 1 byte | Palette delay.[engines/director/frame.cpp (approx. lines 1832-1835)] |
| `0xFE` | 1 byte | Palette style selector.[engines/director/frame.cpp (approx. lines 1835-1838)] |
| `0xFF` | 1 byte | Palette colour code.[engines/director/frame.cpp (approx. lines 1838-1841)] |
| `0x100`..`0x103` | 4 bytes | Palette sprite-detail index (32-bit).[engines/director/frame.cpp (approx. lines 1841-1844)] |
| `0x102`..`0x103` | 2 bytes | Low-word shadow for the palette detail pointer.[engines/director/frame.cpp (approx. lines 1844-1847)] |
| `0x104`..`0x11F` | 28 bytes | Alignment padding for the palette block.[engines/director/frame.cpp (approx. lines 1848-1851)] |


## Step 13 – Sprite detail lookups (Director 6+)


Once a frame is decoded, `loadFrameSpriteDetails()` resolves `_spriteListIdx` values through the detail offsets captured earlier. Each sprite fetches `SpriteInfo` (record `id`), optional behaviours, and human-readable names.[engines/director/score.cpp (approx. lines 1897-2007)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `detailIndex*4`..`+3` | 4 bytes | Pointer into `_spriteDetailOffsets` for sprite info (`i%3==0`), behaviours (`i%3==1`), or names (`i%3==2`).[engines/director/score.cpp (approx. lines 1778-2003)] |


## Step 14 – `VWLB` label stream


`Score::loadLabels()` parses frame labels from the `VWLB` chunk using a packed offset table followed by CR-delimited UTF-8 strings.[engines/director/score.cpp (approx. lines 2162-2215)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x0000`..`0x0001` | 2 bytes | `countMinusOne`; add one to obtain the number of label entries.[engines/director/score.cpp (approx. lines 2168-2176)] |
| `0x0002`..`0x0003` | 2 bytes | Base offset (`offset = count*4 + 2`) applied to subsequent pointers.[engines/director/score.cpp (approx. lines 2169-2174)] |
| `0x0004`..`0x0005` | 2 bytes | First label frame number.[engines/director/score.cpp (approx. lines 2172-2174)] |
| `0x0006`..`0x0007` | 2 bytes | First string offset relative to the base.[engines/director/score.cpp (approx. lines 2172-2174)] |
| `0x0008`.. | `count*4` bytes | Repeated frame numbers and offsets for subsequent labels.[engines/director/score.cpp (approx. lines 2175-2209)] |
| `strings` | variable | CR-terminated label text followed by comment text; converted from Director encoding to UTF-8.[engines/director/score.cpp (approx. lines 2180-2209)] |


## Step 15 – `VWAC` action stream


Frame actions are stored as packed strings with an id/sub-id table. `Score::loadActions()` walks the offset pairs, decodes each script, and hands the text to the Lingo compiler.[engines/director/score.cpp (approx. lines 2222-2265)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x0000`..`0x0001` | 2 bytes | `countMinusOne`; plus one yields the number of action entries.[engines/director/score.cpp (approx. lines 2225-2234)] |
| `0x0002`..`0x0003` | 2 bytes | Base offset (`count*4 + 2`) used to reference the string block.[engines/director/score.cpp (approx. lines 2225-2233)] |
| `0x0004` | 1 byte | Initial action id.[engines/director/score.cpp (approx. lines 2228-2233)] |
| `0x0005` | 1 byte | Initial sub-id (used for printing and continuity).[engines/director/score.cpp (approx. lines 2229-2233)] |
| `0x0006`..`0x0007` | 2 bytes | First script offset relative to the base.[engines/director/score.cpp (approx. lines 2230-2233)] |
| `0x0008`.. | `count*4` bytes | Repeated next-id, next-sub-id, and next-offset tuples for each action entry.[engines/director/score.cpp (approx. lines 2233-2253)] |
| `strings` | variable | Action script text retrieved with `readString()` and decoded through the cast’s string map.[engines/director/score.cpp (approx. lines 2239-2263)] |

