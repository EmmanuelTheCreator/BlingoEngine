using LingoEngine.Blazor.Movies;
using LingoEngine.Stages;

namespace LingoEngine.Blazor.Stages;

/// <summary>
/// Simple stage container that assigns the active Blazor stage
/// to the root panel.
/// </summary>
public class LingoBlazorStageContainer : ILingoFrameworkStageContainer
{
    private readonly LingoBlazorRootPanel _root;

    public LingoBlazorStageContainer(LingoBlazorRootPanel root)
    {
        _root = root;
    }

    /// <inheritdoc />
    public void SetStage(ILingoFrameworkStage stage)
    {
        if (stage is LingoBlazorStage blazorStage)
            _root.Stage = blazorStage;
    }
}

