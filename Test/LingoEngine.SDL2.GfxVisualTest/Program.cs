using LingoEngine.SDL2.Gfx;
using LingoEngine.SDL2.GfxVisualTest;
using LingoEngine.SDL2.Styles;

using var rootContext = new TestSdlRootComponentContext();
var fontManager = new SdlFontManager();
fontManager.LoadAll();
var factory = new SdlGfxFactory(rootContext, fontManager);

var scroll = GfxTestScene.Build(factory);
rootContext.ComponentContainer.Activate(((dynamic)scroll.FrameworkObj).ComponentContext);

rootContext.Run();
