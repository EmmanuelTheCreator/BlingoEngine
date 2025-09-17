using System;

namespace BlingoEngine.Transitions.TransitionLibrary;

public abstract class BlingoBaseTransition
{
    public int Id { get; }
    public string Name { get; }
    public string Code { get; }
    public string Description { get; }
    public string Category { get; }

    protected BlingoBaseTransition(int id, string name, string code, string description, string category)
    {
        Id = id;
        Name = name;
        Code = code;
        Description = description;
        Category = category;
    }

    public abstract byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress);
}

