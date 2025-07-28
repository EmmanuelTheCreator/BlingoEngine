using Godot;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Header control for a single audio channel with mute toggle.
/// </summary>
internal partial class DirGodotSoundHeader : DirGodotTopChannelHeader
{
    private readonly int _channel;
    private bool _muted;

    public DirGodotSoundHeader(int channel, DirScoreGfxValues gfxValues) : base(gfxValues)
    {
        _channel = channel;
    }

    protected override string Icon => _muted ? "🔇" : "🔊";
    protected override string Name => (_channel + 1).ToString();

    public bool IsMuted => _muted;

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Position.X >= 12 && mb.Position.X <= 28)
                ToggleMute();
        }
    }

    public override void SetMovie(LingoMovie? movie)
    {
        base.SetMovie(movie);
        _muted = false;
    }

    private void ToggleMute()
    {
        if (_movie == null) return;
        _muted = !_muted;
        var chObj = _movie.GetEnvironment().Sound.Channel(_channel + 1);
        chObj.Volume = _muted ? 0 : 255;
        QueueRedraw();
    }
}
