using AbstUI.Components;
using LingoEngine.Blazor.Stages;
using AbstUI.Blazor.Components.Containers;

namespace LingoEngine.Blazor.Movies;

/// <summary>
/// Holds the root AbstUI panel used to compose the Blazor stage.
/// </summary>
public class LingoBlazorRootPanel
{
    public AbstBlazorPanelComponent Component { get; }
    public LingoBlazorStage? Stage { get; set; }

    public LingoBlazorRootPanel(IAbstComponentFactory factory)
    {
        Component = factory.CreatePanel("LingoRoot").Framework<AbstBlazorPanelComponent>();
    }
}
