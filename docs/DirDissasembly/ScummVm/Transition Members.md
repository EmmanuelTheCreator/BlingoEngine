# Transition Cast Members

Transition cast members encapsulate stage wipes that can be triggered either directly from the score or by referencing a cast slot. The constructor reads a compact header for Director versions prior to 11, recording timing, chunk size, and a flag byte that determines whether the wipe affects the entire stage or a clipped region.[engines/director/castmember/transition.cpp (approx. lines 27-47)]

## Step 1 – Director 4–10 transition header (version < 0x1100)

The loader captures all usable metadata from six bytes of Director 4–10 cast data. Director 11 and newer movies are currently stubbed because their transition serialization has not been reverse engineered yet.[engines/director/castmember/transition.cpp (approx. lines 37-47)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<reserved>` | 1 byte | Legacy padding byte that is read and ignored before real fields begin.[engines/director/castmember/transition.cpp (approx. lines 38-39)] |
| `<chunk size>` | 1 byte | Number of tiles or strips processed per animation step; stored in `_chunkSize`.[engines/director/castmember/transition.cpp (approx. lines 38-44)] |
| `<transition type>` | 1 byte | Cast encodes the `TransitionType` enumeration; ScummVM casts it directly into `_transType`.[engines/director/castmember/transition.cpp (approx. lines 39-44)] |
| `<flags>` | 1 byte | Bitfield copied into `_flags`. Bit 0 flips `_area`, signaling whether the wipe covers the whole stage (0) or respects the score’s `transArea` bounds (1).[engines/director/castmember/transition.cpp (approx. lines 40-44)] |
| `<duration>` | 2 bytes | Big-endian duration in milliseconds stored in `_durationMillis`, later clamped to at least 250 ms when played.[engines/director/castmember/transition.cpp (approx. lines 41-44)][engines/director/transitions.cpp (approx. lines 102-115)] |

## Step 2 – Consuming transition metadata at runtime

The score activates transitions either from per-frame channel data or from a referenced cast member. In both cases the values above are passed straight to `Window::playTransition()`, which configures palette interpolation, tile counts, and chunk traversal using the recorded header.[engines/director/score.cpp (approx. lines 668-687)][engines/director/transitions.cpp (approx. lines 72-167)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<transType>` | 1 byte | Index into the transition property table that selects the animation algorithm and direction.[engines/director/transitions.cpp (approx. lines 59-118)] |
| `<chunkSize>` | 1 byte | Controls how many strips or pixels advance per frame when `Window::stepTransition()` iterates the effect.[engines/director/transitions.cpp (approx. lines 72-167)] |
| `<duration>` | 2 bytes | Combined with frame timing to compute `MAX_STEPS`, clamping fast transitions to 60 Hz updates.[engines/director/transitions.cpp (approx. lines 52-118)] |
| `<area>` | 1 byte | Propagated into `TransParams.area` so the renderer can limit the wipe to a rectangular region when requested by the score.[engines/director/score.cpp (approx. lines 668-687)][engines/director/transitions.cpp (approx. lines 120-160)] |

## Step 3 – CASt serialization helpers

`TransitionCastMember::getCastDataSize()` reports a six-byte payload for Director 5 projects, mirroring the fields above. Writing the data mirrors the read order, storing the same bytes in little-endian form for the final duration word.[engines/director/castmember/transition.cpp (approx. lines 113-135)]
