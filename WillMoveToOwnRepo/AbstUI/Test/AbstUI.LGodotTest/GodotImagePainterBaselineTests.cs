using System.IO;
using AbstUI.Primitives;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Components.Graphics;
using AbstUI.LGodot.Styles;
using AbstUI.Tests.Common;

namespace AbstUI.LGodotTest;

public class GodotImagePainterBaselineTests
{
    [Fact]
    public void DescenderGlyphExtendsBelowBaseline()
    {
        var fm = new AbstGodotFontManager();
        using var painter = new GodotImagePainterToTexture(fm, 64, 64);

        painter.DrawText(new APoint(0, 0), "h", fontSize: 32);
        var hTex = (AbstGodotTexture2D)painter.GetTexture("h");
        var hPixels = hTex.GetPixels();
        int topH = GraphicsPixelHelper.FindTopOpaqueRow(hPixels, hTex.Width, hTex.Height);
        int bottomH = GraphicsPixelHelper.FindBottomOpaqueRow(hPixels, hTex.Width, hTex.Height);

        painter.Clear(new AColor(0, 0, 0, 0));
        painter.DrawText(new APoint(0, 0), "p", fontSize: 32);
        var pTex = (AbstGodotTexture2D)painter.GetTexture("p");
        var pPixels = pTex.GetPixels();
        int topP = GraphicsPixelHelper.FindTopOpaqueRow(pPixels, pTex.Width, pTex.Height);
        int bottomP = GraphicsPixelHelper.FindBottomOpaqueRow(pPixels, pTex.Width, pTex.Height);

        Assert.Equal(topH, topP);
        Assert.True(bottomP >= bottomH);
    }
}
