# Film Loop Cast Members

Film loops are pre-recorded sprite sequences that ScummVM instantiates through `FilmLoopCastMember`. The loader reads a small `CASt` header for Director 4–5 movies and then decodes an external `SCVW` stream that lists every sprite channel per frame.[engines/director/castmember/filmloop.cpp (approx. lines 42-721)]

## Step 1 – Director 4 `CASt` header (0x400 ≤ version < 0x500)

Director 4 exports include the initial stage rectangle and a 32-bit flag word directly inside the cast-data stream. ScummVM captures the rectangle with `Movie::readRect()` and interprets the flag bits to toggle looping, audio, cropping, and centering.[engines/director/castmember/filmloop.cpp (approx. lines 52-60)][engines/director/movie.cpp (approx. lines 276-284)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit values consumed by `Movie::readRect()` to populate `_initialRect`.[engines/director/castmember/filmloop.cpp (approx. lines 52-54)][engines/director/movie.cpp (approx. lines 276-284)] |
| `<flags>` | 4 bytes | Control bits stored in `_flags`; ScummVM flips `_looping`, `_enableSound`, `_crop`, and `_center` based on their states.[engines/director/castmember/filmloop.cpp (approx. lines 54-59)] |
| `<unknown word>` | 2 bytes | Reserved big-endian word logged as `unk1`; the engine retains it for completeness.[engines/director/castmember/filmloop.cpp (approx. lines 54-56)] |

## Step 2 – Director 5 `CASt` header (0x500 ≤ version < 0x600)

Director 5 keeps the same layout but changes which bit disables looping. The loader still reads the rectangle, a flag dword, and the trailing word from the cast stream.[engines/director/castmember/filmloop.cpp (approx. lines 61-69)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<rect.top>`..`<rect.right>` | 8 bytes | Director 5 movies reuse the same rectangle layout to seed `_initialRect`.[engines/director/castmember/filmloop.cpp (approx. lines 61-63)] |
| `<flags>` | 4 bytes | Bitfield where `0x20` disables looping while the other bits match Director 4 semantics.[engines/director/castmember/filmloop.cpp (approx. lines 63-68)] |
| `<unknown word>` | 2 bytes | Logged as `unk1`; no behaviour depends on it yet.[engines/director/castmember/filmloop.cpp (approx. lines 63-64)] |

## Step 3 – Film-loop flag bits

Both Director 4 and Director 5 decode the same runtime behaviours from the control word. Movie cast members reuse these flags, so the table below shows the meaning of each bit after the loader masks the value.[engines/director/castmember/filmloop.cpp (approx. lines 54-69)][engines/director/castmember/movie.cpp (approx. lines 32-42)]

| Bit mask | Director 4 meaning | Director 5 meaning |
| --- | --- | --- |
| `0x40` | Looping disabled when set; cleared means the loop repeats. | N/A (bit unused). |
| `0x20` | N/A (bit unused). | Looping disabled when set; cleared means the loop repeats. |
| `0x10` | Enables movie scripts when set (Movie cast only). | Enables movie scripts when set (Movie cast only). |
| `0x08` | Enables linked sound playback. | Enables linked sound playback. |
| `0x02` | When clear, crop is enabled; when set, crop is disabled. | When clear, crop is enabled; when set, crop is disabled. |
| `0x01` | Centers the movie on stage when set. | Centers the movie on stage when set. |

## Step 4 – Director 2 `SCVW` stream layout (version < 0x300)

Early film loops rely entirely on an external `SCVW` resource. ScummVM reads a big-endian length, then iterates frames composed of byte-sized message descriptors. Each descriptor encodes the channel number and byte count for the sprite data that follows.[engines/director/castmember/filmloop.cpp (approx. lines 205-266)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<SCVW size>` | 4 bytes | Total size of the stream; used as an upper bound for the decode loop.[engines/director/castmember/filmloop.cpp (approx. lines 205-214)] |
| `<frame size>` | 2 bytes | Big-endian count for the current frame; frames with size 0 are skipped.[engines/director/castmember/filmloop.cpp (approx. lines 217-223)] |
| `<msg width byte>` | 1 byte | Byte width doubled (`*2`) to calculate the sprite payload size; subtracts 0x20 from the next byte to align offsets.[engines/director/castmember/filmloop.cpp (approx. lines 228-233)] |
| `<order byte>` | 1 byte | Byte order doubled (`*2`) then offset by `0x20` to derive the channel and byte offset within the channel.[engines/director/castmember/filmloop.cpp (approx. lines 228-234)] |
| `<sprite bytes>` | `msg width` bytes | Sprite data chunk forwarded to `readSpriteDataD2()` for decoding; long runs spill into subsequent channels.[engines/director/castmember/filmloop.cpp (approx. lines 234-264)] |

## Step 5 – Director 4 `SCVW` header and frames (0x400 ≤ version < 0x500)

Director 4 promotes message widths and orders to 16-bit values. Before decoding frames, ScummVM skips a short header and records the sprite channel stride (`kSprChannelSizeD4`).[engines/director/castmember/filmloop.cpp (approx. lines 306-376)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<SCVW size>` | 4 bytes | Big-endian resource size for bounds checking.[engines/director/castmember/filmloop.cpp (approx. lines 306-312)] |
| `<frames offset>` | 4 bytes | Offset to the frame table; the loader skips to this location before decoding frames.[engines/director/castmember/filmloop.cpp (approx. lines 314-322)] |
| `<ignored header>` | 6 bytes | Reserved header blob skipped because Director does not document it.[engines/director/castmember/filmloop.cpp (approx. lines 319-320)] |
| `<channel size>` | 2 bytes | Value expected to equal `kSprChannelSizeD4` (20 bytes) and stored for per-frame calculations.[engines/director/castmember/filmloop.cpp (approx. lines 320-322)] |
| `<padding to frames>` | `framesOffset - 16` bytes | Remaining header skipped so the stream lands on the first frame record.[engines/director/castmember/filmloop.cpp (approx. lines 321-322)] |

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<frame size>` | 2 bytes | Big-endian frame length minus the size field; zero-sized frames are ignored.[engines/director/castmember/filmloop.cpp (approx. lines 326-333)] |
| `<msg width>` | 2 bytes | Big-endian byte count for the sprite payload that follows.[engines/director/castmember/filmloop.cpp (approx. lines 337-343)] |
| `<order>` | 2 bytes | Big-endian order value; dividing by `channelSize` yields the channel index, and the remainder becomes the offset inside the channel.[engines/director/castmember/filmloop.cpp (approx. lines 338-344)] |
| `<sprite bytes>` | `msg width` bytes | Sprite run decoded via `readSpriteDataD4()`; large runs spill into later channels by incrementing the order.[engines/director/castmember/filmloop.cpp (approx. lines 345-372)] |

## Step 6 – Director 5 `SCVW` header and frames (0x500 ≤ version < 0x600)

Director 5 follows the same header structure but increases the channel stride to 24 bytes and normalises cast-library references for imported sprites.[engines/director/castmember/filmloop.cpp (approx. lines 426-540)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<SCVW size>` | 4 bytes | Resource length mirrored from the Director 4 loader.[engines/director/castmember/filmloop.cpp (approx. lines 426-434)] |
| `<frames offset>` | 4 bytes | Offset where frame records begin.[engines/director/castmember/filmloop.cpp (approx. lines 434-442)] |
| `<ignored header>` | 6 bytes | Reserved header bytes skipped before the channel size field.[engines/director/castmember/filmloop.cpp (approx. lines 439-441)] |
| `<channel size>` | 2 bytes | Should equal `kSprChannelSizeD5` (24 bytes) for Director 5 exports.[engines/director/castmember/filmloop.cpp (approx. lines 440-442)] |
| `<padding to frames>` | `framesOffset - 16` bytes | Remaining header skipped to reach the first frame.[engines/director/castmember/filmloop.cpp (approx. lines 441-442)] |

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<frame size>` | 2 bytes | Big-endian frame payload size.[engines/director/castmember/filmloop.cpp (approx. lines 446-453)] |
| `<msg width>` | 2 bytes | Big-endian sprite run length consumed by `readSpriteDataD5()`.[engines/director/castmember/filmloop.cpp (approx. lines 457-466)] |
| `<order>` | 2 bytes | Big-endian order value; division by `channelSize` yields the channel index.[engines/director/castmember/filmloop.cpp (approx. lines 457-466)] |
| `<sprite bytes>` | `msg width` bytes | Sprite data; cast-library IDs of `-1` are replaced with the film loop’s cast ID before storing the sprite.[engines/director/castmember/filmloop.cpp (approx. lines 470-497)] |

## Step 7 – Director 6 `SCVW` header and frames (0x600 ≤ version < 0x700)

Director 6 still writes an `SCVW` header with the same fields, but ScummVM treats the channel stride as 24 bytes and reuses the Director 5 frame decoding logic with `readSpriteDataD6()`.[engines/director/castmember/filmloop.cpp (approx. lines 550-666)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<SCVW size>` | 4 bytes | Big-endian resource size for the loop stream.[engines/director/castmember/filmloop.cpp (approx. lines 550-558)] |
| `<frames offset>` | 4 bytes | Header offset pointing to the start of frame records.[engines/director/castmember/filmloop.cpp (approx. lines 558-566)] |
| `<ignored header>` | 6 bytes | Reserved header bytes that are skipped.[engines/director/castmember/filmloop.cpp (approx. lines 563-565)] |
| `<channel size>` | 2 bytes | Expected to equal `kSprChannelSizeD6`; the loader currently reads 24 and advances regardless.[engines/director/castmember/filmloop.cpp (approx. lines 564-566)] |
| `<padding to frames>` | `framesOffset - 16` bytes | Remaining header skipped to land on the frame table.[engines/director/castmember/filmloop.cpp (approx. lines 565-566)] |

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<frame size>` | 2 bytes | Big-endian length for each frame payload.[engines/director/castmember/filmloop.cpp (approx. lines 570-577)] |
| `<msg width>` | 2 bytes | Big-endian sprite run size forwarded to `readSpriteDataD6()`.[engines/director/castmember/filmloop.cpp (approx. lines 581-590)] |
| `<order>` | 2 bytes | Big-endian order value; the quotient selects the channel while the remainder provides the channel offset.[engines/director/castmember/filmloop.cpp (approx. lines 582-588)] |
| `<sprite bytes>` | `msg width` bytes | Sprite payload; cast and script library IDs of `-1` are replaced with the film loop’s cast library ID.[engines/director/castmember/filmloop.cpp (approx. lines 595-621)] |

## Step 8 – Locating `SCVW` resources

`FilmLoopCastMember::load()` either looks up `SCVW` records via the cast’s child list (Director 4–6) or directly fetches the resource by ID for legacy versions. Unsupported versions above Director 7 log a stub warning.[engines/director/castmember/filmloop.cpp (approx. lines 684-728)]

