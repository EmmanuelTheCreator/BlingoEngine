using Godot;
using LingoEngine.Inputs;
using LingoEngine.Bitmaps;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.Events;


namespace LingoEngine.LGodot
{
    public class LingoGodotMouse : AbstUIGodotMouse<LingoMouse, LingoMouseEvent> , ILingoFrameworkMouse
    {
        public LingoGodotMouse(Lazy<LingoMouse> lingoMouse) : base(lingoMouse)
        {
        }

        public void SetCursor(LingoMemberBitmap? image)
        {
            if (image == null) return;
            if (!(image.TextureLingo is LingoGodotTexture2D godotTexture))
                return;
            DisplayServer.Singleton.CursorSetCustomImage(godotTexture.Texture, DisplayServer.CursorShape.Arrow, hotspot: Vector2.Zero);
        }

    }

}