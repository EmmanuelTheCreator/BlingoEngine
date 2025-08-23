using System;

namespace LingoEngine.Transitions.TransitionLibrary;

public abstract class LingoBaseTransition
{
    public int Id { get; }
    public string Name { get; }
    public string Code { get; }
    public string Description { get; }

    protected LingoBaseTransition(int id, string name, string code, string description)
    {
        Id = id;
        Name = name;
        Code = code;
        Description = description;
    }

    public abstract byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress);
}
