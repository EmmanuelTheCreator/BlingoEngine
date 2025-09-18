namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Represents a relationship between a parent resource and one of its child chunks.
/// </summary>
/// <param name="ChildId">Identifier of the child resource.</param>
/// <param name="ParentId">Identifier of the parent resource.</param>
/// <param name="Tag">Tag of the child resource recorded in the key table.</param>
public readonly record struct BlResourceKeyLink(int ChildId, int ParentId, BlTag Tag);
