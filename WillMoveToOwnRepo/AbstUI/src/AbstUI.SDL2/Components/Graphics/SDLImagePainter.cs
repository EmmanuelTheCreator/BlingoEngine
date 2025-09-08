using AbstUI.Components.Graphics;
using AbstUI.Styles;

namespace AbstUI.SDL2.Components.Graphics;

public class SDLImagePainter : SDLImagePainterV2
{
    public SDLImagePainter(IAbstFontManager fontManager, int width, int height, nint renderer)
        : base(fontManager, width, height, renderer, false, 128)
    {
    }
}
