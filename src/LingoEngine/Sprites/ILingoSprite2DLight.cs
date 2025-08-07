using LingoEngine.Primitives;
using LingoEngine.Members;

namespace LingoEngine.Sprites
{
    public interface ILingoSprite2DLight
    {
        LingoColor BackColor { get; set; }
        float Blend { get; set; }
        int Constraint { get; set; }
        int DisplayMember { get; set; }
        bool FlipH { get; set; }
        bool FlipV { get; set; }
        LingoColor ForeColor { get; set; }
        float Height { get; set; }
        bool Hilite { get; set; }
        int Ink { get; set; }
        LingoInkType InkType { get; set; }
        bool Linked { get; }
        bool Loaded { get; }
        LingoPoint Loc { get; set; }
        float LocH { get; set; }
        float LocV { get; set; }
        int LocZ { get; set; }
        ILingoMember? Member { get; set; }
        int MemberNum { get; set; }
        string Name { get; set; }
        LingoRect Rect { get; }
        LingoPoint RegPoint { get; set; }
        float Rotation { get; set; }
        float Skew { get; set; }
        int SpriteNumWithChannel { get; }
        float Width { get; set; }
        int SpriteNum { get; }

        void AddKeyframes(params (int Frame, float X, float Y, float Rotation, float Skew)[] keyframes);
        string GetFullName();
        void SetSpriteTweenOptions(bool positionEnabled, bool rotationEnabled, bool skewEnabled, bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled, float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut);
        void UpdateKeyframe(int frame, float x, float y, float rotation, float skew);
    }
}
