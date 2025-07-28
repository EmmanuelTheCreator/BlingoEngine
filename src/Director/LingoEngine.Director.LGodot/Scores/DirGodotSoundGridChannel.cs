using Godot;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.Director.Core.Scores;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sounds;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotSoundGridChannel : DirGodotTopGridChannel<DirGodotScoreAudioClip>
{
    private readonly int _channel;
    private readonly IDirectorEventMediator _mediator;

    public DirGodotSoundGridChannel(int channel, DirScoreGfxValues gfxValues, IDirectorEventMediator mediator)
        : base(gfxValues)
    {
        _channel = channel;
        _mediator = mediator;
    }

    protected override IEnumerable<DirGodotScoreAudioClip> BuildSprites(LingoMovie movie)
    {
        foreach (var clip in movie.GetAudioClips())
            if (clip.Channel == _channel)
                yield return new DirGodotScoreAudioClip(clip);
    }

    protected override void MoveSprite(DirGodotScoreAudioClip sprite, int oldFrame, int newFrame)
    {
        _movie!.MoveAudioClip(sprite.Clip, newFrame);
    }

    protected override void SubscribeMovie(LingoMovie movie)
    {
        movie.AudioClipListChanged += OnClipsChanged;
    }

    protected override void UnsubscribeMovie(LingoMovie movie)
    {
        movie.AudioClipListChanged -= OnClipsChanged;
    }

    private void OnClipsChanged()
    {
        if (_movie == null) return;
        _sprites.Clear();
        foreach (var clip in _movie.GetAudioClips())
            if (clip.Channel == _channel)
                _sprites.Add(new DirGodotScoreAudioClip(clip));
        MarkDirty();
    }

    protected override void OnDoubleClick(int frame, DirGodotScoreAudioClip? sprite)
    {
        // audio channels don't create clips on double click
    }

    protected override void OnSpriteClicked(DirGodotScoreAudioClip sprite)
    {
        _mediator.RaiseMemberSelected(sprite.Clip.Sound);
    }

    protected override bool HandleDrop(InputEventMouseButton mb)
    {
        if (DirectorDragDropHolder.IsDragging && DirectorDragDropHolder.Member != null && _movie != null)
        {
            var pos = mb.Position;
            if (DirectorDragDropUtils.TryHandleSoundDrop(
                    _movie,
                    new LingoEngine.Primitives.LingoPoint(pos.X, pos.Y),
                    _gfxValues.ChannelHeight,
                    _gfxValues.FrameWidth,
                    ScrollX,
                    out _, out int frame, out var sound))
            {
                _movie.AddAudioClip(_channel, frame, sound!);
                OnClipsChanged();
            }
            DirectorDragDropHolder.EndDrag();
            return true;
        }
        return false;
    }
}
