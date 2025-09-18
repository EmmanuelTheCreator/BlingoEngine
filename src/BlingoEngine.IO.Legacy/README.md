# BlingoEngine.IO.Legacy

`BlingoEngine.IO.Legacy` loads classic Director assets (`.dir`, `.cst`, `.dcr/.dxr`) and projects them into the
`BlingoEngine.IO.Data` DTO set. The pipeline mirrors the original byte layout while keeping the architecture clean and
extension-friendly.

## Highlights

- Shared `ReaderContext` drives parsing for Director versions ranging from early classic builds to Afterburner projectors.
- Dedicated readers translate classic (`imap`, `mmap`, `KEY*`) and Afterburner (`Fver`, `Fcdr`, `ABMP`, `FGEI`) metadata streams.
- Endian-aware `BlStreamReader` offers efficient helpers for fixed and variable-width integers as well as FourCC tags.

## Contributing

Before adding new features or refactors, read the
[legacy coding guidelines](./LEGACY_CODING_GUIDELINES.md) for expectations around naming, extension methods, context usage, and
single-responsibility design.

The existing implementation is intentionally free of direct code copies from other projects. Use the disassembly documentation and
ProjectorRays notes only as background material when designing clean-room implementations.
