
using AbstUI.ImGui;
using LingoEngine.SDL2.GfxVisualTest;
using LingoEngine.ImGui.GfxVisualTest;

using var rootContext = new TestImGuiRootComponentContext();
var factory = new AbstImGuiComponentFactory(rootContext, rootContext.FontManager);

var scroll = GfxTestScene.Build(factory);
rootContext.ComponentContainer.Activate(((dynamic)scroll.FrameworkObj).ComponentContext);

rootContext.Run();
