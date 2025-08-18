
using AbstUI.ImGui;
using AbstUI.ImGui.Styles;
using LingoEngine.SDL2.GfxVisualTest;

using var rootContext = new TestImGuiRootComponentContext();
var fontManager = new ImGuiFontManager();
fontManager.LoadAll();
var factory = new AbstImGuiComponentFactory(rootContext, fontManager);

var scroll = GfxTestScene.Build(factory);
rootContext.ComponentContainer.Activate(((dynamic)scroll.FrameworkObj).ComponentContext);

rootContext.Run();
