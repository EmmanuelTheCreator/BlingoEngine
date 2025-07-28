using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal class DirGodotColorPaletteSprite : DirGodotKeyframeSprite<LingoColorPaletteKeyframe>
{
    public DirGodotColorPaletteSprite(LingoColorPaletteKeyframe keyframe) : base(keyframe)
    {
    }

    protected override string Label => Keyframe.PaletteId.ToString();

    internal override void DeleteFromMovie(LingoMovie movie)
    {
        movie.RemoveColorPaletteKeyframe(Keyframe.Frame);
    }
}
