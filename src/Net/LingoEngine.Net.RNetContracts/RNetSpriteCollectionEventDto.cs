namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Describes a change to a sprite manager's collection.
/// </summary>
/// <param name="Manager">Name of the sprite manager.</param>
/// <param name="Event">Type of collection change.</param>
/// <param name="SpriteNum">Sprite number affected; unused for cleared events.</param>
/// <param name="Sprite">Full sprite state for added events; otherwise null.</param>
public sealed record RNetSpriteCollectionEventDto(string Manager, RNetSpriteCollectionEventType Event, int SpriteNum, RNetSpriteDto? Sprite);

/// <summary>
/// Types of changes that can occur in a sprite collection.
/// </summary>
public enum RNetSpriteCollectionEventType
{
    /// <summary>A sprite was added to the collection.</summary>
    Added,

    /// <summary>A sprite was removed from the collection.</summary>
    Removed,

    /// <summary>The collection was cleared.</summary>
    Cleared
}
