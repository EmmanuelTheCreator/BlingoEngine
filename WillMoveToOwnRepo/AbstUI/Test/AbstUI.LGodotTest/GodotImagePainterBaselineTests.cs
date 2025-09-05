using AbstUI.Primitives;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Components.Graphics;
using FluentAssertions;
using AbstUI.LGodot.Styles;
using AbstUI.Tests.Common;
using GdUnit4;

namespace AbstUI.LGodotTest;

public class GodotImagePainterBaselineTests
{
    [Fact]
    [RequireGodotRuntime]
    [GodotExceptionMonitor]
    public void DescenderGlyphExtendsBelowBaseline()
    {
        GodotTestHost.Run(fontManager =>
        {
            using var painter = new GodotImagePainterToTexture(fontManager, 64, 64);
            painter.AutoResize = true;

            painter.Name = "h";
            //painter.DrawRect(ARect.New(0, 0, 80, 30), AColors.Red);
            painter.DrawSingleLine(new APoint(0, 0), "halooo", "Tahoma", AColors.Black, 32);
            painter.Render();
            var hTex = (AbstGodotTexture2D)painter.GetTexture("h");
            var hPixels = hTex.GetPixels();
            int topH = GraphicsPixelHelper.FindTopOpaqueRow(hPixels, hTex.Width, hTex.Height);
            int bottomH = GraphicsPixelHelper.FindBottomOpaqueRow(hPixels, hTex.Width, hTex.Height);

            painter.Name = "p";
            painter.Clear(new AColor(0, 0, 0, 0));
            painter.DrawSingleLine(new APoint(0, 0), "p", "Tahoma", AColors.Black, 32);
            painter.Render();
            var pTex = (AbstGodotTexture2D)painter.GetTexture("p");
            var pPixels = pTex.GetPixels();
            int topP = GraphicsPixelHelper.FindTopOpaqueRow(pPixels, pTex.Width, pTex.Height);
            int bottomP = GraphicsPixelHelper.FindBottomOpaqueRow(pPixels, pTex.Width, pTex.Height);

            topH.Should().Be(topP);
            bottomP.Should().BeGreaterThanOrEqualTo(bottomH);
        });
    }
}
