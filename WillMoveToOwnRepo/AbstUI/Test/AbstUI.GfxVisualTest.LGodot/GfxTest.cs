using AbstUI.Components;
using AbstUI.LGodot;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Styles;
using AbstUI.Styles;
using AbstUI.Windowing;
using Godot;
using LingoEngine.SDL2.GfxVisualTest;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AbstUI.GfxVisualTest.LGodot;

public partial class GfxTest : Node
{
    public override void _Ready()
    {
        try
        {
            var rootNode = new TestRootNode(this);
            var serviceCollection = new ServiceCollection();
            ServiceProvider serviceProvider = null!;
            serviceCollection
                .WithAbstUIGodot(w => w.AddSingletonWindow<GfxTestWindow, GodotTestWindow>(GfxTestWindow.MyWindowCode))
                .AddSingleton<IAbstGodotRootNode>(rootNode)
                .AddTransient<IServiceProvider>(p => serviceProvider);
            ;

            serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.WithAbstUIGodot();

            var factory = serviceProvider.GetRequiredService<GodotComponentFactory>();
            var root = GfxTestScene.Build(factory);
            if (root.FrameworkObj.FrameworkNode is Node node)
            {
                AddChild(node);
            }
        }
        catch (Exception ex)
        {

            throw;
        }
       
    }
    private class TestRootNode : IAbstGodotRootNode
    {
        public TestRootNode(Node rootNode)
        {
            RootNode = rootNode;
        }

        public Node RootNode { get; }

    }
}
