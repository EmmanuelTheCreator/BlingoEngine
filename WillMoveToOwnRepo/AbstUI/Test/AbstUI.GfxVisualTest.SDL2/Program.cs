using AbstUI.Components;
using AbstUI.GfxVisualTest;
using AbstUI.GfxVisualTest.SDL2;
using AbstUI.Primitives;
using AbstUI.SDL2;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Graphics;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Styles;
using AbstUI.Texts;
using LingoEngine.SDL2.GfxVisualTest;
using Microsoft.Extensions.DependencyInjection;
public class Program
{     
    public static void Main(string[] args)
    {
        try
        {
            using var rootContext = new TestSdlRootComponentContext();
            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddSingleton<ISdlRootComponentContext>(rootContext)
                .WithAbstUISdl(w => w.AddSingletonWindow<GfxTestWindow, SDLTestWindow>(GfxTestWindow.MyWindowCode))
                .SetupGfxTest()
                ;

            var serviceProvier = serviceCollection.BuildServiceProvider();
            serviceProvier
                .WithAbstUISdl();

            var factory = serviceProvier.GetRequiredService<IAbstComponentFactory>();
            rootContext.Factory = (AbstSdlComponentFactory)factory;

            serviceProvier.SetupGfxTest();

            var scroll = GfxTestScene.Build(factory);
            rootContext.ComponentContainer.Activate(((dynamic)scroll.FrameworkObj).ComponentContext);

            //RunTest(rootContext.Renderer);
            //RunTest2(rootContext.Renderer);
            
            rootContext.Run();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("An error occurred while running the SDL2 Gfx Visual Test:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex);
        }
    }

    private static void RunTest2(nint renderer)
    {
        var fontManager = CreateFontManager();  
        var markDownRender = new AbstMarkdownRenderer(fontManager);
        using var painter = new SDLImagePainter(fontManager, 0, 0, renderer);
        painter.AutoResize = true;

        //var text = "New Highscore!!! Enter your Name";
        //markDownRender.SetText(text, new[] { new AbstTextStyle { Name = "1", Font = "Earth", FontSize = 12 } }); 
        // var text = "{{PARA:1}}New **Highscore!!!**\n{{PARA:2}}Enter your {{FONT-SIZE:18}}Name";
        //markDownRender.SetText(text, new [] { new AbstTextStyle { Name = "1", Font = "Earth", FontSize = 12, Alignment= AbstTextAlignment.Right }, new AbstTextStyle { Name = "2", Font = "Tahoma", FontSize = 14 } });
        var text = "{{PARA:1}}Enter your {{FONT-SIZE:32}}Name";
        markDownRender.SetText(text, new [] { new AbstTextStyle { Name = "1", Font = "Tahoma", FontSize = 12 }});
        markDownRender.Render(painter, new APoint(0,0));
        painter.GetTexture();
    }
    private static void RunTest(nint renderer)
    {
        SdlFontManager fontManager = CreateFontManager();

        using var painter = new SDLImagePainter(fontManager, 0, 0, renderer);
        painter.AutoResize = true;

        painter.Name = "h";
        //painter.DrawRect(ARect.New(0, 0, 80, 30), AColors.Red);
        painter.DrawText(new APoint(0, 0), "Stage\ntest 4 longer...", "Earth", AColors.Black, 32, -1, AbstTextAlignment.Right);
        painter.Render();
        painter.GetTexture();
    }

    private static SdlFontManager CreateFontManager()
    {
        var fontManager = new SdlFontManager();
        fontManager
           .AddFont("Arcade", Path.Combine("Media", "Fonts", "arcade.ttf"))
           .AddFont("Bikly", Path.Combine("Media", "Fonts", "bikly.ttf"))
           .AddFont("8Pin Matrix", Path.Combine("Media", "Fonts", "8PinMatrix.ttf"))
           .AddFont("Earth", Path.Combine("Media", "Fonts", "earth.ttf"))
           .AddFont("Tahoma", Path.Combine("Media", "Fonts", "Tahoma.ttf"));
        fontManager.LoadAll();
        return fontManager;
    }
}



