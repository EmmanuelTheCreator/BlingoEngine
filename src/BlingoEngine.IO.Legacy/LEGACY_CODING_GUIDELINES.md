# BlingoEngine.IO.Legacy Coding Guidelines

These notes capture the conventions that drive the legacy Director reader. Follow them whenever you extend the project so the
pipeline stays consistent and easy to reason about.

## Architectural Principles

- **One responsibility per type.** Keep parsing, DTO projection, and orchestration in separate classes. A method named `Read`
  should only read bytes and return data (records/tuples), leaving the caller to apply the results.
- **Domain-driven grouping.** Place files next to their feature (Afterburner, Classic, Compression, etc.). Avoid generic "Data"
  dumps where behaviours are mixed together.
- **Immutable DTO flow.** Everything ultimately maps into `BlingoEngine.IO.Data` DTOs. Preserve the read-only nature of those
  models and avoid mutating them after creation.

## Naming Rules

- Use the `BlBlockXXXX` pattern for block models (e.g., `BlBlockMmap`, `BlBlockImap`). Keep payload rows or entries in separate
  types inside the same file when needed.
- Keep helper extensions in a sibling class (e.g., `BlBlockMmapExtensions`) rather than mixing them with the data type.
- Use `Bl` prefixes for public types exposed outside the project and keep internal helpers scoped appropriately.

## ReaderContext Usage

- Always work through the shared `ReaderContext`. It holds the base stream, endian-aware `BlStreamReader`, compression catalog,
  resource table, and the current `BlAfterburnerState`.
- Reset the context via `ResetRegistries` before parsing a new container and use helpers such as `AddResource`,
  `SetResourceInlineSegment`, `AddResourceRelationship`, and `AddCompression` rather than mutating the underlying collections.
- Store the Afterburner bookkeeping with `SetAfterburnerState` so payload loaders can retrieve it without extra return values.

## Extension Method Strategy

- Provide extension methods for orchestration entry points: `_context.ReadAfterburner(dataBlock)`, `dataBlock.ReadMapData(context)`,
  `_context.ReadDirFilesContainer()`, etc. This keeps call-sites expressive while the underlying types remain focused on parsing.
- Keep extension classes near the feature they expose (Afterburner, Classic, Compression, etc.).

## Stream Handling

- Use `BlStreamReader` for all byte operations. It supports both endian modes and offers helpers for variable-width integers and
  FourCC tags.
- Do not duplicate stream logic from external projects (e.g., ScummVM or ProjectorRays). Translate the intent into clean,
  well-tested C# that matches these conventions.

## Documentation

- Document public APIs with XML comments that describe byte layouts, offsets, and block semantics.
- When copying explanations from reference material, omit references to other projects and keep the focus on the binary
  structure itself.

Following these guidelines keeps the legacy loader maintainable while letting us support every Director version cleanly.
