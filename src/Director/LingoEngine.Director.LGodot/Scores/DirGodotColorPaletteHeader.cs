using LingoEngine.Director.Core.Scores;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotColorPaletteHeader : DirGodotTopChannelHeader
{
    public DirGodotColorPaletteHeader(DirScoreGfxValues gfxValues) : base(gfxValues) {}

    protected override string Icon => "🎨";
}
