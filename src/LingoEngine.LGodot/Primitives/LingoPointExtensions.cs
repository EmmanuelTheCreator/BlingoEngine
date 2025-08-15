using Godot;
using AbstUI.Primitives;

namespace LingoEngine.LGodot.Primitives
{
    public static class LingoPointExtensions
    {
        public static Vector2 ToVector2(this APoint point)
            => new Vector2(point.X, point.Y);

        public static APoint ToLingoPoint(this Vector2 vector)
            => new APoint(vector.X, vector.Y); 
        
        public static Rect2 ToRect2(this ARect rect)
            => new Rect2(rect.Left, rect.Top, rect.Width, rect.Height);

        public static ARect ToLingoRect(this Rect2 rect)
            => ARect.New(rect.Position.X, rect.Position.Y, rect.Size.X, rect.Size.Y);
    }

}
