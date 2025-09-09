using AbstUI.SDL2.Inputs;
using AbstUI.SDL2.SDLL;
using Xunit;

namespace AbstUI.SDLTest;

public class SdlKeyCodeMapTests
{
    [Fact]
    public void MapsLettersToLingoCodes()
    {
        var code = SdlKeyCodeMap.ToLingo(SDL.SDL_Keycode.SDLK_a);
        Assert.Equal(0, code);
        Assert.Equal(SDL.SDL_Keycode.SDLK_a, SdlKeyCodeMap.ToSDL(0));
    }

    [Fact]
    public void MapsFunctionKey()
    {
        Assert.Equal(122, SdlKeyCodeMap.ToLingo(SDL.SDL_Keycode.SDLK_F1));
    }
}
