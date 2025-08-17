using System;
using AbstUI.Inputs;
using LingoEngine.Blazor.Inputs;
using LingoEngine.Inputs;
using AbstUI.Primitives;

namespace LingoEngine.Blazor;

public abstract class BlazorRootContext<TMouse> : IDisposable
    where TMouse : IAbstMouse
{
    public LingoBlazorKey Key { get; set; } = null!;
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
