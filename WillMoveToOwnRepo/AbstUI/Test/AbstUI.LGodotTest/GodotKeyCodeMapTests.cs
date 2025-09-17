using AbstUI.LGodot.Inputs;
using Godot;
using Xunit;

namespace AbstUI.LGodotTest;

public class GodotKeyCodeMapTests
{
    [Fact]
    public void MapsLettersToBlingoCodes()
    {
        var code = GodotKeyCodeMap.ToBlingo(Key.A);
        Assert.Equal(0, code);
        Assert.Equal(Key.A, GodotKeyCodeMap.ToGodot(0));
    }

    [Fact]
    public void MapsFunctionKey()
    {
        Assert.Equal(122, GodotKeyCodeMap.ToBlingo(Key.F1));
    }
}

