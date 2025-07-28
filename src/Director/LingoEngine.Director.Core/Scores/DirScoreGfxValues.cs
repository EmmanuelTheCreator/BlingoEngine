using LingoEngine.Primitives;
using LingoEngine.Director.Core.Styles;

namespace LingoEngine.Director.Core.Scores;

public class DirScoreGfxValues
{
    public int ChannelHeight { get; set; } = 16;
    public int FrameWidth { get; set; } = 9;
    public int LeftMargin { get; set; } = 0;
    public int ChannelLabelWidth { get; set; } = 54;
    public int ChannelInfoWidth { get; set; }
    public int ExtraMargin { get; set; } = 20;
    public int TopStripHeight { get; set; } = 120;
    public int LabelsBarHeight { get; set; } = 20;

    public LingoColor ColLineLight { get; set; } = DirectorColors.ScoreGridLineLight;
    public LingoColor ColLineDark { get; set; } = DirectorColors.ScoreGridLineDark;
    public float ChannelFramesHeight { get; set; } = 20;

    public DirScoreGfxValues()
    {
        ChannelInfoWidth = ChannelLabelWidth + 16;
    }
}
