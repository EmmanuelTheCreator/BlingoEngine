using AbstUI.SDL2;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.Windowing;
using AbstUI.Styles;
using AbstUI.Windowing;
using LingoEngine.SDL2.GfxVisualTest;
using Microsoft.Extensions.DependencyInjection;

using var rootContext = new TestSdlRootComponentContext();
var fontManager = new SdlFontManager();
fontManager.LoadAll();
var styleManager = new AbstStyleManager();
var serviceCollection = new ServiceCollection();

serviceCollection
    .AddSingleton<IAbstStyleManager>(styleManager)
    .AddSingleton<IAbstFontManager>(fontManager)
    .AddSingleton<IAbstFrameworkWindowManager, AbstSdlWindowManager>()
    .AddSingleton<IAbstWindowManager, AbstWindowManager>()
    ;

var serviceProvier = serviceCollection.BuildServiceProvider();

var factory = new AbstSdlComponentFactory(rootContext, serviceProvier);
rootContext.Factory = factory;

var scroll = GfxTestScene.Build(factory);
rootContext.ComponentContainer.Activate(((dynamic)scroll.FrameworkObj).ComponentContext);

rootContext.Run();
