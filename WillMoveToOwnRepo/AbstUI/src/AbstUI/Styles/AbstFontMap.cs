using System.Collections.Generic;
using AbstUI.Primitives;

namespace AbstUI.Styles;

/// <summary>
/// Represents a mapping between two platform font names as defined by Director font map files.
/// </summary>
public sealed record class AbstFontMap
{
    public AbstFontMap(
        ARuntimePlatform sourcePlatform,
        string sourceFontName,
        ARuntimePlatform targetPlatform,
        string targetFontName,
        bool mapCharacters,
        IReadOnlyDictionary<int, int>? sizeMappings = null)
    {
        SourcePlatform = sourcePlatform;
        SourceFontName = sourceFontName;
        TargetPlatform = targetPlatform;
        TargetFontName = targetFontName;
        MapCharacters = mapCharacters;
        SizeMappings = sizeMappings != null
            ? new Dictionary<int, int>(sizeMappings)
            : new Dictionary<int, int>();
    }

    /// <summary>Origin platform (e.g., "Mac" or "Win").</summary>
    public ARuntimePlatform SourcePlatform { get; }

    /// <summary>Font name declared for the origin platform.</summary>
    public string SourceFontName { get; }

    /// <summary>Destination platform (e.g., "Win" or "Mac").</summary>
    public ARuntimePlatform TargetPlatform { get; }

    /// <summary>Font name used on the destination platform.</summary>
    public string TargetFontName { get; }

    /// <summary>Whether characters should be remapped when this font is applied.</summary>
    public bool MapCharacters { get; }

    /// <summary>
    /// Optional point-size remappings (e.g., 14 => 12) that apply to this font pairing.
    /// </summary>
    public IReadOnlyDictionary<int, int> SizeMappings { get; }
}

/// <summary>
/// Represents a character-code translation between two platforms.
/// </summary>
public sealed record class AbstInputKeyMap
{
    public AbstInputKeyMap(
        ARuntimePlatform sourcePlatform,
        ARuntimePlatform targetPlatform,
        IReadOnlyDictionary<int, int>? keys = null)
    {
        SourcePlatform = sourcePlatform;
        TargetPlatform = targetPlatform;
        Keys = keys != null
            ? new Dictionary<int, int>(keys)
            : new Dictionary<int, int>();
    }

    /// <summary>Origin platform (e.g., "Mac" or "Win").</summary>
    public ARuntimePlatform SourcePlatform { get; }

    /// <summary>Destination platform (e.g., "Win" or "Mac").</summary>
    public ARuntimePlatform TargetPlatform { get; }

    /// <summary>Character-code mappings keyed by the origin code.</summary>
    public IReadOnlyDictionary<int, int> Keys { get; }
}
