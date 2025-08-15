using AbstUI.Components;
using AbstUI.Primitives;
using LingoEngine.Director.Core.Styles;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Scores;

/// <summary>
/// Framework independent painter for the Score grid.
/// Uses a <see cref="AbstUIGfxCanvas"/> to draw the grid graphics.
/// </summary>
public class DirScoreGridPainter
{
    private readonly DirScoreGfxValues _gfxValues;
    public AbstUIGfxCanvas Canvas { get; }

    public int FrameCount { get; set; }
    public int ChannelCount { get; set; }
    public bool DrawBackground { get; set; } = true;

    public DirScoreGridPainter(ILingoFrameworkFactory factory, DirScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
        Canvas = factory.CreateGfxCanvas("ScoreGridCanvas", 0, 0);
    }

    /// <summary>Redraws the grid to the canvas.</summary>
    public void Draw()
    {
        var colorLines = DirectorColors.ScoreGridLineDark;
        float width = _gfxValues.LeftMargin + FrameCount * _gfxValues.FrameWidth;
        float height = ChannelCount * _gfxValues.ChannelHeight;
        Canvas.Clear(AColor.Transparent());
        Canvas.Width = width;
        Canvas.Height = height;


        if (DrawBackground)
            Canvas.DrawRect(ARect.New(0, 0, width, height), AColors.White, true);



        // Draw horizontal lines
        for (int c = 0; c <= ChannelCount; c++)
        {
            float y = c * _gfxValues.ChannelHeight;
            Canvas.DrawLine(new APoint(0, y), new APoint(width , y), _gfxValues.ColLineLight);
        }

        // Draw backgrounds every 5 frame.
        for (int f = 0; f < FrameCount; f++)
        {
            float x =  _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            if ((f+1) % 5 == 0)
                Canvas.DrawRect(ARect.New(x, 0, _gfxValues.FrameWidth, height), colorLines, true);
        }

        // Draw vertical lines
        for (int f = 0; f <= FrameCount; f++)
        {
            float x =  _gfxValues.LeftMargin + f * _gfxValues.FrameWidth;
            Canvas.DrawLine(new APoint(x, 0), new APoint(x, height), colorLines);
        }

        // Draw first bg on frame 1, director sets exceptionalmy the first bg in dark.
        Canvas.DrawRect(ARect.New(0, 0, _gfxValues.FrameWidth, height), colorLines, true);

        // Draw top bottom lines
        Canvas.DrawLine(new APoint(0, 0), new APoint(width , 0), colorLines);
        Canvas.DrawLine(new APoint(0, height), new APoint(width , height), colorLines);
    }
}
