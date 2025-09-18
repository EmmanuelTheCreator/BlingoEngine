# Reading Archives

This note documents how ScummVM's Director engine finds and opens standalone `.dir` movies. Each section tracks a major step in the loader and highlights the exact bytes ScummVM reads to distinguish container variants and Director versions.

## Step 1 – Picking the archive implementation

`DirectorEngine::createArchive()` picks a concrete archive reader purely from the Director version and target platform: classic titles (version < 4) get a `MacArchive` on Mac and a `RIFFArchive` on Windows, while Director 4 and newer always use the RIFX reader.[engines/director/resource.cpp (approx. lines 46-55)]

```cpp
if (getVersion() < 400) {
    if (getPlatform() != Common::kPlatformWindows)
        return new MacArchive();
    return new RIFFArchive();
}
return new RIFXArchive();
```

`openArchive()` reuses already opened archives and otherwise delegates to the platform loaders. Only after `loadEXE()`/`loadMac()` fail does it fall back to reading the `.dir` file directly with the factory-created archive.[engines/director/resource.cpp (approx. lines 244-273)]

## Step 2 – Windows executables and projector headers

`loadEXE()` inspects the first four bytes of the stream. Raw `.dir` exports carry `RIFX`/`XFIR`, plain RIFF bundles use `RIFF`/`FFIR`, and older projectors embed the movie after Windows resources. The loader probes for `PJ` projector signatures—`PJ93`, `PJ95`, `PJ00`/`PJ01`—to locate the RIFX offset that follows the 32-bit tag and metadata fields before jumping into the archive payload.[engines/director/resource.cpp (approx. lines 280-520)] When the opener encounters `PJ93`/`PJ95` headers, it reads the little-endian offset immediately after the tag to find the embedded RIFX movie, ensuring the correct byte position for Director 4 and 5 projectors.[engines/director/resource.cpp (approx. lines 404-470)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `52 49 46 58` (`RIFX`) / `58 46 49 52` (`XFIR`) | 4 bytes | Identifies raw `.dir` exports that can be passed straight to `loadEXERIFX()` without projector metadata.[engines/director/resource.cpp (approx. lines 312-330)] |
| `52 49 46 46` (`RIFF`) / `46 46 49 52` (`FFIR`) | 4 bytes | Flags plain RIFF bundles that should be opened with `RIFFArchive` instead of the RIFX loader.[engines/director/resource.cpp (approx. lines 332-348)] |
| `50 4A 39 33` (`PJ93`), `50 4A 39 35` (`PJ95`), `50 4A 30 30` (`PJ00`), `50 4A 30 31` (`PJ01`) | 4 bytes | Windows projector signatures that gate which loader routine runs for Director 4, 5, and 7 executables.[engines/director/resource.cpp (approx. lines 404-556)] |
| `<offset>` | 4 bytes (little-endian) | Immediately follows each `PJ**` tag and tells the loader where the embedded RIFX movie starts within the executable.[engines/director/resource.cpp (approx. lines 414-546)] |

## Step 3 – Mac binaries and data-fork detection

`loadMac()` first tries to open the resource fork for pre-Director 4 titles. For Director 4+, it switches to the data fork and peeks at the first four bytes: PowerPC projectors store a `PJxx` tag (`PJ93`, `PJ95`, `PJ00`) followed by a big-endian offset to the RIFX data, while 68k exports present the RIFX header directly. This byte check determines whether the loader should skip ahead or start at offset zero.[engines/director/resource.cpp (approx. lines 560-612)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `50 4A 39 33` (`PJ93`), `50 4A 39 35` (`PJ95`), `50 4A 30 30` (`PJ00`) | 4 bytes | Mac PPC projector signatures that signal the presence of a RIFX chunk later in the data fork.[engines/director/resource.cpp (approx. lines 576-592)] |
| `<offset>` | 4 bytes (big-endian) | Big-endian seek value pointing at the embedded RIFX stream inside a PowerPC projector.[engines/director/resource.cpp (approx. lines 580-598)] |
| `52 49 46 58` (`RIFX`) / `58 46 49 52` (`XFIR`) | 4 bytes | Indicates that a 68k binary already starts with the movie header so the archive begins at offset zero.[engines/director/resource.cpp (approx. lines 572-604)] |

## Step 4 – Core RIFX header validation

`RIFXArchive::openStream()` validates that the stream really begins with `RIFX` or byte-swapped `XFIR`, accounting for MacBinary wrappers by probing the resource fork before reading the data fork. It also records the endianness based on the tag it read. Immediately after the eight-byte header it reads the archive's four-character type code (`MV93`, `MC95`, `APPL`, `FGDM`, `FGDC`) and uses it to select the map parser.[engines/director/archive.cpp (approx. lines 608-676)]

```cpp
_rifxType = endianStream.readUint32();
switch (_rifxType) {
case MKTAG('M','V','9','3'):
case MKTAG('M','C','9','5'):
case MKTAG('A','P','P','L'):
    readMapSuccess = readMemoryMap(...);
    break;
case MKTAG('F','G','D','M'):
case MKTAG('F','G','D','C'):
    readMapSuccess = readAfterburnerMap(...);
    break;
}
```

The archive reader also marks the `RIFX` chunk itself as accessed and optionally dumps the raw bytes when debugging is enabled.[engines/director/archive.cpp (approx. lines 640-718)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `52 49 46 58` (`RIFX`) / `58 46 49 52` (`XFIR`) | 4 bytes | Container signature checked to select big-endian (`RIFX`) or little-endian (`XFIR`) parsing.[engines/director/archive.cpp (approx. lines 616-640)] |
| `<chunk size>` | 4 bytes | Unsigned 32-bit value giving the byte size of the top-level movie chunk (plus header) used to dump embedded movies.[engines/director/archive.cpp (approx. lines 642-660)] |
| `4D 56 39 33` (`MV93`), `4D 43 39 35` (`MC95`), `41 50 50 4C` (`APPL`), `46 47 44 4D` (`FGDM`), `46 47 44 43` (`FGDC`) | 4 bytes | Archive subtype that dictates whether `readMemoryMap()` or `readAfterburnerMap()` should parse the resource tables.[engines/director/archive.cpp (approx. lines 666-704)] |

## Step 5 – Memory-map archives (`MV93`, `MC95`, `APPL`)

`readMemoryMap()` expects the `imap` and `mmap` control blocks. After reading the `imap` length and map version, it records a 32-bit `version` field whose values correlate with Director releases—`0x0` for Director 4, `0x4c1` for Director 5, `0x4c7` for Director 6, `0x708` for Director 8.5, and `0x742` for Director 10. The loader uses the offset stored in `imap` to jump to the `mmap` table, where each entry packs the resource type tag, data size, file offset (patched when dumping embedded movies), 16-bit flags, an unknown 16-bit field, and a link for free-list housekeeping.[engines/director/archive.cpp (approx. lines 804-868)] Every entry read here becomes a `Resource` with the offset adjusted by any MacBinary data-fork prefix before later reads consume the 8-byte chunk header stored in the file.[engines/director/archive.cpp (approx. lines 809-868)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `69 6D 61 70` (`imap`) | 4 bytes | Memory map prologue tag that must be present before any table data.[engines/director/archive.cpp (approx. lines 804-814)] |
| `<imap length>` | 4 bytes | Size of the `imap` block used to validate dumps.[engines/director/archive.cpp (approx. lines 812-820)] |
| `<map version>` | 4 bytes | Map format version (usually 0 or 1) stored for informational logging.[engines/director/archive.cpp (approx. lines 812-820)] |
| `<mmap offset>` | 4 bytes | Offset of the `mmap` list, patched when dumping embedded movies to keep offsets valid.[engines/director/archive.cpp (approx. lines 816-828)] |
| `<archive version>` | 4 bytes | Director release marker (`0x0`, `0x4c1`, `0x4c7`, `0x708`, `0x742`).[engines/director/archive.cpp (approx. lines 820-826)] |

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `6D 6D 61 70` (`mmap`) | 4 bytes | Table tag that starts the resource entry block.[engines/director/archive.cpp (approx. lines 828-840)] |
| `<mmap length>` | 4 bytes | Big-endian size of the `mmap` chunk; stored for debugging but not otherwise used.[engines/director/archive.cpp (approx. lines 836-842)] |
| `<header size>` | 2 bytes | `_mmapHeaderSize`; describes how many bytes precede the first entry.[engines/director/archive.cpp (approx. lines 836-844)] |
| `<entry size>` | 2 bytes | `_mmapEntrySize`; width of each table row before the per-entry payload begins.[engines/director/archive.cpp (approx. lines 836-844)] |
| `<total entries>` | 4 bytes | `_totalCount`; includes both populated and free-map slots.[engines/director/archive.cpp (approx. lines 836-848)] |
| `<filled entries>` | 4 bytes | `_resCount`; number of resource rows to decode for this archive.[engines/director/archive.cpp (approx. lines 836-848)] |
| `FF FF FF FF FF FF FF FF` padding | 8 bytes | Unused bytes (all `0xFF`) that Director left between the header and freelist pointer.[engines/director/archive.cpp (approx. lines 842-846)] |
| `<first free resource id>` | 4 bytes | Index of the first unused row (`-1` when no free slots remain).[engines/director/archive.cpp (approx. lines 842-848)] |
| `<tag>` | 4 bytes per entry | Resource type, stored as a four-character code.[engines/director/archive.cpp (approx. lines 848-864)] |
| `<size>` | 4 bytes per entry | Chunk payload size excluding the 8-byte RIFX sub-header.[engines/director/archive.cpp (approx. lines 848-864)] |
| `<offset>` | 4 bytes per entry | File position of the chunk, adjusted by `moreOffset` for MacBinary wrappers.[engines/director/archive.cpp (approx. lines 850-864)] |
| `<flags>` | 2 bytes per entry | Attribute bitfield copied into the in-memory `Resource`.[engines/director/archive.cpp (approx. lines 854-866)] |
| `<unk1>` | 2 bytes per entry | Unknown field retained for parity with Director's table layout.[engines/director/archive.cpp (approx. lines 854-866)] |
| `<next free>` | 4 bytes per entry | Index used to chain free slots inside the archive map.[engines/director/archive.cpp (approx. lines 856-868)] |

## Step 6 – Afterburner archives (`FGDM`, `FGDC`)

Afterburner movies use a compressed metadata stream. `readAfterburnerMap()` first checks for `Fver` to read the Afterburner version (via a Shockwave-style variable-length integer), skips over `Fcdr` compression descriptors, and then consumes the `ABMP` block: the code reads a variable-length encoded length, compression kind, and uncompressed size before inflating the chunk with `readZlibData()`.[engines/director/archive.cpp (approx. lines 874-923)] The decompressed `ABMP` stream begins with two more varints and the resource count; each resource entry then stores (all via varints) the resource ID, offset (adjusted if non-negative), compressed length, uncompressed length, compression type, followed by the four-byte tag. These bytes fully describe where to seek and how to decompress each resource at runtime.[engines/director/archive.cpp (approx. lines 924-968)] The helper that decodes these varints shifts in seven data bits per byte until a byte with the high bit clear appears.[engines/director/util.cpp (approx. lines 1248-1268)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `46 76 65 72` (`Fver`) | 4 bytes | Afterburner version tag consumed before reading two varints.[engines/director/archive.cpp (approx. lines 884-904)] |
| `<varint length>` | 1–5 bytes | Length of the `Fver` payload that bounds the version read.[engines/director/archive.cpp (approx. lines 892-906)] |
| `<varint version>` | 1–5 bytes | Encoded Afterburner build number logged for debugging.[engines/director/archive.cpp (approx. lines 896-910)] |
| `46 63 64 72` (`Fcdr`) | 4 bytes | Compression descriptor tag skipped after reading its varint length.[engines/director/archive.cpp (approx. lines 908-918)] |
| `<varint descriptor length>` | 1–5 bytes | `_fcdrLength`; number of descriptor bytes skipped before the map begins.[engines/director/archive.cpp (approx. lines 912-918)] |
| `41 42 4D 50` (`ABMP`) | 4 bytes | Afterburner map header preceding length, compression, and uncompressed size varints.[engines/director/archive.cpp (approx. lines 918-934)] |
| `<varint map length>` | 1–5 bytes | Encoded size of the compressed `ABMP` payload that feeds `readZlibData()`.[engines/director/archive.cpp (approx. lines 922-936)] |
| `<varint compression>` | 1–5 bytes | Compression algorithm selector stored before inflating `ABMP`.[engines/director/archive.cpp (approx. lines 926-940)] |
| `<varint uncompressed size>` | 1–5 bytes | Expected length of the inflated metadata, checked after decompression.[engines/director/archive.cpp (approx. lines 926-946)] |

| Field | Length | Explanation |
| --- | --- | --- |
| `<varint unk1>` | 1–5 bytes | First field inside the decompressed `ABMP`; ScummVM logs it as `abmpUnk1`.[engines/director/archive.cpp (approx. lines 940-952)] |
| `<varint unk2>` | 1–5 bytes | Second control value logged as `abmpUnk2`; meaning currently unknown.[engines/director/archive.cpp (approx. lines 940-952)] |
| `<varint resource count>` | 1–5 bytes | Number of Afterburner resource records that follow in the `ABMP` stream.[engines/director/archive.cpp (approx. lines 940-952)] |

| Field | Length | Explanation |
| --- | --- | --- |
| `<varint resource id>` | 1–5 bytes | Index used to register the resource inside `_types`.[engines/director/archive.cpp (approx. lines 944-964)] |
| `<varint offset>` | 1–5 bytes | Relative offset rebased by `moreOffset` when non-negative; `-1` marks Initial Load Segment entries.[engines/director/archive.cpp (approx. lines 946-964)] |
| `<varint compressed size>` | 1–5 bytes | Length of the stored payload used when fetching data later.[engines/director/archive.cpp (approx. lines 948-964)] |
| `<varint uncompressed size>` | 1–5 bytes | Target size to verify when inflating the resource chunk.[engines/director/archive.cpp (approx. lines 948-966)] |
| `<varint compression type>` | 1–5 bytes | Compression scheme applied to the resource (zlib or stored).[engines/director/archive.cpp (approx. lines 950-966)] |
| `<tag>` | 4 bytes | Four-character resource type identifier appended after the varints.[engines/director/archive.cpp (approx. lines 950-968)] |

## Step 7 – Initial load segments and key tables

After parsing the ABMP metadata, the loader expects an `FGEI` chunk describing the Initial Load Segment. Its header contributes a varint control value, and the body is inflated into `_ilsData` so resources flagged with an offset of `-1` can be served from memory instead of the main stream. Each sub-entry inside the ILS is again keyed by a varint resource ID before the raw bytes are copied aside.[engines/director/archive.cpp (approx. lines 968-1012)] Once the maps are ready, `readKeyTable()` insists that a `KEY*` resource exists, reads 12-byte table entries, and associates each child resource ID with its parent ID and tag—linking cast members, movies (hardcoded parent 1024), and library casts for later lookups.[engines/director/archive.cpp (approx. lines 1012-1064)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `46 47 45 49` (`FGEI`) | 4 bytes | Initial Load Segment header tag that must appear after the ABMP block.[engines/director/archive.cpp (approx. lines 972-984)] |
| `<varint control>` | 1–5 bytes | Control value logged before decompressing the ILS payload.[engines/director/archive.cpp (approx. lines 976-988)] |
| `<varint resource id>` | 1–5 bytes | ID read for each entry stored entirely in `_ilsData`.[engines/director/archive.cpp (approx. lines 988-1006)] |
| `<raw resource bytes>` | `res.size` bytes | Exact chunk data copied from the decompressed stream for offset `-1` resources.[engines/director/archive.cpp (approx. lines 992-1008)] |

| Field | Length | Explanation |
| --- | --- | --- |
| `<entry size>` | 2 bytes | First `KEY*` field confirming the 12-byte row layout Director uses.[engines/director/archive.cpp (approx. lines 1008-1046)] |
| `<entry count>` | 4 bytes | Number of populated rows that will be processed.[engines/director/archive.cpp (approx. lines 1010-1048)] |
| `<child resource id>` | 4 bytes | Resource index to attach to the parent.[engines/director/archive.cpp (approx. lines 1048-1060)] |
| `<parent resource id>` | 4 bytes | Owning cast, movie, or library index recorded for lookups.[engines/director/archive.cpp (approx. lines 1050-1062)] |
| `<child tag>` | 4 bytes | Resource type so the relationship can be grouped by tag when querying.[engines/director/archive.cpp (approx. lines 1052-1064)] |

Together these steps explain how ScummVM locates `.dir` content, interprets the version-specific metadata it reads, and exposes the movie's resources through a uniform archive interface.
