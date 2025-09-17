using System;
using AbstUI.Inputs;
using BlingoEngine.Blazor.Inputs;
using BlingoEngine.Inputs;
using AbstUI.Primitives;

namespace BlingoEngine.Blazor;

public abstract class BlazorRootContext<TMouse> : IDisposable
    where TMouse : IAbstMouse
{
    public BlingoBlazorKey Key { get; set; } = null!;
    public IAbstFrameworkMouse Mouse { get; set; } = null!;

    public IAbstKey AbstKey { get; protected set; } = null!;
    public IAbstMouse AbstMouse { get; set; } = null!;

    protected BlazorRootContext()
    {
    }

    public virtual APoint GetWindowSize() => new APoint(0, 0);

    public virtual void Dispose()
    {
    }
}

