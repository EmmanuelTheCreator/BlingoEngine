using LingoEngine.Animations;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.Stages
{
    public interface ILingoStage
    {
        LingoMovie? ActiveMovie { get; }
        LingoColor BackgroundColor { get; set; }
        int Height { get; set; }
        LingoMember? MouseMemberUnderMouse { get; }
        bool RecordKeyframes { get; set; }
        int Width { get; set; }

        void AddKeyFrame(LingoSprite sprite);
        
        T Framework<T>() where T : class, ILingoFrameworkStage;
        ILingoFrameworkStage FrameworkObj();
        LingoSpriteMotionPath? GetSpriteMotionPath(LingoSprite sprite);
        LingoSprite? GetSpriteUnderMouse();
        void SetActiveMovie(LingoMovie? lingoMovie);
        void SetSpriteTweenOptions(LingoSprite sprite, bool positionEnabled, bool rotationEnabled, bool skewEnabled, bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled, float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut);
        void UpdateKeyFrame(LingoSprite sprite);
    }
}