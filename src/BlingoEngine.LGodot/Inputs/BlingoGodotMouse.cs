using Godot;
using BlingoEngine.Inputs;
using BlingoEngine.Bitmaps;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Inputs;
using AbstUI.Inputs;


namespace BlingoEngine.LGodot.Inputs
{
    public class BlingoGodotMouse : AbstGodotMouse, IBlingoFrameworkMouse
    {
        public BlingoGodotMouse(Lazy<IAbstMouseInternal> blingoMouse) : base(blingoMouse)
        {
        }

        public void SetCursor(BlingoMemberBitmap? image)
        {
            if (image == null) return;
            if (!(image.TextureBlingo is AbstGodotTexture2D godotTexture))
                return;
            DisplayServer.Singleton.CursorSetCustomImage(godotTexture.Texture, DisplayServer.CursorShape.Arrow, hotspot: Vector2.Zero);
        }

       
    }

}
