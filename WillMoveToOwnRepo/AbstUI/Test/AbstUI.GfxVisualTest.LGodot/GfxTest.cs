using AbstUI.Components;
using AbstUI.LGodot;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Styles;
using AbstUI.Styles;
using Godot;
using LingoEngine.SDL2.GfxVisualTest;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AbstUI.GfxVisualTest.LGodot;

public partial class GfxTest : Node
{
    public override void _Ready()
    {
        var rootNode = new TestRootNode(this);
        //var fontManager = new AbstGodotFontManager();
        //fontManager.LoadAll();
        //var styleManager = new AbstGodotStyleManager();
        var serviceCollection = new ServiceCollection();
        ServiceProvider serviceProvider = null!;
        serviceCollection
            .WithAbstUIGodot()
            .AddSingleton<IAbstGodotRootNode>(rootNode)
            //.AddSingleton<IAbstStyleManager>(styleManager)
            //.AddSingleton<IAbstFontManager>(fontManager)
            //.AddSingleton<GodotComponentFactory>()
            //.AddTransient<IAbstComponentFactory>(p => p.GetRequiredService<GodotComponentFactory>())
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
    private class TestRootNode : IAbstGodotRootNode
    {
        public TestRootNode(Node rootNode)
        {
            RootNode = rootNode;
        }

        public Node RootNode { get; }

    }
}
