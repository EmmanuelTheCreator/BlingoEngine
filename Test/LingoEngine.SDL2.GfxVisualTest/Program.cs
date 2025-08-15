using AbstUI.SDL2.Components;
using AbstUI.SDL2.Styles;
using LingoEngine.SDL2.GfxVisualTest;

using var rootContext = new TestSdlRootComponentContext();
var fontManager = new SdlFontManager();
fontManager.LoadAll();
var factory = new SdlGfxFactory(rootContext, fontManager);

var scroll = GfxTestScene.Build(factory);
rootContext.ComponentContainer.Activate(((dynamic)scroll.FrameworkObj).ComponentContext);

rootContext.Run();
