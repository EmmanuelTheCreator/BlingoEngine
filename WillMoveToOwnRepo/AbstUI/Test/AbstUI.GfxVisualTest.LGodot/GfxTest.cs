using AbstUI.Components;
using AbstUI.LGodot;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Components.Graphics;
using AbstUI.LGodot.Styles;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Windowing;
using Godot;
using LingoEngine.SDL2.GfxVisualTest;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace AbstUI.GfxVisualTest.LGodot;

public partial class GfxTest : Node
{
    public override void _Ready()
    {
        try
        {
            //RunTest();
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
    private void RunTest()
    {
        var fontManager = new AbstGodotFontManager();
        fontManager
           .AddFont("Arcade", Path.Combine("Media", "Fonts", "arcade.ttf"))
           .AddFont("Bikly", Path.Combine("Media", "Fonts", "bikly.ttf"))
           .AddFont("8Pin Matrix", Path.Combine("Media", "Fonts", "8PinMatrix.ttf"))
           .AddFont("Earth", Path.Combine("Media", "Fonts", "earth.ttf"))
           .AddFont("Tahoma", Path.Combine("Media", "Fonts", "Tahoma.ttf"));
        fontManager.LoadAll();

        using var painter = new GodotImagePainterToTexture(fontManager, 0, 0);
        painter.AutoResize = true;

        painter.Name = "h";
        //painter.DrawRect(ARect.New(0, 0, 80, 30), AColors.Red);
        painter.DrawText(new APoint(50, 90), "Stage\nother longer text\nmore text", "Tahoma", AColors.Black, 32,-1,Texts.AbstTextAlignment.Right);
        painter.Render();
        painter.GetTexture();
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
