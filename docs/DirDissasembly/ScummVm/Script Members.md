# Script Cast Members

Script cast members describe the type of Lingo code that will be bound to a cast entry. `ScriptCastMember` only parses a two-byte selector from the `CASt` stream because the compiled bytecode lives in companion `Lscr` resources that the cast later attaches to the runtime Lingo archive.[engines/director/castmember/script.cpp (approx. lines 30-66)][engines/director/cast.cpp (approx. lines 1748-1765)]

## Step 1 – Director 4–10 script header (0x400 ≤ version < 0x1100)

Director 4 and later exports store a big-endian word whose low byte identifies whether the member is a score, movie, or parent script. The constructor asserts that no additional data follows the selector.[engines/director/castmember/script.cpp (approx. lines 40-63)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<script type word>` | 2 bytes | Big-endian selector where the low byte matches one of the codes below; the high byte remains zero in observed files.[engines/director/castmember/script.cpp (approx. lines 43-57)] |

| Code (low byte) | Script category |
| --- | --- |
| `0x01` | Score script executed in the score timeline.[engines/director/castmember/script.cpp (approx. lines 45-48)] |
| `0x03` | Movie script that responds to global events.[engines/director/castmember/script.cpp (approx. lines 48-51)] |
| `0x07` | Parent script; ScummVM logs a warning because this subtype is not implemented yet.[engines/director/castmember/script.cpp (approx. lines 52-55)] |

## Step 2 – Serialized output format

When exporting cast data, ScummVM writes the selector back out as two bytes: a zero placeholder followed by the script-type code. Director 4 members also reserve two extra bytes in the cast-data header for the cast type and flag byte that wrap the stream.[engines/director/castmember/script.cpp (approx. lines 142-171)][engines/director/cast.cpp (approx. lines 1469-1543)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `0x00` | 1 byte | Placeholder byte written before the selector; legacy Director files carry an unused high byte here.[engines/director/castmember/script.cpp (approx. lines 156-171)] |
| `<script type code>` | 1 byte | Low byte reproduced from `_scriptType`: `0x01`, `0x03`, or `0x07`.[engines/director/castmember/script.cpp (approx. lines 160-168)] |

## Step 3 – Linking compiled `Lscr` resources

`Cast::loadLingoContext()` walks the cast-resource table, fetches each `Lscr` chunk that contains compiled Lingo code, and registers the script under the type recorded in the `ScriptCastMember`. Duplicated IDs are rejected, and scripts with the placeholder cast library of `-1` are patched to the owning cast.[engines/director/cast.cpp (approx. lines 1748-1765)]

