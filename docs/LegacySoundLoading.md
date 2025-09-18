[← Back to Docs Home](README.md)

# Legacy Sound Loading Notes

The legacy IO layer now exposes a `BlLegacySoundReader` that resolves `ediM` entries from a
movie's resource table and classifies the decoded bytes into familiar audio container formats.
This mirrors how the original Director runtime bundled edited audio data: the actual sound stream
is saved inside the `ediM` entry while the surrounding `snd ` container only holds metadata or
index records. Once the reader inflates the payload (handling Afterburner compression when needed),
it inspects the file signature to label each sound with a `BlLegacySoundFormatKind`.

## Format classification

`BlLegacySoundFormat.Detect` performs a lightweight header inspection so higher layers can decide
how to replay or transcode the bytes. The classifier recognises:

- ID3 tags and MP3 frame sync words for MPEG Layer III streams.
- Big-endian RIFF variants (RIFF/FFIR/RIFX) for waveform data.
- FORM chunks that expand into AIFF or AIFC subtypes.
- Ogg, FLAC, AU, CAF, and MIDI signatures via their magic numbers.
- MPEG-4 audio when an `ftyp` box appears at the beginning of the buffer.

If no signature is found the loader reports the payload as `Unknown` so callers can fall back to
external probing tools.

## Historical notes from ScummVM research

Older Director releases stored audio differently, and understanding that history helps align our
reader with real-world movies:

- **Director 2–3:** sound members implicitly pointed at classic Macintosh `SND ` resources. The ID
  was derived from the cast member number, and playback relied on the platform decoder.
- **Director 4–5:** child resource entries were introduced, still targeting `snd ` or `SND ` tags but
  now explicitly listed inside the cast library.
- **Director 6:** Macromedia migrated to bundled MOA assets with `sndH`, `sndS`, and `ediM` pieces.
  A helper assembled these segments and interpreted compression or looping metadata before exposing
  audio to the mixer.

The ScummVM Director engine documents these transitions in detail and served as a reference when
verifying how Afterburner archives surface their sound payloads, especially for early projector
files that mix classic and MOA-era assets without consistent tagging.
