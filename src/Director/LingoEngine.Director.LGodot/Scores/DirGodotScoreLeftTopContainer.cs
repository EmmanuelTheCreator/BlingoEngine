using Godot;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreLeftTopContainer : Control
{
    private readonly DirScoreLeftTopContainer _lingoContainer;


    public DirGodotScoreLeftTopContainer(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, ILingoMouse mouse, Vector2 position, IDirectorEventMediator mediator)
    {
        Position = position;
        _lingoContainer = new DirScoreLeftTopContainer(gfxValues, factory, mouse, Position.ToLingoPoint(), mediator);
        _lingoContainer.RequestRedraw = () => QueueRedraw();
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
        _lingoContainer.Draw();
        Size = new Vector2(_lingoContainer.Width, _lingoContainer.Height);
        CustomMinimumSize = Size;
    }


    public void SetMovie(LingoMovie? movie) => _lingoContainer.SetMovie(movie);


}
