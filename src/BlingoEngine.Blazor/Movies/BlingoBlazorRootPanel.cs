using AbstUI.Components;
using BlingoEngine.Blazor.Stages;
using AbstUI.Blazor.Components.Containers;

namespace BlingoEngine.Blazor.Movies;

/// <summary>
/// Holds the root AbstUI panel used to compose the Blazor stage.
/// </summary>
public class BlingoBlazorRootPanel
{
    public AbstBlazorPanelComponent Component { get; }
    public BlingoBlazorStage? Stage { get; set; }

    public BlingoBlazorRootPanel(IAbstComponentFactory factory)
    {
        Component = factory.CreatePanel("BlingoRoot").Framework<AbstBlazorPanelComponent>();
    }
}

