using Godot;//
using LingoEngine.Movies;
using LingoEngine.Director.Core.Scores;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Tools;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreLeftChannelsContainer : Control
{
    private DirScoreLeftChannelsContainer _headerContainer;
    private DirScoreGfxValues _gfxValues;
    private LingoMovie? _movie;


    public DirGodotScoreLeftChannelsContainer(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, ILingoMouse mouse, Vector2 position, IDirectorEventMediator mediator)
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Position = position;
        _gfxValues = gfxValues;
        ClipContents = true;
        _headerContainer = new DirScoreLeftChannelsContainer(gfxValues, factory, mouse, new LingoPoint(0, 0), mediator);
        _headerContainer.RequestRedraw = RequestRedraw;
        var node = (Node)_headerContainer.FrameworkGfxNode.FrameworkNode;
        AddChild(node);
    }

    private void RequestRedraw()
    {
        UpdateSize();
    }

    public void SetMovie(LingoMovie? movie)
    {
        if (_movie != null)
            _movie.SpriteListChanged -= OnSpriteListChanged;
        _movie = movie;
        if (_movie != null)
            _movie.SpriteListChanged += OnSpriteListChanged;
        _headerContainer.SetMovie(movie);
        UpdateSize();
    }

    private void UpdateSize()
    {
        Size = new Vector2(_gfxValues.ChannelInfoWidth, _headerContainer.Height);
        CustomMinimumSize = new Vector2(_gfxValues.ChannelInfoWidth, _headerContainer.Height);
        QueueRedraw();
    }

    private void OnSpriteListChanged()
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_movie == null) return;
        _headerContainer.Draw();
    }

    internal void UpdatePosition(Vector2 newPos, float topY)
    {
        Position = newPos;
        _headerContainer.UpdatePosition(new LingoPoint(0, newPos.Y + topY));
    }
}
