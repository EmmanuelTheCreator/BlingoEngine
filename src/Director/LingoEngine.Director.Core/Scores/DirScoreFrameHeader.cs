using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Styles;

namespace LingoEngine.Director.Core.Scores;

public class DirScoreFrameHeader : IDisposable
{
    private LingoMovie? _movie;
    private readonly DirScoreGfxValues _gfxValues;
    private readonly LingoGfxCanvas _canvas;

    public LingoGfxCanvas Canvas => _canvas;

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
        _canvas.DrawText(new LingoPoint(1, 13), "1", null, LingoColorList.Gray, 10);
        for (int f = 0; f <= frameCount; f++)
        {
            float x = _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            if ((f + 1) % 5 == 0)
                _canvas.DrawText(new LingoPoint(x + 1, 13), (f + 1).ToString(), null, LingoColorList.Gray, 10);
        }
        _canvas.DrawLine(new LingoPoint(0, 0), new LingoPoint(_canvas.Width, 0), DirectorColors.LineLight, 1);
        _canvas.DrawLine(new LingoPoint(0, _gfxValues.ChannelFramesHeight), new LingoPoint(_canvas.Width, _gfxValues.ChannelFramesHeight), DirectorColors.LineDark, 1);
    }

    public void HandleMouseEvent(LingoMouseEvent mouseEvent, int mouseFrame)
    {
        if (_movie == null) return;
        if (mouseEvent.Type == LingoMouseEventType.MouseDown && mouseEvent.Mouse.LeftMouseDown)
        {
            if (mouseFrame >= 1 && mouseFrame <= _movie.FrameCount)
            {
                if (_movie.IsPlaying)
                    _movie.GoTo(mouseFrame);
                else
                    _movie.GoToAndStop(mouseFrame);
            }
        }
    }
}

