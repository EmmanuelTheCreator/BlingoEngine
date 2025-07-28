using LingoEngine.Director.Core.Scores;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotTempoHeader : DirGodotTopChannelHeader
{
    public DirGodotTempoHeader(DirScoreGfxValues gfxValues) : base(gfxValues) {}

    protected override string Icon => "⏱";
}
