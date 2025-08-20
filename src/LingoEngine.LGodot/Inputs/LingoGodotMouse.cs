using Godot;
using LingoEngine.Inputs;
using LingoEngine.Bitmaps;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Inputs;
using AbstUI.Inputs;


namespace LingoEngine.LGodot.Inputs
{
    public class LingoGodotMouse : AbstGodotMouse, ILingoFrameworkMouse
    {
        public LingoGodotMouse(Lazy<IAbstMouseInternal> lingoMouse) : base(lingoMouse)
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