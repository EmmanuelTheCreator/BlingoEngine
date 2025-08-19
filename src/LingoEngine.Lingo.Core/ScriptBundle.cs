using System.Collections.Generic;

namespace LingoEngine.Lingo.Core;

/// <summary>
/// Represents a single Lingo script file with its type.
/// </summary>
public class LingoScriptFile
{
    public required string Name { get; init; }
    public required string Source { get; init; }
    public LingoScriptType Type { get; init; }
}

/// <summary>
/// Specifies the kind of script a file contains.
/// </summary>
public enum LingoScriptType
{
    Parent,
    Behavior,
    Movie
}

/// <summary>
/// Result of a batch conversion.
/// </summary>
public class LingoBatchResult
{
    public Dictionary<string, string> ConvertedScripts { get; } = new();
    public HashSet<string> CustomMethods { get; } = new();
    public Dictionary<string, List<MethodSignature>> Methods { get; } = new();
    public Dictionary<string, List<PropertyInfo>> Properties { get; } = new();
}

/// <summary>
/// Represents a converted method signature.
/// </summary>
public record MethodSignature(string Name, List<ParameterInfo> Parameters);

/// <summary>
/// Represents a parameter detected during conversion.
/// </summary>
public record ParameterInfo(string Name, string Type);

/// <summary>
/// Represents a property detected during conversion.
/// </summary>
public record PropertyInfo(string Name, string Type);
