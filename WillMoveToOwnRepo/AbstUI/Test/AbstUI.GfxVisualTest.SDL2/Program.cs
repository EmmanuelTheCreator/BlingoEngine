using AbstUI.Components;
using AbstUI.GfxVisualTest;
using AbstUI.GfxVisualTest.SDL2;
using AbstUI.SDL2;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;
using LingoEngine.SDL2.GfxVisualTest;
using Microsoft.Extensions.DependencyInjection;
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

    rootContext.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.DarkRed;
    Console.WriteLine("An error occurred while running the SDL2 Gfx Visual Test:");
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex);
}

