using System;

namespace LingoEngine.Lingo.Core;

/// <summary>
/// Configuration for Lingo to C# class name generation.
/// </summary>
public class LingoToCSharpConverterSettings
{
    public string BehaviorSuffix { get; set; } = "Behavior";
    public string ParentSuffix { get; set; } = "Parent";
    public string MovieScriptSuffix { get; set; } = "MovieScript";
}
