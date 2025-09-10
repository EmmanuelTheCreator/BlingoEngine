using System;

namespace LingoEngine.Sprites.BehaviorLibrary;

public class LingoBehaviorDefinition
{
    public string Name { get; }
    public Type BehaviorType { get; }
    public string Category { get; }
    public string? Icon { get; }

    public LingoBehaviorDefinition(string name, Type behaviorType, string category, string? icon = null)
    {
        Name = name;
        BehaviorType = behaviorType;
        Category = category;
        Icon = icon;
    }
}
