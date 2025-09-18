# Movie Cast Members

Movie cast members extend the film-loop loader and add a few movie-specific flags. This guide breaks down the inline `CASt` data as well as the external `SCVW` stream that enumerates every sprite channel in the looped movie.

## Step 1 – Director 4 film-loop headers (0x400 ≤ version < 0x500)

Director 4 encodes the movie’s initial rectangle and control flags directly inside the `CASt` data before the `SCVW` frames are consulted.[engines/director/castmember/filmloop.cpp (approx. lines 52-60)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit values read through `Movie::readRect()` to initialise `_initialRect`.[engines/director/castmember/filmloop.cpp (approx. lines 52-54)] |
| `<flags>` | 4 bytes | Bitfield copied into `_flags`; individual bits toggle looping, sound, cropping, and centering.[engines/director/castmember/filmloop.cpp (approx. lines 54-59)] |
| `<unknown word>` | 2 bytes | Stored but unused (`unk1`); ScummVM logs the value for diagnostics.[engines/director/castmember/filmloop.cpp (approx. lines 54-56)] |

## Step 2 – Director 5 film-loop headers (0x500 ≤ version < 0x600)

Director 5 reuses the same structure but adjusts the bit assignments for looping. The loader still consumes the rectangle, flag dword, and trailing word from the `CASt` stream.[engines/director/castmember/filmloop.cpp (approx. lines 61-69)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<rect.top>`..`<rect.right>` | 8 bytes | Initial rectangle stored as four signed words.[engines/director/castmember/filmloop.cpp (approx. lines 62-63)] |
| `<flags>` | 4 bytes | Updated control bits where `0x20` disables looping, while the remaining bits mirror Director 4.[engines/director/castmember/filmloop.cpp (approx. lines 63-68)] |
| `<unknown word>` | 2 bytes | Reserved value logged as `unk1`; no behaviour depends on it yet.[engines/director/castmember/filmloop.cpp (approx. lines 63-64)] |

## Step 3 – Director 2 `SCVW` stream layout

Legacy movies rely entirely on the external `SCVW` resource to describe film loops. The stream begins with a size field and then iterates over frames, each of which contains channel-sized message runs encoded with single-byte widths and offsets.[engines/director/castmember/filmloop.cpp (approx. lines 201-268)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<SCVW size>` | 4 bytes | Big-endian length used to bound the decoding loop.[engines/director/castmember/filmloop.cpp (approx. lines 205-212)] |
| `<frame size>` | 2 bytes | Big-endian word indicating the number of bytes that follow for the current frame; zero-sized frames are skipped.[engines/director/castmember/filmloop.cpp (approx. lines 216-222)] |
| `<msg width byte>` | 1 byte × 2 | Director stores message widths and orders as bytes scaled by two; ScummVM multiplies by two and subtracts `0x20` from the order to derive the channel number and offset.[engines/director/castmember/filmloop.cpp (approx. lines 227-236)] |
| `<sprite bytes>` | `msg width` bytes | Sprite channel chunks decoded via `readSpriteDataD2()`; runs may spill into subsequent channels based on the message width.[engines/director/castmember/filmloop.cpp (approx. lines 241-264)] |

## Step 4 – Director 4–6 `SCVW` headers

Later versions keep the same framing concept but promote widths and orders to 16-bit values. Before frame decoding begins, ScummVM skips six bytes of header data, records the sprite-channel size, and seeks to the frame table offset.[engines/director/castmember/filmloop.cpp (approx. lines 306-323)][engines/director/castmember/filmloop.cpp (approx. lines 422-438)][engines/director/castmember/filmloop.cpp (approx. lines 546-564)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<SCVW size>` | 4 bytes | Total size of the `SCVW` resource.[engines/director/castmember/filmloop.cpp (approx. lines 306-312)][engines/director/castmember/filmloop.cpp (approx. lines 422-428)][engines/director/castmember/filmloop.cpp (approx. lines 546-552)] |
| `<frames offset>` | 4 bytes | Offset where frame records begin; ScummVM uses it to skip the header blob.[engines/director/castmember/filmloop.cpp (approx. lines 314-323)][engines/director/castmember/filmloop.cpp (approx. lines 430-438)][engines/director/castmember/filmloop.cpp (approx. lines 554-564)] |
| `<ignored header>` | 6 bytes | Reserved data skipped before the channel-size field.[engines/director/castmember/filmloop.cpp (approx. lines 314-321)][engines/director/castmember/filmloop.cpp (approx. lines 430-436)][engines/director/castmember/filmloop.cpp (approx. lines 554-560)] |
| `<channel size>` | 2 bytes | Big-endian sprite-channel stride (`20` bytes for Director 4, `24` bytes for Director 5 and 6).[engines/director/castmember/filmloop.cpp (approx. lines 320-322)][engines/director/castmember/filmloop.cpp (approx. lines 436-438)][engines/director/castmember/filmloop.cpp (approx. lines 560-562)] |

## Step 5 – Director 4–6 frame messages

Within each frame the loader reads 16-bit message widths and orders, then copies the requested number of bytes into sprite descriptors. Director 5 and 6 also normalise cast-library references for sprites imported from other casts.[engines/director/castmember/filmloop.cpp (approx. lines 326-411)][engines/director/castmember/filmloop.cpp (approx. lines 439-536)][engines/director/castmember/filmloop.cpp (approx. lines 566-660)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<frame size>` | 2 bytes | Big-endian byte count for the frame body; decremented as messages are parsed.[engines/director/castmember/filmloop.cpp (approx. lines 326-333)][engines/director/castmember/filmloop.cpp (approx. lines 439-454)][engines/director/castmember/filmloop.cpp (approx. lines 566-577)] |
| `<msg width>` | 2 bytes | Big-endian width of the sprite data that follows.[engines/director/castmember/filmloop.cpp (approx. lines 337-348)][engines/director/castmember/filmloop.cpp (approx. lines 455-466)][engines/director/castmember/filmloop.cpp (approx. lines 578-590)] |
| `<order>` | 2 bytes | Big-endian order value; dividing by `channel size` yields the channel index while the remainder produces the channel offset.[engines/director/castmember/filmloop.cpp (approx. lines 338-346)][engines/director/castmember/filmloop.cpp (approx. lines 455-466)][engines/director/castmember/filmloop.cpp (approx. lines 578-590)] |
| `<sprite bytes>` | `msg width` bytes | Channel data forwarded to the version-specific `readSpriteDataDX()` helper.[engines/director/castmember/filmloop.cpp (approx. lines 351-366)][engines/director/castmember/filmloop.cpp (approx. lines 467-500)][engines/director/castmember/filmloop.cpp (approx. lines 592-627)] |

## Step 6 – Movie-specific flag bits

Film-loop flags control runtime behaviour for both generic film loops and movie cast members. Movie casts also toggle Lingo script execution via an extra bit in `_flags`.[engines/director/castmember/filmloop.cpp (approx. lines 54-69)][engines/director/castmember/movie.cpp (approx. lines 32-42)]

| Bit mask | Director 4 meaning | Director 5 meaning |
| --- | --- | --- |
| `0x40` | Looping disabled when set; cleared means the loop repeats.[engines/director/castmember/filmloop.cpp (approx. lines 54-58)] | N/A |
| `0x20` | N/A | Looping disabled when set; cleared means the loop repeats.[engines/director/castmember/filmloop.cpp (approx. lines 63-67)] |
| `0x10` | Enables movie scripts when set (Movie cast only).[engines/director/castmember/movie.cpp (approx. lines 32-42)] | Enables movie scripts when set (Movie cast only).[engines/director/castmember/movie.cpp (approx. lines 32-42)] |
| `0x08` | Enables linked sound playback.[engines/director/castmember/filmloop.cpp (approx. lines 57-68)] | Enables linked sound playback.[engines/director/castmember/filmloop.cpp (approx. lines 63-68)] |
| `0x02` | When clear, crop is enabled; when set, crop is disabled.[engines/director/castmember/filmloop.cpp (approx. lines 57-68)] | When clear, crop is enabled; when set, crop is disabled.[engines/director/castmember/filmloop.cpp (approx. lines 63-68)] |
| `0x01` | Centers the movie on stage when set.[engines/director/castmember/filmloop.cpp (approx. lines 57-69)] | Centers the movie on stage when set.[engines/director/castmember/filmloop.cpp (approx. lines 63-69)] |

## Step 7 – Locating the `SCVW` resource

During `load()` the film-loop cast searches either the cast-array offset (for Director 3 and earlier) or its child resource list (for Director 4–6) to locate the correct `SCVW` stream before decoding frames.[engines/director/castmember/filmloop.cpp (approx. lines 690-724)]

