using Godot;//
using LingoEngine.Movies;
using LingoEngine.Director.Core.Scores;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Primitives;
using LingoEngine.Gfx;
using LingoEngine.LGodot.Primitives;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreChannelBar : Control
{
    private DirScoreChannelsHeaderContainer _headerContainer;
    private DirScoreGfxValues _gfxValues;
    private LingoMovie? _movie;


    public DirGodotScoreChannelBar(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, ILingoMouse mouse, Vector2 position)
    {
        Position = position;
        _gfxValues = gfxValues;
        ClipContents = true;
        _headerContainer = new DirScoreChannelsHeaderContainer(gfxValues, factory, mouse, new LingoPoint(0, 0));
        AddChild((Node)_headerContainer.FrameworkGfxNode.FrameworkNode);
    }

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        _headerContainer.SetMovie(movie);
        Size = new Vector2(_gfxValues.ChannelInfoWidth, _headerContainer.Height);
        CustomMinimumSize = new Vector2(_gfxValues.ChannelInfoWidth, _headerContainer.Height);
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

    //public override void _Process(double delta)
    //{
    //    QueueRedraw();
    //}

    //public override void _GuiInput(InputEvent @event)
    //{
    //    if (_movie == null || !Visible) return;

    //    if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
    //    {
    //        Vector2 pos = GetLocalMousePosition()+ new Vector2(0, _gfxValues.ChannelHeight);
    //        int channel = (int)(pos.Y / _gfxValues.ChannelHeight);
    //        if (channel >= 0 && channel < _movie.MaxSpriteChannelCount && pos.X >= 0 && pos.X < _gfxValues.ChannelHeight)
    //        {
    //            var ch = _movie.Channel(channel);
    //            ch.Visibility = !ch.Visibility;
    //            QueueRedraw();
    //        }
    //    }
    //}
}
