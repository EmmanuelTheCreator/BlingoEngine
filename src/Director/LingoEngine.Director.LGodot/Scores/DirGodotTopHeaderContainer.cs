using Godot;
using LingoEngine.Director.Core.Scores;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotTopHeaderContainer : Control
{
    private readonly DirScoreTopHeaderContainer _lingoContainer;


    public DirGodotTopHeaderContainer(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, ILingoMouse mouse, Vector2 position)
    {
        Position = position;
        _lingoContainer = new DirScoreTopHeaderContainer(gfxValues, factory, mouse, Position.ToLingoPoint());
        AddChild((Node)_lingoContainer.FrameworkGfxNode.FrameworkNode);
        QueueRedraw();
    }
    protected override void Dispose(bool disposing)
    {
        RemoveChild((Node)_lingoContainer.FrameworkGfxNode.FrameworkNode);
        base.Dispose(disposing);
    }

    public bool Collapsed
    {
        get => _lingoContainer.Collapsed;
        set
        {
            _lingoContainer.Collapsed = value;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        base._Draw();
        Size = new Vector2(_lingoContainer.Width, _lingoContainer.Height);
        CustomMinimumSize = Size;
        _lingoContainer.Draw();
    }


    public void SetMovie(LingoMovie? movie) => _lingoContainer.SetMovie(movie);


}
