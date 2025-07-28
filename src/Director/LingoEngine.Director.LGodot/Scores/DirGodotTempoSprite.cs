using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Drawable tempo keyframe.
/// </summary>
internal class DirGodotTempoSprite : DirGodotKeyframeSprite<LingoTempoKeyframe>
{
    public DirGodotTempoSprite(LingoTempoKeyframe keyframe) : base(keyframe)
    {
    }
    protected override string Label => Keyframe.Fps.ToString();

    internal override void DeleteFromMovie(LingoMovie movie)
    {
        movie.RemoveTempoKeyframe(Keyframe.Frame);
    }
}
