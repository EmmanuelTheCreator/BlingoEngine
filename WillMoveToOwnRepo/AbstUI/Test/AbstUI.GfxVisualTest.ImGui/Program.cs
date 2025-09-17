using AbstUI.ImGui;
using BlingoEngine.SDL2.GfxVisualTest;
using BlingoEngine.ImGui.GfxVisualTest;
using AbstUI.Styles;

using var rootContext = new TestImGuiRootComponentContext();
var styleManager = new AbstStyleManager();
var factory = new AbstImGuiComponentFactory(rootContext, rootContext.FontManager, styleManager);

var scroll = GfxTestScene.Build(factory);
rootContext.ComponentContainer.Activate(((dynamic)scroll.FrameworkObj).ComponentContext);

rootContext.Run();

