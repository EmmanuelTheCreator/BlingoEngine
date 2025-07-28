using LingoEngine.Director.Core.Scores;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Sounds;
using LingoEngine.Director.LGodot.Scores.Sounds;
using LingoEngine.Commands;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotAudioGridChannel : DirGodotTopGridChannel<ILingoSpriteAudioManager, DirGodotSoundSprite, LingoSpriteSound>
{

    protected override DirGodotSoundSprite CreateUISprite(LingoSpriteSound sprite) => new DirGodotSoundSprite();

    protected override ILingoSpriteAudioManager GetManager(LingoMovie movie) => movie.Audio;


    private readonly int _channel;

    public DirGodotAudioGridChannel(int channel, DirScoreGfxValues gfxValues, IDirectorEventMediator mediator, ILingoFrameworkFactory factory, ILingoCommandManager commandManager)
        : base(gfxValues, mediator, factory, commandManager)
    {
        _channel = channel;
    }


  

    //protected override bool HandleDrop(InputEventMouseButton mb)
    //{
    //    if (DirectorDragDropHolder.IsDragging && DirectorDragDropHolder.Member != null && _movie != null)
    //    {
    //        var pos = mb.Position;
    //        if (DirectorDragDropUtils.TryHandleSoundDrop(
    //                _movie,
    //                new Primitives.LingoPoint(pos.X, pos.Y),
    //                _gfxValues.ChannelHeight,
    //                _gfxValues.FrameWidth,
    //                ScrollX,
    //                out _, out int frame, out var sound))
    //        {
    //            _movie.AddAudioClip(_channel, frame, sound!);
    //            OnClipsChanged();
    //        }
    //        DirectorDragDropHolder.EndDrag();
    //        return true;
    //    }
    //    return false;
    //}
}
