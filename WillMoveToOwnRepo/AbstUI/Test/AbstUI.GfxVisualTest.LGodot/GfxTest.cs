using Godot;
using LingoEngine.SDL2.GfxVisualTest;

namespace AbstUI.GfxVisualTest.LGodot;

public partial class GfxTest : Node
{
    public override void _Ready()
    {
        var factory = new AbstGodotComponentFactory();
        var root = GfxTestScene.Build(factory);
        if (root.FrameworkObj.FrameworkNode is Node node)
        {
            AddChild(node);
        }
    }
}
