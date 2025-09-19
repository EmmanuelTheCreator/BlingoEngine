using System.Collections.Generic;

namespace BlingoEngine.Director.Core.Importer;

/// <summary>
/// Describes a single annotated range in a binary stream for visualization purposes.
/// </summary>
public sealed class BinaryAnnotation
{
    public long Address { get; set; }
    public int Length { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Keys { get; } = new();
}

/// <summary>
/// Container of annotated regions associated with a binary buffer.
/// </summary>
public sealed class BinaryAnnotationSet
{
    public long StreamOffsetBase { get; set; }
    public List<BinaryAnnotation> Annotations { get; } = new();
}
