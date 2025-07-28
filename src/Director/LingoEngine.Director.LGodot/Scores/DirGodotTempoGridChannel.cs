using Godot;
using System.Collections.Generic;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Scores;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotTempoGridChannel : DirGodotTopGridChannel<DirGodotTempoSprite>
{
    private readonly AcceptDialog _dialog = new();
    private readonly HSlider _slider = new();
    private int _editFrame;

    public DirGodotTempoGridChannel(DirScoreGfxValues gfxValues)
        : base(gfxValues)
    {
        _dialog.Title = "Tempo";
        _slider.MinValue = 1;
        _slider.MaxValue = 60;
        _slider.Step = 1;
        _dialog.AddChild(_slider);
        _dialog.GetOkButton().Pressed += OnDialogOk;
        AddChild(_dialog);
    }

    protected override IEnumerable<DirGodotTempoSprite> BuildSprites(LingoMovie movie)
    {
        foreach (var k in movie.GetTempoKeyframes())
            yield return new DirGodotTempoSprite(k);
    }

    protected override void MoveSprite(DirGodotTempoSprite sprite, int oldFrame, int newFrame)
    {
        _movie!.MoveTempoKeyframe(oldFrame, newFrame);
    }

    protected override void SubscribeMovie(LingoMovie movie)
    {
        movie.TempoKeyframesChanged += OnKeyframesChanged;
    }

    protected override void UnsubscribeMovie(LingoMovie movie)
    {
        movie.TempoKeyframesChanged -= OnKeyframesChanged;
    }

    private void OnKeyframesChanged()
    {
        if (_movie == null) return;
        _sprites.Clear();
        foreach (var k in _movie.GetTempoKeyframes())
            _sprites.Add(new DirGodotTempoSprite(k));
        MarkDirty();
    }

    protected override void OnDoubleClick(int frame, DirGodotTempoSprite? sprite)
    {
        _editFrame = frame;
        _slider.Value = sprite?.Keyframe.Fps ?? _movie!.Tempo;
        _dialog.PopupCentered(new Vector2I(200, 80));
    }

    private void OnDialogOk()
    {
        _movie!.AddTempoKeyFrame(_editFrame, (int)_slider.Value);
        MarkDirty();
    }
}
