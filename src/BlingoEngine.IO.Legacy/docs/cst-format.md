# Cast Library (`.cst`) Resources

## Overview

Cast libraries hold the assets that populate a Director movie: sprites, scripts, sounds, and other cast members. Exported movies keep this information in `CASt` resources that are referenced through `KEY*` and `CAS*` tables. Understanding these tables makes it easier to resolve each cast slot back to the chunk that supplies its data.

## `KEY*` – parent/child relationships

`KEY*` resources are always read before any cast data is parsed. Each row in the table links a parent resource (movie or cast library) to a child resource (such as `CASt`, `Lscr`, or media chunks).

| Field | Length | Notes |
| --- | --- | --- |
| Entry size | 2 bytes | Confirms the 12-byte row layout. |
| Reserved size | 2 bytes | Secondary size field stored alongside the entry size. |
| Total rows | 4 bytes | Number of allocated rows in the table. |
| Used rows | 4 bytes | Number of rows that contain actual data. |
| Child resource id | 4 bytes per row | Resource index of the child chunk. |
| Parent resource id | 4 bytes per row | Resource index of the owning movie or cast library. |
| Child tag | 4 bytes per row | Four-character code that identifies the child resource type. |

When a row references a `CAS*` resource, the loader records which library owns that table before continuing.

## `CAS*` – cast slot lookup

After the `KEY*` table is processed, each referenced `CAS*` chunk is opened. The chunk consists of a series of big-endian 32-bit cast indices:

| Value | Meaning |
| --- | --- |
| `00 00 00 00` | Empty cast slot. |
| Non-zero | Resource id of the `CASt` chunk that supplies the cast member. |

The slot position in the table becomes the cast member number, and the parent id recorded from `KEY*` identifies the cast library that owns the slot.

## `CASt` – cast member records

Every cast member ultimately lives in a `CASt` resource. The header layout changes across Director releases, but the goal is the same: isolate a small block of cast-specific bytes ("cast data") and optional metadata ("cast info") before passing the payload to the appropriate loader.

### Director 2–3 (`VWCR` entries)

Earlier movies use the `VWCR` block instead of standalone `CASt` resources. Each entry is parsed as:

| Field | Length | Notes |
| --- | --- | --- |
| Entry size | 1 byte | Total length of the entry, including the type byte. |
| Cast type | 1 byte | Enumerated cast type (bitmap, text, palette, etc.). |
| Flags1 | 1 byte (optional) | Present when `entry size` is greater than 1. |
| Cast payload | Remaining bytes | Forwarded directly to the cast-type loader. |

### Director 4–5 (`CASt` header)

Director 4 introduces explicit `CASt` chunks with a short header:

| Field | Length | Notes |
| --- | --- | --- |
| Cast data size | 2 bytes | Big-endian length of the cast-data section (includes the type byte). |
| Cast info size | 4 bytes | Big-endian length of the metadata block that follows the cast data. |
| Cast type | 1 byte | Identifies which cast subclass should parse the payload. |
| Flags1 | 1 byte (optional) | Present when the cast data size exceeds one byte. |
| Cast data payload | `cast data size` − consumed bytes | Bytes passed to the cast-specific parser. |
| Cast info payload | `cast info size` bytes | Optional metadata strings or timestamps. |

### Director 5–10 (`CASt` header)

Later versions expand the header and reorder the fields:

| Field | Length | Notes |
| --- | --- | --- |
| Cast type | 4 bytes | Four-character tag stored in big endian. |
| Cast info size | 4 bytes | Big-endian length of the metadata block that immediately follows. |
| Cast data size | 4 bytes | Big-endian length of the cast-data section stored after the info block. |
| Cast info payload | `cast info size` bytes | Optional metadata strings or timestamps. |
| Cast data payload | `cast data size` bytes | Bytes forwarded to the cast-specific parser. |

### Loading process

Regardless of the version, the loader wraps the cast-data payload in a memory stream, instantiates the matching cast member class, and attaches any linked resources recorded in the `KEY*` table. This allows higher-level systems to fetch sprites, scripts, and media by cast member number without re-reading the tables.
