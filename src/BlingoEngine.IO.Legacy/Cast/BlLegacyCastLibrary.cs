using System.Collections.Generic;

namespace BlingoEngine.IO.Legacy.Cast;

/// <summary>
/// Represents the table of cast-member resource identifiers stored inside a <c>CAS*</c> chunk.
/// Each slot mirrors the four-byte entries Director wrote to reference individual <c>CASt</c>
/// members that belong to the owning cast library.
/// </summary>
internal sealed class BlLegacyCastLibrary
{
    public BlLegacyCastLibrary(int resourceId, int? libraryId, int entryCount)
    {
        ResourceId = resourceId;
        LibraryId = libraryId;
        EntryCount = entryCount;
    }

    /// <summary>
    /// Gets the resource identifier assigned to the <c>CAS*</c> table in the map.
    /// </summary>
    public int ResourceId { get; }

    /// <summary>
    /// Gets the parent resource identifier recorded in the <c>KEY*</c> table. When present this
    /// value identifies the cast library that owns the <c>CAS*</c> table.
    /// </summary>
    public int? LibraryId { get; }

    /// <summary>
    /// Gets the number of four-byte slots stored in the <c>CAS*</c> payload (including empty slots).
    /// </summary>
    public int EntryCount { get; }

    /// <summary>
    /// Gets the list of populated cast-member slots. Empty slots are omitted but their original
    /// index is preserved so consumers can reconstruct member numbering.
    /// </summary>
    public List<BlLegacyCastMember> Members { get; } = new();
}

/// <summary>
/// Describes a single populated entry inside the <c>CAS*</c> table. The slot index represents the
/// position within the table while the resource identifier points to the <c>CASt</c> chunk that
/// contains the cast-member data.
/// </summary>
/// <param name="SlotIndex">Zero-based position of the entry within the table.</param>
/// <param name="ResourceId">Identifier of the <c>CASt</c> resource referenced by the slot.</param>
/// <param name="MemberType">Type of cast member stored inside the <c>CASt</c> payload.</param>
/// <param name="Name">Name recorded in the member info block, when available.</param>
internal readonly record struct BlLegacyCastMember(int SlotIndex, int ResourceId, BlLegacyCastMemberType MemberType, string Name);

/// <summary>
/// Enumerates the legacy cast-member types encoded at the start of the <c>CASt</c> payload.
/// </summary>
internal enum BlLegacyCastMemberType
{
    Unknown = -1,
    Null = 0,
    Bitmap = 1,
    FilmLoop = 2,
    Text = 3,
    Palette = 4,
    Picture = 5,
    Sound = 6,
    Button = 7,
    Shape = 8,
    Movie = 9,
    DigitalVideo = 10,
    Script = 11,
    Rte = 12,
    Font = 13,
    Xtra = 14,
    Field = 15
}
