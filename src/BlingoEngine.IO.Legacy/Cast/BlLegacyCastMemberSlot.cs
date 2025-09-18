namespace BlingoEngine.IO.Legacy.Cast;

/// <summary>
/// Describes a single populated entry inside the <c>CAS*</c> table. The slot index represents the
/// position within the table while the resource identifier points to the <c>CASt</c> chunk that
/// contains the cast-member data.
/// </summary>
/// <param name="SlotIndex">Zero-based position of the entry within the table.</param>
/// <param name="ResourceId">Identifier of the <c>CASt</c> resource referenced by the slot.</param>
/// <param name="MemberType">Type of cast member stored inside the <c>CASt</c> payload.</param>
/// <param name="Name">Name recorded in the member info block, when available.</param>
internal readonly record struct BlLegacyCastMemberSlot(int SlotIndex, int ResourceId, BlLegacyCastMemberType MemberType, string Name);
