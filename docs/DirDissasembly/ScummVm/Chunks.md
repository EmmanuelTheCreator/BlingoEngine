# Chunk Handling

This note describes how ScummVM's Director engine interprets individual RIFX chunks after the archive header is validated. Each step highlights the byte fields the loader consumes and how those fields vary across Director versions.

## Step 1 – Map entries describe chunk bytes

`readMemoryMap()` confirms the `imap` signature, captures the block length, map version, stored archive version (`0x0` for Director 4, `0x4c1` for Director 5, `0x4c7` for Director 6, `0x708` for Director 8.5, `0x742` for Director 10), and the patched offset to the `mmap` table before seeking there.[engines/director/archive.cpp (approx. lines 804-828)] The `mmap` header records the chunk length, header size, entry width, total and filled entry counts, an all-`0xFF` spacer, and the freelist head so later dumps can mirror Director's layout.[engines/director/archive.cpp (approx. lines 828-848)] Each `mmap` row then contributes a four-character tag, the stored chunk length, a 32-bit offset (adjusted by `moreOffset` to include projector padding), and two 16-bit fields before linking to the next free slot.[engines/director/archive.cpp (approx. lines 848-868)] These bytes populate the in-memory `Resource` with its index, absolute offset, and size so later reads can locate the chunk payload without reinterpreting the map.

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `69 6D 61 70` (`imap`) | 4 bytes | Verifies that the memory map header is present before version data is read.[engines/director/archive.cpp (approx. lines 804-812)] |
| `<imap length>` | 4 bytes | Size of the `imap` block; stored for parity with Director dumps.[engines/director/archive.cpp (approx. lines 812-820)] |
| `<map version>` | 4 bytes | `_mapversion`; observed values `0` and `1`.[engines/director/archive.cpp (approx. lines 812-820)] |
| `<mmap offset>` | 4 bytes | Location of the `mmap` table before MacBinary padding is applied.[engines/director/archive.cpp (approx. lines 816-828)] |
| `<archive version>` | 4 bytes | Director release marker copied into `_version` for later conditionals.[engines/director/archive.cpp (approx. lines 820-826)] |
| `6D 6D 61 70` (`mmap`) | 4 bytes | Resource table signature that must follow the `imap` header.[engines/director/archive.cpp (approx. lines 828-836)] |
| `<mmap length>` | 4 bytes | Stored `mmap` chunk size used when patching offsets for dumps.[engines/director/archive.cpp (approx. lines 836-842)] |
| `<header size>` | 2 bytes | `_mmapHeaderSize`; describes bytes that precede the first entry.[engines/director/archive.cpp (approx. lines 836-844)] |
| `<entry size>` | 2 bytes | `_mmapEntrySize`; width of each resource row.[engines/director/archive.cpp (approx. lines 836-844)] |
| `<total entries>` | 4 bytes | `_totalCount`; includes populated rows and freelist slots.[engines/director/archive.cpp (approx. lines 836-848)] |
| `<filled entries>` | 4 bytes | `_resCount`; number of active resources.[engines/director/archive.cpp (approx. lines 836-848)] |
| `FF FF FF FF FF FF FF FF` padding | 8 bytes | All-`0xFF` filler preceding the freelist pointer.[engines/director/archive.cpp (approx. lines 842-846)] |
| `<first free resource id>` | 4 bytes | Index of the first unused row (`-1` when none).[engines/director/archive.cpp (approx. lines 842-848)] |
| `<tag>` | 4 bytes per entry | Four-character resource type stored for each chunk.[engines/director/archive.cpp (approx. lines 848-858)] |
| `<size>` | 4 bytes per entry | Chunk payload length excluding the 8-byte RIFX subheader.[engines/director/archive.cpp (approx. lines 848-862)] |
| `<offset>` | 4 bytes per entry | File offset patched by `moreOffset` so the loader can seek directly to the chunk.[engines/director/archive.cpp (approx. lines 850-864)] |
| `<flags>` | 2 bytes per entry | Attribute bits that mirror Director's resource map flags.[engines/director/archive.cpp (approx. lines 854-866)] |
| `<unk1>` | 2 bytes per entry | Unknown field preserved from the original table layout.[engines/director/archive.cpp (approx. lines 854-866)] |
| `<next free>` | 4 bytes per entry | Index used to maintain Director's freelist for unused slots.[engines/director/archive.cpp (approx. lines 856-868)] |

## Step 2 – Afterburner metadata and compressed entries


Afterburner movies replace the classic `mmap` table with an `ABMP` stream: `readAfterburnerMap()` first validates the `Fver` header, recording its varint length and the Afterburner build number, then consumes the `Fcdr` descriptor before dropping into the compressed `ABMP` metadata.[engines/director/archive.cpp (approx. lines 884-918)] The `ABMP` block itself contributes Shockwave-style varints for the compressed map length, compression algorithm, expected uncompressed size, two control values, and the resource count before iterating each resource's ID, relative offset, compressed length, uncompressed length, compression type, and trailing four-character tag; ScummVM also warns when the inflated byte count differs from the stored length.[engines/director/archive.cpp (approx. lines 918-968)] Positive offsets are rebased with `moreOffset`, while negative offsets (`-1`) denote Initial Load Segment (ILS) resources that are copied into memory. Once the `ABMP` metadata is decoded, the loader inflates the `FGEI` chunk into `_ilsData`, reads its control varint, and walks a sequence of varint resource IDs paired with raw byte blobs so embedded resources can be served directly from RAM.[engines/director/archive.cpp (approx. lines 968-1008)]


| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `46 76 65 72` (`Fver`) | 4 bytes | File-version tag read before the map; bounds the next varint block.[engines/director/archive.cpp (approx. lines 884-904)] |
| `<varint length>` | 1–5 bytes | `_fverLength`; expected byte count for the version payload.[engines/director/archive.cpp (approx. lines 892-906)] |
| `<varint version>` | 1–5 bytes | `_afterBurnerVersion`; logged Afterburner build number.[engines/director/archive.cpp (approx. lines 896-910)] |
| `46 63 64 72` (`Fcdr`) | 4 bytes | Compression-descriptor tag skipped after its length varint.[engines/director/archive.cpp (approx. lines 908-918)] |
| `<varint descriptor length>` | 1–5 bytes | `_fcdrLength`; byte count discarded before reading `ABMP`.[engines/director/archive.cpp (approx. lines 912-918)] |
| `41 42 4D 50` (`ABMP`) | 4 bytes | Afterburner metadata header preceding the varint-encoded resource table.[engines/director/archive.cpp (approx. lines 918-934)] |
| `<varint map length>` | 1–5 bytes | `_abmpLength`; compressed byte count passed to `readZlibData()`.[engines/director/archive.cpp (approx. lines 922-936)] |
| `<varint compression>` | 1–5 bytes | `_abmpCompressionType`; algorithm used for the `ABMP` payload.[engines/director/archive.cpp (approx. lines 926-940)] |
| `<varint uncompressed size>` | 1–5 bytes | `_abmpUncompLength`; expected size of the inflated metadata.[engines/director/archive.cpp (approx. lines 926-946)] |
| `<varint unk1>` | 1–5 bytes | First control varint inside the decompressed `ABMP` stream (logged as `abmpUnk1`).[engines/director/archive.cpp (approx. lines 940-952)] |
| `<varint unk2>` | 1–5 bytes | Second control varint (`abmpUnk2`) read before the resource count.[engines/director/archive.cpp (approx. lines 940-952)] |
| `<varint resource count>` | 1–5 bytes | Number of resource rows encoded in the `ABMP` metadata.[engines/director/archive.cpp (approx. lines 940-952)] |
| `<varint resource id>` | 1–5 bytes | Resource index that determines where the chunk metadata is stored.[engines/director/archive.cpp (approx. lines 944-964)] |
| `<varint offset>` | 1–5 bytes | Relative seek position adjusted by `moreOffset`; `0x7F...` encodings yield `-1` for ILS chunks.[engines/director/archive.cpp (approx. lines 946-964)] |
| `<varint compressed size>` | 1–5 bytes | Length of the compressed payload needed for later decompression.[engines/director/archive.cpp (approx. lines 948-964)] |
| `<varint uncompressed size>` | 1–5 bytes | Target size used to validate the inflated data.[engines/director/archive.cpp (approx. lines 948-966)] |
| `<varint compression type>` | 1–5 bytes | Algorithm selector (zlib vs. stored) saved for chunk extraction.[engines/director/archive.cpp (approx. lines 950-966)] |
| `<tag>` | 4 bytes | Four-character resource code appended after the varints.[engines/director/archive.cpp (approx. lines 950-968)] |
| `46 47 45 49` (`FGEI`) | 4 bytes | Initial Load Segment header that precedes varint resource IDs and raw bytes in `_ilsData`.[engines/director/archive.cpp (approx. lines 972-1006)] |
| `<varint control>` | 1–5 bytes | `ilsUnk1`; logged before inflating the ILS payload.[engines/director/archive.cpp (approx. lines 972-982)] |
| `<varint resource id>` | 1–5 bytes | Identifies which map entry the following inline bytes satisfy.[engines/director/archive.cpp (approx. lines 988-1006)] |
| `<raw resource bytes>` | `res.size` bytes | Chunk data copied into `_ilsData` for offset `-1` entries.[engines/director/archive.cpp (approx. lines 992-1008)] |

## Step 3 – Building chunk streams for readers

Every chunk request ultimately flows through `getResource()`, which marks the resource as accessed, adjusts for the ILS special case, and seeks to the stored byte ranges. For Afterburner entries the code either wraps `_ilsData` in a memory stream or decompresses the zlib payload found at `_ilsBodyOffset + res.offset`, warning if the actual length differs from the metadata.[engines/director/archive.cpp (approx. lines 1124-1150)] Classic entries skip the 8-byte RIFX subheader before exposing the chunk as a substream.[engines/director/archive.cpp (approx. lines 1138-1156)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<offset + 0x00>` | 4 bytes | RIFX subchunk tag read implicitly when the caller consumes the returned stream.[engines/director/archive.cpp (approx. lines 1142-1150)] |
| `<offset + 0x04>` | 4 bytes | Subchunk length that the caller reads before the actual payload.[engines/director/archive.cpp (approx. lines 1142-1154)] |
| `<payload>` | `res.size - 8` bytes | Data served by the `SeekableSubReadStream` after the 8-byte header skip.[engines/director/archive.cpp (approx. lines 1144-1156)] |
| `<zlib stream>` | `res.size` bytes | Compressed data inflated by `readZlibData()` when `compressionType` ≠ 0.[engines/director/archive.cpp (approx. lines 1130-1148)] |

## Step 4 – Tracking and dumping chunk usage

Chunks that are never accessed get reported when the archive closes: `listUnaccessedChunks()` collects resource types where no entry flipped its `accessed` bit, filtering out known authoring-only tags such as `THUM` or `SCRF` before logging the unused counts.[engines/director/archive.cpp (approx. lines 40-96)] When script dumping is enabled, `dumpChunk()` reuses `getResource()` to read each chunk's payload, writes the raw bytes to `./dumps/<movie>-<tag>-<index>`, and frees the temporary buffer, making it easy to inspect chunk contents outside the game.[engines/director/archive.cpp (approx. lines 248-304)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<chunk stream bytes>` | `len` bytes | Complete chunk data read from `getResource()` (including the 8-byte subheader) before being written to disk.[engines/director/archive.cpp (approx. lines 248-284)] |
| `<dump filename>` | variable | UTF-8 path assembled from the archive name, tag, and index, matching Director's resource identifiers.[engines/director/archive.cpp (approx. lines 284-300)] |

These routines ensure the engine keeps precise accounting of every chunk: it records where bytes live, decompresses them as required, and exposes the resulting streams to higher-level systems.
