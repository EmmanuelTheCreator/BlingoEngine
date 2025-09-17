using System.Collections.Generic;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Inputs;

public static class SdlKeyCodeMap
{
    private static readonly Dictionary<SDL.SDL_Keycode, int> Mac = new()
    {
        // Editing / Navigation
        { SDL.SDL_Keycode.SDLK_RETURN, 36 },
        { SDL.SDL_Keycode.SDLK_ESCAPE, 53 },
        { SDL.SDL_Keycode.SDLK_TAB, 48 },
        { SDL.SDL_Keycode.SDLK_SPACE, 49 },
        { SDL.SDL_Keycode.SDLK_BACKSPACE, 51 },
        { SDL.SDL_Keycode.SDLK_DELETE, 117 },
        { SDL.SDL_Keycode.SDLK_HOME, 115 },
        { SDL.SDL_Keycode.SDLK_END, 119 },
        { SDL.SDL_Keycode.SDLK_PAGEUP, 116 },
        { SDL.SDL_Keycode.SDLK_PAGEDOWN, 121 },
        { SDL.SDL_Keycode.SDLK_LEFT, 123 },
        { SDL.SDL_Keycode.SDLK_RIGHT, 124 },
        { SDL.SDL_Keycode.SDLK_DOWN, 125 },
        { SDL.SDL_Keycode.SDLK_UP, 126 },
        { SDL.SDL_Keycode.SDLK_HELP, 114 },
        { SDL.SDL_Keycode.SDLK_CLEAR, 71 },
        { SDL.SDL_Keycode.SDLK_KP_ENTER, 76 },
        // Modifiers
        { SDL.SDL_Keycode.SDLK_LSHIFT, 56 },
        { SDL.SDL_Keycode.SDLK_RSHIFT, 56 },
        { SDL.SDL_Keycode.SDLK_LCTRL, 59 },
        { SDL.SDL_Keycode.SDLK_RCTRL, 59 },
        { SDL.SDL_Keycode.SDLK_LALT, 58 },
        { SDL.SDL_Keycode.SDLK_RALT, 58 },
        { SDL.SDL_Keycode.SDLK_LGUI, 55 },
        { SDL.SDL_Keycode.SDLK_RGUI, 54 },
        { SDL.SDL_Keycode.SDLK_CAPSLOCK, 57 },
        // Function Keys
        { SDL.SDL_Keycode.SDLK_F1, 122 },
        { SDL.SDL_Keycode.SDLK_F2, 120 },
        { SDL.SDL_Keycode.SDLK_F3, 99 },
        { SDL.SDL_Keycode.SDLK_F4, 118 },
        { SDL.SDL_Keycode.SDLK_F5, 96 },
        { SDL.SDL_Keycode.SDLK_F6, 97 },
        { SDL.SDL_Keycode.SDLK_F7, 98 },
        { SDL.SDL_Keycode.SDLK_F8, 100 },
        { SDL.SDL_Keycode.SDLK_F9, 101 },
        { SDL.SDL_Keycode.SDLK_F10, 109 },
        { SDL.SDL_Keycode.SDLK_F11, 103 },
        { SDL.SDL_Keycode.SDLK_F12, 111 },
        // Letters
        { SDL.SDL_Keycode.SDLK_a, 0 },
        { SDL.SDL_Keycode.SDLK_b, 11 },
        { SDL.SDL_Keycode.SDLK_c, 8 },
        { SDL.SDL_Keycode.SDLK_d, 2 },
        { SDL.SDL_Keycode.SDLK_e, 14 },
        { SDL.SDL_Keycode.SDLK_f, 3 },
        { SDL.SDL_Keycode.SDLK_g, 5 },
        { SDL.SDL_Keycode.SDLK_h, 4 },
        { SDL.SDL_Keycode.SDLK_i, 34 },
        { SDL.SDL_Keycode.SDLK_j, 38 },
        { SDL.SDL_Keycode.SDLK_k, 40 },
        { SDL.SDL_Keycode.SDLK_l, 37 },
        { SDL.SDL_Keycode.SDLK_m, 46 },
        { SDL.SDL_Keycode.SDLK_n, 45 },
        { SDL.SDL_Keycode.SDLK_o, 31 },
        { SDL.SDL_Keycode.SDLK_p, 35 },
        { SDL.SDL_Keycode.SDLK_q, 12 },
        { SDL.SDL_Keycode.SDLK_r, 15 },
        { SDL.SDL_Keycode.SDLK_s, 1 },
        { SDL.SDL_Keycode.SDLK_t, 17 },
        { SDL.SDL_Keycode.SDLK_u, 32 },
        { SDL.SDL_Keycode.SDLK_v, 9 },
        { SDL.SDL_Keycode.SDLK_w, 13 },
        { SDL.SDL_Keycode.SDLK_x, 7 },
        { SDL.SDL_Keycode.SDLK_y, 16 },
        { SDL.SDL_Keycode.SDLK_z, 6 },
        // Numbers & Symbols
        { SDL.SDL_Keycode.SDLK_1, 18 },
        { SDL.SDL_Keycode.SDLK_2, 19 },
        { SDL.SDL_Keycode.SDLK_3, 20 },
        { SDL.SDL_Keycode.SDLK_4, 21 },
        { SDL.SDL_Keycode.SDLK_5, 23 },
        { SDL.SDL_Keycode.SDLK_6, 22 },
        { SDL.SDL_Keycode.SDLK_7, 26 },
        { SDL.SDL_Keycode.SDLK_8, 28 },
        { SDL.SDL_Keycode.SDLK_9, 25 },
        { SDL.SDL_Keycode.SDLK_0, 29 },
        { SDL.SDL_Keycode.SDLK_MINUS, 27 },
        { SDL.SDL_Keycode.SDLK_EQUALS, 24 },
        { SDL.SDL_Keycode.SDLK_BACKQUOTE, 50 },
        { SDL.SDL_Keycode.SDLK_LEFTBRACKET, 33 },
        { SDL.SDL_Keycode.SDLK_RIGHTBRACKET, 30 },
        { SDL.SDL_Keycode.SDLK_BACKSLASH, 42 },
        { SDL.SDL_Keycode.SDLK_SEMICOLON, 41 },
        { SDL.SDL_Keycode.SDLK_QUOTE, 39 },
        { SDL.SDL_Keycode.SDLK_COMMA, 43 },
        { SDL.SDL_Keycode.SDLK_PERIOD, 47 },
        { SDL.SDL_Keycode.SDLK_SLASH, 44 },
        // Numpad
        { SDL.SDL_Keycode.SDLK_KP_0, 82 },
        { SDL.SDL_Keycode.SDLK_KP_1, 83 },
        { SDL.SDL_Keycode.SDLK_KP_2, 84 },
        { SDL.SDL_Keycode.SDLK_KP_3, 85 },
        { SDL.SDL_Keycode.SDLK_KP_4, 86 },
        { SDL.SDL_Keycode.SDLK_KP_5, 87 },
        { SDL.SDL_Keycode.SDLK_KP_6, 88 },
        { SDL.SDL_Keycode.SDLK_KP_7, 89 },
        { SDL.SDL_Keycode.SDLK_KP_8, 91 },
        { SDL.SDL_Keycode.SDLK_KP_9, 92 },
        { SDL.SDL_Keycode.SDLK_KP_PERIOD, 65 },
        { SDL.SDL_Keycode.SDLK_KP_DIVIDE, 75 },
        { SDL.SDL_Keycode.SDLK_KP_MULTIPLY, 67 },
        { SDL.SDL_Keycode.SDLK_KP_MINUS, 78 },
        { SDL.SDL_Keycode.SDLK_KP_PLUS, 69 },
    };

    private static Dictionary<int, SDL.SDL_Keycode> BuildReverse(Dictionary<SDL.SDL_Keycode, int> source)
    {
        var dict = new Dictionary<int, SDL.SDL_Keycode>();
        foreach (var kv in source)
            if (!dict.ContainsKey(kv.Value))
                dict[kv.Value] = kv.Key;
        return dict;
    }

    private static readonly Dictionary<int, SDL.SDL_Keycode> MacReverse = BuildReverse(Mac);

    public static int ToBlingo(SDL.SDL_Keycode key)
    {
        if (Mac.TryGetValue(key, out var code))
            return code;
        int val = (int)key;
        if (val >= 'a' && val <= 'z')
            return val - 32;
        if (val >= '0' && val <= '9')
            return val;
        return val;
    }

    public static SDL.SDL_Keycode ToSDL(int blingoCode)
    {
        if (MacReverse.TryGetValue(blingoCode, out var key))
            return key;
        if (blingoCode >= 65 && blingoCode <= 90)
            return (SDL.SDL_Keycode)(blingoCode + 32);
        if (blingoCode >= 48 && blingoCode <= 57)
            return (SDL.SDL_Keycode)blingoCode;
        return (SDL.SDL_Keycode)blingoCode;
    }
}

