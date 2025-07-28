using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal abstract class DirGodotKeyframeSprite<TKeyframe> : DirGodotLabelSprite
    where TKeyframe : ILingoKeyframe
{
    internal readonly TKeyframe Keyframe;

    protected DirGodotKeyframeSprite(TKeyframe keyframe)
    {
        Keyframe = keyframe;
    }

    internal override int BeginFrame { get => Keyframe.Frame; set => Keyframe.Frame = value; }
    internal override int EndFrame { get => Keyframe.Frame; set => Keyframe.Frame = value; }
}
