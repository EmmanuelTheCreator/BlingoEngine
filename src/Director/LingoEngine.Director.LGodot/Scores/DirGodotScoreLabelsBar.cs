using Godot;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Scores;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Director.LGodot.Styles;
using LingoEngine.Primitives;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreLabelsBar : Control
{
    private readonly DirScoreLabelsBar _labelsBar;

    public DirGodotScoreLabelsBar(DirScoreGfxValues gfxValues, ILingoCommandManager commandManager, DirectorGodotStyle godotStyle, ILingoFrameworkFactory factory, ILingoMouse mouse)
    {
        _labelsBar = new DirScoreLabelsBar(gfxValues, factory, mouse, commandManager, new LingoPoint(0, 0));
        _labelsBar.RequestRedraw = () => QueueRedraw();
        AddChild((Node)_labelsBar.Canvas.FrameworkObj.FrameworkNode);
        var editNode = (Node)_labelsBar.EditField.FrameworkObj.FrameworkNode;
        AddChild(editNode);
        if (editNode is LineEdit le)
            le.Theme = godotStyle.GetLineEditTheme();
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public bool HeaderCollapsed
    {
        get => _labelsBar.HeaderCollapsed;
        set => _labelsBar.HeaderCollapsed = value;
    }

    public event Action<bool>? HeaderCollapseChanged
    {
        add => _labelsBar.HeaderCollapseChanged += value;
        remove => _labelsBar.HeaderCollapseChanged -= value;
    }

    public void ToggleCollapsed() => _labelsBar.ToggleCollapsed();

    public void SetMovie(LingoMovie? movie) => _labelsBar.SetMovie(movie);

    internal void UpdatePosition(Vector2 position)
    {
        _labelsBar.UpdatePosition(position.ToLingoPoint());
    }

    public override void _Draw()
    {
        _labelsBar.Draw();
        Size = new Vector2(_labelsBar.Width, _labelsBar.Height);
        CustomMinimumSize = Size;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _labelsBar.Dispose();
        base.Dispose(disposing);
    }
}
