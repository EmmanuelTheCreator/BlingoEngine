# Digital Video Cast Members

Digital video casts encapsulate linked QuickTime or AVI movies. The loader records a bounding rectangle, decodes a packed flag word, and later resolves a `MooV` alias or filesystem link before instantiating the appropriate video decoder.

## Step 1 – Director 4–6 `CASt` records

When a digital video member is constructed, ScummVM reads a stage rectangle followed by a 32-bit flag word. No additional bytes are stored inline; media data is external.[engines/director/castmember/digitalvideo.cpp (approx. lines 95-116)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit values describing the movie’s default placement on stage.[engines/director/castmember/digitalvideo.cpp (approx. lines 95-96)] |
| `<video flags>` | 4 bytes | Bitfield stored in `_vflags`; the loader also extracts the encoded frame rate from the high byte.[engines/director/castmember/digitalvideo.cpp (approx. lines 96-113)] |

## Step 2 – `_vflags` bit layout

The `_vflags` word controls playback behaviour and identifies the media container. ScummVM expands each bit into boolean properties during construction.[engines/director/castmember/digitalvideo.cpp (approx. lines 97-114)]

| Bit mask | Meaning |
| --- | --- |
| `0x8000` | Marks the cast as referencing a QuickTime movie (`_qtmovie`).[engines/director/castmember/digitalvideo.cpp (approx. lines 101-105)] |
| `0x4000` | Marks the cast as referencing a Video for Windows clip (`_avimovie`).[engines/director/castmember/digitalvideo.cpp (approx. lines 101-105)] |
| `0x3000` | When combined with `0x0800`, selects an alternative frame-rate type stored in bits 12–13.[engines/director/castmember/digitalvideo.cpp (approx. lines 99-103)] |
| `0x0800` | Signals that the frame-rate type bits are valid and should override the default (`kFrameRateDefault`).[engines/director/castmember/digitalvideo.cpp (approx. lines 99-104)] |
| `0x0400` | Requests preloading of the movie resource before playback (`_preload`).[engines/director/castmember/digitalvideo.cpp (approx. lines 105-108)] |
| `0x0200` | Disables video rendering when set (`_enableVideo` becomes false).[engines/director/castmember/digitalvideo.cpp (approx. lines 106-108)] |
| `0x0100` | Starts the movie paused (`_pausedAtStart`).[engines/director/castmember/digitalvideo.cpp (approx. lines 106-108)] |
| `0x0040` | Shows transport controls when set (`_showControls`).[engines/director/castmember/digitalvideo.cpp (approx. lines 109-110)] |
| `0x0020` | Enables “direct to stage” compositing (`_directToStage`).[engines/director/castmember/digitalvideo.cpp (approx. lines 109-110)] |
| `0x0010` | Enables looping playback (`_looping`).[engines/director/castmember/digitalvideo.cpp (approx. lines 110-112)] |
| `0x0008` | Enables audio playback (`_enableSound`).[engines/director/castmember/digitalvideo.cpp (approx. lines 111-112)] |
| `0x0002` | When clear, cropping is active; when set, cropping is disabled (`_crop`).[engines/director/castmember/digitalvideo.cpp (approx. lines 112-113)] |
| `0x0001` | Centers the movie on stage when set (`_center`).[engines/director/castmember/digitalvideo.cpp (approx. lines 112-114)] |

## Step 3 – Expected data lengths by version

`DigitalVideoCastMember::getCastDataSize()` documents the number of bytes Director stores in the `CASt` record for each version so the serializer can round-trip the data accurately.[engines/director/castmember/digitalvideo.cpp (approx. lines 655-666)]

| Director version | Bytes stored |
| --- | --- |
| 4.x | 13 bytes when `_flags1` equals `0xFF`, otherwise 14 bytes (rectangle + flag word + optional `flags1`).[engines/director/castmember/digitalvideo.cpp (approx. lines 655-662)] |
| 5.x | 12 bytes (rectangle + flag word only).[engines/director/castmember/digitalvideo.cpp (approx. lines 661-662)] |

## Step 4 – Resolving external movie data

At load time the cast asks its parent `Cast` object for a playable path. The lookup searches for an embedded `MooV` resource (extracting the QuickTime alias path when present) and falls back to a linked filesystem path if the resource is missing. Once a location is resolved, `loadVideo()` instantiates either the QuickTime or AVI decoder to stream frames and audio.[engines/director/cast.cpp (approx. lines 1284-1329)][engines/director/castmember/digitalvideo.cpp (approx. lines 179-259)][engines/director/castmember/digitalvideo.cpp (approx. lines 360-434)]

