using BlingoEngine.Blazor.Movies;
using BlingoEngine.Stages;

namespace BlingoEngine.Blazor.Stages;

/// <summary>
/// Simple stage container that assigns the active Blazor stage
/// to the root panel.
/// </summary>
public class BlingoBlazorStageContainer : IBlingoFrameworkStageContainer
{
    private readonly BlingoBlazorRootPanel _root;

    public BlingoBlazorStageContainer(BlingoBlazorRootPanel root)
    {
        _root = root;
    }

    /// <inheritdoc />
    public void SetStage(IBlingoFrameworkStage stage)
    {
        if (stage is BlingoBlazorStage blazorStage)
            _root.Stage = blazorStage;
    }
}


