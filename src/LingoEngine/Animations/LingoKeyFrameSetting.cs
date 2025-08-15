using AbstUI.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.Animations
{
    public struct LingoKeyFrameSetting
    {
       

        public int Frame { get; set; }
        public APoint? Position { get; set; }
        public APoint? Size { get; set; }
        public float? Rotation { get; set; }
        public float? Blend { get; set; }
        public float? Skew { get; set; }
        public AColor? ForeColor { get; set; }
        public AColor? BackColor { get; set; }


        public LingoKeyFrameSetting(int frame, APoint? position = null, APoint? size = null, float? rotation = null, float? blend = null, float? skew = null, AColor? foreColor = null, AColor? backColor = null)
        {
            Frame = frame;
            Position = position;
            Size = size;
            Rotation = rotation;
            Blend = blend;
            Skew = skew;
            ForeColor = foreColor;
            BackColor = backColor;
        }

        public static implicit operator LingoKeyFrameSetting((int frame, int x, int y, float width, float height) t)
        {
            return new LingoKeyFrameSetting(
                frame: t.frame,
                position: new APoint(t.x, t.y),
                size: new APoint(t.width,t.height)
            );
        } 
        public static implicit operator LingoKeyFrameSetting((int frame, int x, int y) t)
        {
            return new LingoKeyFrameSetting(
                frame: t.frame,
                position: new APoint(t.x, t.y)
            );
        }

    }

    public static class LingoKeyFrameSettingExtensions
    {

        public static LingoKeyFrameSetting ToKeyFrameSetting(this ILingoSprite2DLight sprite, int frame)
        {
            return new LingoKeyFrameSetting(
                frame: frame,
                position: new APoint(sprite.LocH, sprite.LocV),
                size: new APoint(sprite.Width, sprite.Height),
                rotation: sprite.Rotation,
                blend: sprite.Blend,
                skew: sprite.Skew,
                foreColor: sprite.ForeColor,
                backColor: sprite.BackColor
            );
        }
    }
}
