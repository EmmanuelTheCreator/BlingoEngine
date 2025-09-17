using BlingoEngine.Director.Core.Styles;
using AbstUI.Primitives;

namespace BlingoEngine.Director.Core.Scores;

public class DirScoreGfxValues
{
    public int LabelsBarHeight { get; set; } = 20;
    public int ChannelHeight { get; set; } = 16;
    public int FrameWidth { get; set; } = 9;
    public int LeftMargin { get; set; } = 0;
    public int ChannelLabelWidth { get; set; } = 54;
    public int ChannelInfoWidth { get; set; }
    public int ExtraMargin { get; set; } = 20;
    public int TopStripHeight { get; set; } = 120;

    public AColor ColLineLight { get; set; } = DirectorColors.LineLight;
    public AColor ColLineDark { get; set; } = DirectorColors.LineDark;
    public float ChannelFramesHeight { get; set; } = 20;

    public DirScoreGfxValues()
    {
        ChannelInfoWidth = ChannelLabelWidth + 16;
    }
}

