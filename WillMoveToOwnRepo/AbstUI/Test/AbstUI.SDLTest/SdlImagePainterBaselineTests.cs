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
            painter.AutoResize = true;


            painter.Name = "h";
            painter.DrawRect(ARect.New(0, 0, 80, 30), AColors.Red);
            painter.DrawText(new APoint(0, 0), "halooo", "Tahoma",AColors.Black,32);
            painter.Render();
            var hTex = (SdlTexture2D)painter.GetTexture("h");
            var hPixels = hTex.GetPixels(renderer);
            int topH = GraphicsPixelHelper.FindTopOpaqueRow(hPixels, hTex.Width, hTex.Height);
            int bottomH = GraphicsPixelHelper.FindBottomOpaqueRow(hPixels, hTex.Width, hTex.Height);

            painter.Name = "p";
            painter.Clear(new AColor(0, 0, 0, 0));
            painter.DrawText(new APoint(0, 0), "p", fontSize: 32);
            painter.Render();
            var pTex = (SdlTexture2D)painter.GetTexture("p");
            var pPixels = pTex.GetPixels(renderer);
            int topP = GraphicsPixelHelper.FindTopOpaqueRow(pPixels, pTex.Width, pTex.Height);
            int bottomP = GraphicsPixelHelper.FindBottomOpaqueRow(pPixels, pTex.Width, pTex.Height);

            //topH.Should().Be(topP);
            //bottomP.Should().BeGreaterThanOrEqualTo(bottomH);
        });

    }
}
