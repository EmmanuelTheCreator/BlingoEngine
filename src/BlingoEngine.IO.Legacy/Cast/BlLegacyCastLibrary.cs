
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
    public List<BlLegacyCastMemberSlot> MemberSlots { get; } = new();
}
