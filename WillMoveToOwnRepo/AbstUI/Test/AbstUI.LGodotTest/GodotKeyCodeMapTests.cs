using AbstUI.LGodot.Inputs;
using Godot;
using Xunit;

namespace AbstUI.LGodotTest;

public class GodotKeyCodeMapTests
{
    [Fact]
    public void MapsLettersToLingoCodes()
    {
        var code = GodotKeyCodeMap.ToLingo(Key.A);
        Assert.Equal(0, code);
        Assert.Equal(Key.A, GodotKeyCodeMap.ToGodot(0));
    }

    [Fact]
    public void MapsFunctionKey()
    {
        Assert.Equal(122, GodotKeyCodeMap.ToLingo(Key.F1));
    }
}
