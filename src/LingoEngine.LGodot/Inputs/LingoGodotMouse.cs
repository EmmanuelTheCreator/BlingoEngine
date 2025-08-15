using Godot;
using LingoEngine.Inputs;
using LingoEngine.Bitmaps;
using LingoEngine.Events;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Inputs;


namespace LingoEngine.LGodot.Inputs
{
    public class LingoGodotMouse : AbstUIGodotMouse<LingoMouse, LingoMouseEvent>, ILingoFrameworkMouse
    {
        public LingoGodotMouse(Lazy<LingoMouse> lingoMouse) : base(lingoMouse)
        {
        }

        public void SetCursor(LingoMemberBitmap? image)
        {
            if (image == null) return;
            if (!(image.TextureLingo is AbstGodotTexture2D godotTexture))
                return;
            DisplayServer.Singleton.CursorSetCustomImage(godotTexture.Texture, DisplayServer.CursorShape.Arrow, hotspot: Vector2.Zero);
        }

    }

}