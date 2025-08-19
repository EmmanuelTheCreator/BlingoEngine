using AbstUI.ImGui;
using LingoEngine.SDL2.GfxVisualTest;
using LingoEngine.ImGui.GfxVisualTest;
using AbstUI.Styles;

using var rootContext = new TestImGuiRootComponentContext();
var styleManager = new AbstStyleManager();
var factory = new AbstImGuiComponentFactory(rootContext, rootContext.FontManager, styleManager);

var scroll = GfxTestScene.Build(factory);
rootContext.ComponentContainer.Activate(((dynamic)scroll.FrameworkObj).ComponentContext);

rootContext.Run();
