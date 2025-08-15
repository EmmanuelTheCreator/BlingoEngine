using AbstUI.Primitives;
using LingoEngine.Animations;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Stages
{
    public interface ILingoStage
    {
        LingoMovie? ActiveMovie { get; }
        AColor BackgroundColor { get; set; }
        int Height { get; set; }
        LingoMember? MouseMemberUnderMouse { get; }
        int Width { get; set; }

        void AddKeyFrame(LingoSprite2D sprite);
        
        T Framework<T>() where T : class, ILingoFrameworkStage;
        ILingoFrameworkStage FrameworkObj();
        LingoSpriteMotionPath? GetSpriteMotionPath(LingoSprite2D sprite);
        LingoSprite2D? GetSpriteUnderMouse();
        void SetActiveMovie(LingoMovie? lingoMovie);
        void SetSpriteTweenOptions(LingoSprite2D sprite, bool positionEnabled, bool sizeEnabled, bool rotationEnabled, bool skewEnabled, bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled, float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut);
        void UpdateKeyFrame(LingoSprite2D sprite);
    }
}