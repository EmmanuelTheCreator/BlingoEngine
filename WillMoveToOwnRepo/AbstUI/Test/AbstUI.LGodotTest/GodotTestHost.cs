using System;
using System.IO;
using AbstUI.LGodot.Styles;
using Godot;

namespace AbstUI.LGodotTest;

public static class GodotTestHost
{
    public static void Run(Action<AbstGodotFontManager> test)
    {
        AbstGodotFontManager.IsRunningInTest = true;
        var fontManager = new AbstGodotFontManager();
        fontManager
            .AddFont("Arcade", Path.Combine("Media", "Fonts", "arcade.ttf"))
            .AddFont("Bikly", Path.Combine("Media", "Fonts", "bikly.ttf"))
            .AddFont("8Pin Matrix", Path.Combine("Media", "Fonts", "8PinMatrix.ttf"))
            .AddFont("Earth", Path.Combine("Media", "Fonts", "earth.ttf"))
            .AddFont("Tahoma", Path.Combine("Media", "Fonts", "Tahoma.ttf"));
        fontManager.LoadAll();
        test(fontManager);
    }
}
