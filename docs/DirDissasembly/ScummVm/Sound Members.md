# Sound Cast Members

Sound cast members act as lightweight wrappers around external audio resources. Their constructor does not read extra fields beyond the shared `CastMember` header; instead `SoundCastMember::load()` inspects child resource tags to find the actual sound data, adapting to format changes across Director releases.[engines/director/castmember/sound.cpp (approx. lines 27-152)]

## Step 1 – Director 2–3 implicit SND lookup (version < 0x400)

Early Director movies store sound samples as classic Mac `SND ` resources. The loader derives the resource ID directly from the cast member identifier and falls back to those IDs whenever no explicit child entries are present.[engines/director/castmember/sound.cpp (approx. lines 58-73)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<'S','N','D',' '>` | 4 bytes | Resource tag selected for Director 2–3 audio, pointing at legacy Mac sound assets.[engines/director/castmember/sound.cpp (approx. lines 58-61)] |
| `<castId + castIDoffset>` | 2 bytes | Resource ID computed from `_castId` plus the library offset so the loader can request the correct `SND ` chunk.[engines/director/castmember/sound.cpp (approx. lines 59-61)] |

## Step 2 – Director 4–5 child tags (0x400 ≤ version < 0x600)

Director 4 begins attaching resource children to the cast member. ScummVM scans those children for `snd ` (lowercase) or `SND ` entries, still falling back to the derived Director 3 ID when no match is found.[engines/director/castmember/sound.cpp (approx. lines 61-73)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<'s','n','d',' '>` | 4 bytes | Preferred child tag for Director 4 `snd ` resources.[engines/director/castmember/sound.cpp (approx. lines 62-67)] |
| `<'S','N','D',' '>` | 4 bytes | Uppercase variant seen in some exports; treated as interchangeable.[engines/director/castmember/sound.cpp (approx. lines 62-67)] |
| `<fallback castId>` | 2 bytes | If no child is found, the loader reuses the derived Director 3 ID to access an embedded `SND ` resource.[engines/director/castmember/sound.cpp (approx. lines 69-73)] |

## Step 3 – Director 6 MOA sound bundles (0x600 ≤ version < 0x700)

Director 6 migrates audio into bundled MOA assets: a `snd ` directory entry with optional `sndH` headers, `sndS` sample data, and `ediM` editing metadata. ScummVM stitches these pieces together with `MoaSoundFormatDecoder` before exposing audio to the mixer.[engines/director/castmember/sound.cpp (approx. lines 74-96)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<'s','n','d',' '>` | 4 bytes | Child tag that points to the MOA sound directory; its index becomes the resource ID for playback.[engines/director/castmember/sound.cpp (approx. lines 75-79)] |
| `<'s','n','d','H'>` | 4 bytes | Optional header stream decoded via `MoaSoundFormatDecoder::loadHeaderStream()` to recover compression and looping metadata.[engines/director/castmember/sound.cpp (approx. lines 79-85)] |
| `<'s','n','d','S'>` | 4 bytes | Sample payload read with `MoaSoundFormatDecoder::loadSampleStream()` to supply the PCM data.[engines/director/castmember/sound.cpp (approx. lines 85-90)] |
| `<'e','d','i','M'>` | 4 bytes | Editor metadata that is currently stubbed; ScummVM logs a warning and discards the stream.[engines/director/castmember/sound.cpp (approx. lines 90-95)] |

## Step 4 – Resource decoding and linked file fallback

After the appropriate tag is selected, the loader retrieves the resource stream from the cast library, preferring MOA decoders when available. If no data exists in the archive, ScummVM looks up the sound on disk via `Cast::getLinkedPath()` and streams it with `AudioFileDecoder`. Successfully loaded `SND` resources flow through `SNDDecoder::loadStream()` so legacy loop metadata can be honored.[engines/director/castmember/sound.cpp (approx. lines 100-148)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<resource stream>` | variable | Raw bytes pulled from the archive and passed to either `MoaSoundFormatDecoder` or `SNDDecoder`, preserving loop bounds when valid.[engines/director/castmember/sound.cpp (approx. lines 100-148)] |

Loop flags are inferred from the decoder: Director 3 exports loop whenever loop bounds exist, while Director 4 and later clear invalid loops to match the original player’s behavior.[engines/director/castmember/sound.cpp (approx. lines 128-147)]

## Step 5 – CASt payload

Sound cast members contribute no additional data to the Director 4 `CASt` records besides the shared cast type byte, so `getCastDataSize()` reports a single byte and `writeCastData()` remains empty.[engines/director/castmember/sound.cpp (approx. lines 113-133)]
