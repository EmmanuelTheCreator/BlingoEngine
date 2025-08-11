using LingoEngine.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.Animations
{
    public struct LingoKeyFrameSetting
    {
       

        public int Frame { get; set; }
        public LingoPoint? Position { get; set; }
        public LingoPoint? Size { get; set; }
        public float? Rotation { get; set; }
        public float? Blend { get; set; }
        public float? Skew { get; set; }
        public LingoColor? ForeColor { get; set; }
        public LingoColor? BackColor { get; set; }


        public LingoKeyFrameSetting(int frame, LingoPoint? position = null, LingoPoint? size = null, float? rotation = null, float? blend = null, float? skew = null, LingoColor? foreColor = null, LingoColor? backColor = null)
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
                position: new LingoPoint(t.x, t.y),
                size: new LingoPoint(t.width,t.height)
            );
        } 
        public static implicit operator LingoKeyFrameSetting((int frame, int x, int y) t)
        {
            return new LingoKeyFrameSetting(
                frame: t.frame,
                position: new LingoPoint(t.x, t.y)
            );
        }

    }

    public static class LingoKeyFrameSettingExtensions
    {

        public static LingoKeyFrameSetting ToKeyFrameSetting(this ILingoSprite2DLight sprite, int frame)
        {
            return new LingoKeyFrameSetting(
                frame: frame,
                position: new LingoPoint(sprite.LocH, sprite.LocV),
                size: new LingoPoint(sprite.Width, sprite.Height),
                rotation: sprite.Rotation,
                blend: sprite.Blend,
                skew: sprite.Skew,
                foreColor: sprite.ForeColor,
                backColor: sprite.BackColor
            );
        }
    }
}
