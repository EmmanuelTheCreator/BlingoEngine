using System;

namespace BlingoEngine.Sprites.BehaviorLibrary;

public class BlingoBehaviorDefinition
{
    public string Name { get; }
    public Type BehaviorType { get; }
    public string Category { get; }
    public string? Icon { get; }

    public BlingoBehaviorDefinition(string name, Type behaviorType, string category, string? icon = null)
    {
        Name = name;
        BehaviorType = behaviorType;
        Category = category;
        Icon = icon;
    }
}

