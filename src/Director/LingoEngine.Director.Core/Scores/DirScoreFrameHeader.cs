using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Styles;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Inputs;

namespace LingoEngine.Director.Core.Scores;

public class DirScoreFrameHeader : IDisposable
{
    private LingoMovie? _movie;
    private readonly DirScoreGfxValues _gfxValues;
    private readonly AbstUIGfxCanvas _canvas;

    public AbstUIGfxCanvas Canvas => _canvas;

    public DirScoreFrameHeader(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory)
    {
        _gfxValues = gfxValues;
        _canvas = factory.CreateGfxCanvas("ScoreFrameHeader", 0, (int)gfxValues.ChannelFramesHeight);
    }

    public void Dispose() => _canvas.Dispose();

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        var width = _gfxValues.LeftMargin + (_movie?.FrameCount ?? 0) * _gfxValues.FrameWidth;
        _canvas.Width = width;
        _canvas.Height = _gfxValues.ChannelFramesHeight;
        Draw();
    }

    private void Draw()
    {
        _canvas.Clear(DirectorColors.BG_WhiteMenus);
        if (_movie == null) return;

        int frameCount = _movie.FrameCount;
        // draw first frame number
        _canvas.DrawText(new APoint(1, 13), "1", null, AColors.Gray, 10);
        for (int f = 0; f <= frameCount; f++)
        {
            float x = _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            if ((f + 1) % 5 == 0)
                _canvas.DrawText(new APoint(x + 1, 13), (f + 1).ToString(), null, AColors.Gray, 10);
        }
        _canvas.DrawLine(new APoint(0, 0), new APoint(_canvas.Width, 0), DirectorColors.LineLight, 1);
        _canvas.DrawLine(new APoint(0, _gfxValues.ChannelFramesHeight), new APoint(_canvas.Width, _gfxValues.ChannelFramesHeight), DirectorColors.LineDark, 1);
    }

    public void HandleMouseEvent(LingoMouseEvent mouseEvent, int mouseFrame)
    {
        if (_movie == null)
            return;

        bool isDragging = mouseEvent.Mouse.LeftMouseDown &&
            (mouseEvent.Type == AbstUIMouseEventType.MouseDown || mouseEvent.Type == AbstUIMouseEventType.MouseMove);

        if (isDragging && mouseFrame >= 1 && mouseFrame <= _movie.FrameCount)
        {
            if (_movie.IsPlaying)
                _movie.GoTo(mouseFrame);
            else
                _movie.GoToAndStop(mouseFrame);
        }
    }
}

