using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal class DirGodotTransitionSprite : DirGodotKeyframeSprite<LingoTransitionKeyframe>
{
    public DirGodotTransitionSprite(LingoTransitionKeyframe keyframe) : base(keyframe)
    {
    }

    protected override string Label => Keyframe.TransitionId.ToString();

    internal override void DeleteFromMovie(LingoMovie movie)
    {
        movie.RemoveTransitionKeyframe(Keyframe.Frame);
    }
}
