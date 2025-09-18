# Cast Member Base Records

This note captures the version-specific headers that wrap every Director cast member (`CASt`) entry before the type-specific loaders see the payload. Each section calls out the bytes ScummVM reads while dispatching the stream into the correct cast member class.

## Step 1 – Director 2–3 `VWCR` table entries (version < 0x400)

Legacy movies enumerate cast members inside the `VWCR` block. Each entry starts with a size byte and a type selector; any remaining bytes are forwarded straight to the cast-specific constructor.[engines/director/cast.cpp (approx. lines 1374-1414)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<entry size>` | 1 byte | Total number of bytes to read for this cast entry, including the type byte and optional flag byte.[engines/director/cast.cpp (approx. lines 1379-1396)] |
| `<cast type>` | 1 byte | Enumerated cast type used to choose the correct subclass (bitmap, text, palette, etc.).[engines/director/cast.cpp (approx. lines 1399-1443)] |
| `<flags1>` | 1 byte (optional) | Present when `entry size` exceeds 1; Director stored miscellaneous cast flags here, so ScummVM preserves the byte for later use.[engines/director/cast.cpp (approx. lines 1388-1394)] |
| `<cast payload>` | `entry size` − consumed bytes | Remaining bytes are passed to the loader for the requested cast type; the stream position is restored to the end of the entry afterward.[engines/director/cast.cpp (approx. lines 1397-1429)] |

## Step 2 – Director 4–5 `CASt` headers (0x400 ≤ version < 0x500)

Director 4 switched to explicit `CASt` resources. ScummVM first reads the data and info lengths, then peeks at the type byte (and an optional `flags1` byte) before slicing the payload. The info block sits immediately after the cast data.[engines/director/cast.cpp (approx. lines 1469-1506)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<cast data size>` | 2 bytes | Big-endian count of bytes that follow in the cast-data section, including the type byte and optional `flags1`.[engines/director/cast.cpp (approx. lines 1494-1503)] |
| `<cast info size>` | 4 bytes | Big-endian byte count for the metadata chunk stored after the cast data.[engines/director/cast.cpp (approx. lines 1494-1506)] |
| `<cast type>` | 1 byte | Identifies the cast subclass instantiated later in `loadCastData`.[engines/director/cast.cpp (approx. lines 1498-1537)] |
| `<flags1>` | 1 byte (optional) | Preserved when the cast data size exceeds one byte; text-derived members rely on this flag.[engines/director/cast.cpp (approx. lines 1499-1503)] |
| `<cast data payload>` | `cast data size` − consumed bytes | Version-specific data passed to the constructor for the requested cast type.[engines/director/cast.cpp (approx. lines 1501-1537)] |
| `<cast info payload>` | `cast info size` bytes | Optional metadata strings or timestamps consumed by `Cast::loadCastInfo` using the previously computed offset.[engines/director/cast.cpp (approx. lines 1504-1506)] |

## Step 3 – Director 5–10 `CASt` headers (0x500 ≤ version < 0x1100)

Later RIFX movies promote the type field to 32 bits and swap the order of the header fields. The info block appears first in the stream, followed by the cast data segment the loader copies into a scratch buffer.[engines/director/cast.cpp (approx. lines 1507-1514)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<cast type>` | 4 bytes | Four-character tag recorded in big endian; values mirror the legacy cast type enum used in the dispatcher.[engines/director/cast.cpp (approx. lines 1508-1537)] |
| `<cast info size>` | 4 bytes | Big-endian byte count for the info block immediately following the header.[engines/director/cast.cpp (approx. lines 1508-1513)] |
| `<cast data size>` | 4 bytes | Big-endian byte count describing how many bytes belong to the cast-data section stored after the info block.[engines/director/cast.cpp (approx. lines 1509-1513)] |
| `<cast info payload>` | `cast info size` bytes | Optional info strings and timestamps that ScummVM relays to `loadCastInfo` using the offset derived from `castInfoSize`.[engines/director/cast.cpp (approx. lines 1512-1513)] |
| `<cast data payload>` | `cast data size` bytes | Bytes forwarded to the specific cast member constructor for parsing.[engines/director/cast.cpp (approx. lines 1511-1537)] |

## Step 4 – Instantiating cast members

Once the header bytes have been peeled away, ScummVM wraps the remaining cast data in a `MemoryReadStream` and instantiates the appropriate cast subclass. The dispatcher preserves `flags1`, saves the raw data size on the cast object, and copies any child resource references so type loaders can resolve external assets later.[engines/director/cast.cpp (approx. lines 1518-1587)]

