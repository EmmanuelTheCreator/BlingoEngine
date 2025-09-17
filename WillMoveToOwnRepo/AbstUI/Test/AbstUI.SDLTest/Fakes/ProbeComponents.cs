using System.Collections.Generic;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Core;

namespace AbstUI.SDLTest.Fakes;

internal sealed class ProbePanel : AbstPanel
{
    public ProbeSdlPanel Impl { get; }

    public ProbePanel(AbstSdlComponentFactory factory, string name)
        : base(factory)
    {
        Impl = new ProbeSdlPanel(factory)
        {
            Name = name,
        };
        Init(Impl);
        Name = name;
    }
}

internal sealed class ProbeWrapPanel : AbstWrapPanel
{
    public ProbeSdlWrapPanel Impl { get; }

    public ProbeWrapPanel(AbstSdlComponentFactory factory, AOrientation orientation, string name)
        : base(factory)
    {
        Impl = new ProbeSdlWrapPanel(factory, orientation)
        {
            Name = name,
        };
        Init(Impl);
        Name = name;
    }
}

internal sealed class ProbeSdlPanel : AbstSdlPanel
{
    public List<RenderSnapshot> RenderSnapshots { get; } = new();

    public ProbeSdlPanel(AbstSdlComponentFactory factory)
        : base(factory)
    {
    }

    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        RenderSnapshots.Add(RenderSnapshot.FromComponent(this));
        return base.Render(context);
    }
}

internal sealed class ProbeSdlWrapPanel : AbstSdlWrapPanel
{
    public List<RenderSnapshot> RenderSnapshots { get; } = new();

    public ProbeSdlWrapPanel(AbstSdlComponentFactory factory, AOrientation orientation)
        : base(factory, orientation)
    {
    }

    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        RenderSnapshots.Add(RenderSnapshot.FromComponent(this));
        return base.Render(context);
    }
}
