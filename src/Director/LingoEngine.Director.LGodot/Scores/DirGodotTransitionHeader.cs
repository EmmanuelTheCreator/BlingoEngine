using LingoEngine.Director.Core.Scores;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotTransitionHeader : DirGodotTopChannelHeader
{
    public DirGodotTransitionHeader(DirScoreGfxValues gfxValues) : base(gfxValues) {}

    protected override string Icon => "➡";
}
