# Cast Resource Mapping

This note explains how ScummVM maps Director cast libraries, CAS* tables, and CASt resources so that every cast member resolves to its data chunk. Each step calls out the exact bytes the loader reads while stitching those relationships together.

## Step 1 – KEY* rows map parent-child relationships

`readKeyTable()` starts by consuming two 16-bit fields for the entry size and two 32-bit counters for the total and used row counts, mirroring the 12-byte rows produced by Director.[engines/director/archive.cpp (approx. lines 1008-1048)] Every populated row then yields three 32-bit values: the child resource index, the parent index, and the child's four-character tag.[engines/director/archive.cpp (approx. lines 1048-1060)] These bytes populate `_keyData` so later lookups can see which cast member, movie, or library resource owns each chunk.

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<entry size>` | 2 bytes | Confirms the 12-byte layout Director uses for KEY* rows.[engines/director/archive.cpp (approx. lines 1008-1016)] |
| `<reserved size>` | 2 bytes | Secondary size field stored alongside the entry size.[engines/director/archive.cpp (approx. lines 1008-1016)] |
| `<total rows>` | 4 bytes | Count of allocated rows before filtering unused entries.[engines/director/archive.cpp (approx. lines 1010-1018)] |
| `<used rows>` | 4 bytes | Number of KEY* rows that contain data and must be parsed.[engines/director/archive.cpp (approx. lines 1012-1020)] |
| `<child resource id>` | 4 bytes per row | Resource index belonging to the current child chunk.[engines/director/archive.cpp (approx. lines 1048-1056)] |
| `<parent resource id>` | 4 bytes per row | Cast, movie, or library owner recorded for the child.[engines/director/archive.cpp (approx. lines 1050-1058)] |
| `<child tag>` | 4 bytes per row | Four-character resource type assigned to the child entry.[engines/director/archive.cpp (approx. lines 1052-1060)] |

## Step 2 – CAS* entries enumerate CASt members

Whenever a KEY* row references the `CAS*` tag, the loader marks the CAS* resource with the parent index, effectively noting which cast library it belongs to.[engines/director/archive.cpp (approx. lines 1060-1074)] After the key table is parsed, `readCast()` opens every CAS* chunk, divides the stream length by four, and reads a big-endian 32-bit cast index for each slot. Zero entries indicate empty cast positions, while non-zero indexes mark the CASt chunk that backs the slot and record both the cast number and owning library ID.[engines/director/archive.cpp (approx. lines 1022-1039)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<cast index>` | 4 bytes | Big-endian CASt resource number stored inside the CAS* table.[engines/director/archive.cpp (approx. lines 1028-1036)] |
| `00 00 00 00` | 4 bytes | Sentinel indicating an empty cast slot that should be skipped.[engines/director/archive.cpp (approx. lines 1028-1034)] |

```cpp
uint32 castIndex = casStream.readUint32BE();
if (castIndex == 0) {
    continue;
}
```
[engines/director/archive.cpp (approx. lines 1028-1035)]

This table-driven approach mirrors the way Director stored cast member pointers inside exported movies.[engines/director/archive.cpp (approx. lines 1022-1039)]

## Step 3 – Linking cast members to their resources

As each KEY* row is processed, the loader pushes the child resource into the `children` array of the matching CASt entry. If the parent and child indexes are swapped in the table (a quirk seen in some files), the code also handles that reversed layout by checking both directions before recording the relationship.[engines/director/archive.cpp (approx. lines 1074-1080)] Because every child entry retains the resource tag, later systems can enumerate sprites, scripts, or media belonging to a particular cast member straight from the KEY* data.

## Step 4 – Library-aware resource lookups

`getFirstResource(tag, parentId)` uses the `_keyData` map filled by the KEY* reader to locate the first child chunk belonging to a specific parent library or cast member, returning `nullptr` when the table never referenced that combination.[engines/director/archive.cpp (approx. lines 1088-1108)] This same mapping also stores shortcuts for the movie itself: rows that list parent index `1024` (Director's hardcoded movie resource ID) register the chunk under `_movieChunks` so the engine can fetch movie-wide assets such as score data without scanning the entire archive.[engines/director/archive.cpp (approx. lines 1080-1088)]
