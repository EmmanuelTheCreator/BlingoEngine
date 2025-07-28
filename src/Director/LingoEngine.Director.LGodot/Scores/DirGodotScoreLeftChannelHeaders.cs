using Godot;//
using LingoEngine.Movies;
using LingoEngine.Director.Core.Scores;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Primitives;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreLeftChannelHeaders : Control
{
    private DirScoreChannelsHeaderContainer _headerContainer;
    private DirScoreGfxValues _gfxValues;
    private LingoMovie? _movie;


    public DirGodotScoreLeftChannelHeaders(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, ILingoMouse mouse, Vector2 position)
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Position = position;
        _gfxValues = gfxValues;
        ClipContents = true;
        _headerContainer = new DirScoreChannelsHeaderContainer(gfxValues, factory, mouse, new LingoPoint(0, 0));
        var node = (Node)_headerContainer.FrameworkGfxNode.FrameworkNode;
        AddChild(node);
    }

    public void SetMovie(LingoMovie? movie)
    {
        if (_movie != null)
            _movie.SpriteListChanged -= OnSpriteListChanged;
        _movie = movie;
        if (_movie != null)
            _movie.SpriteListChanged += OnSpriteListChanged;
        _headerContainer.SetMovie(movie);
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
