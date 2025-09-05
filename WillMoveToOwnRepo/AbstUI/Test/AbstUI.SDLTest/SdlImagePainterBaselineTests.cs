using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.Components.Graphics;
using FluentAssertions;
using AbstUI.SDL2.Styles;
using AbstUI.Tests.Common;

namespace AbstUI.SDLTest;

public class SdlImagePainterBaselineTests
{
    [Fact]
    public void DescenderGlyphExtendsBelowBaseline()
    {
        SdlTestHost.Run((window, renderer, fontManager) =>
        {
            using var painter = new SDLImagePainter(fontManager, 64, 64, renderer);

            painter.DrawText(new APoint(0, 0), "h", fontSize: 32);
            var hTex = (SdlTexture2D)painter.GetTexture("h");
            var hPixels = hTex.GetPixels(renderer);
            int topH = GraphicsPixelHelper.FindTopOpaqueRow(hPixels, hTex.Width, hTex.Height);
            int bottomH = GraphicsPixelHelper.FindBottomOpaqueRow(hPixels, hTex.Width, hTex.Height);

            painter.Clear(new AColor(0, 0, 0, 0));
            painter.DrawText(new APoint(0, 0), "p", fontSize: 32);
            var pTex = (SdlTexture2D)painter.GetTexture("p");
            var pPixels = pTex.GetPixels(renderer);
            int topP = GraphicsPixelHelper.FindTopOpaqueRow(pPixels, pTex.Width, pTex.Height);
            int bottomP = GraphicsPixelHelper.FindBottomOpaqueRow(pPixels, pTex.Width, pTex.Height);

            topH.Should().Be(topP);
            bottomP.Should().BeGreaterThanOrEqualTo(bottomH);
        });
    }
}
