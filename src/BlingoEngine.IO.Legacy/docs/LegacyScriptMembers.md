# Legacy Script Cast Members

[← Back to the format overview](./README.md)

Director stores script-specific metadata inside the `CASt` info block that
precedes every compiled `Lscr` resource. The modern Director 6–8 exports used by
our fixtures follow a predictable layout that allows the loader to grab the
script number, the embedded source, and the behaviour name without scanning.

## `CASt` info block structure

The info payload begins immediately after the `memberType`, `infoLength`, and
`specificLength` words in the `CASt` chunk. Offsets below are measured from the
start of the info payload. Director 4 and later include a pointer table that
positions the embedded script text at byte `0x006A`. A small subset of
truncated archives omit the pointer data, so the loader falls back to the
length-adjacent offset at `0x0021`. The reader chooses between these layouts by
inspecting the archive's **Director file version** stored in the `imap` control
block.

| Offset | Size | Description |
| --- | --- | --- |
| `0x0000` | 4 | Reserved fields that remain zero in observed files. |
| `0x0010` | 4 | **Script number** stored as a big-endian signed integer. The
value links the member to the `Lscr` entry listed in the `Lctx/LctX`
script-context tables. |
| `0x001D` | 4 | **Script text length** stored as a little-endian unsigned word.
Lengths of zero mark compiled-only entries. |
| `0x0068` | 2 | Pointer table slot that precedes the behaviour text (Director 4+). The loader skips the two-byte pointer and begins reading at `0x006A`. |
| `0x006A` | `length` | Lingo source text encoded as single-byte characters (Director 4+). |
| `0x006A + length` | 1 | Length of the script name. Zero indicates that the name is omitted. |
| `0x006B + length` | `nameLength` | Script/behaviour name in single-byte encoding. |

Older exports that strip the pointer table expose the script text immediately
after the length word at `0x0021`. Unknown Director versions are left untouched
until we document their layout.

## Script categories

The `CASt` specific data stores a selector word whose low byte identifies the
script category. ScummVM documents the same mapping in its Director reader, so
we keep the codes aligned with that behaviour. For compatibility the loader
accepts both big- and little-endian encodings as well as four-byte words that
carry the selector in the least-significant byte.

| Code | Category |
| --- | --- |
| `0x01` | Behaviour (score) script |
| `0x03` | Movie script |
| `0x07` | Parent script |

For more background see
[docs/DirDissasembly/ScummVm/Script Members.md](../../../docs/DirDissasembly/ScummVm/Script%20Members.md).

## Compatibility notes

* Compiled-only behaviours report a zero text length. The reader keeps the
format information but leaves the `Text` and `Name` properties unset.
* Director 4/5 style casts only provide the script selector in the specific
block. When the pointer table is missing, the reader falls back to the
length-adjacent offsets after confirming the byte count fits.
