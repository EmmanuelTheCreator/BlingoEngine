using System.Collections.Generic;
using Godot;

namespace AbstUI.LGodot.Inputs;

public static class GodotKeyCodeMap
{
    private static readonly Dictionary<Key, int> Mac = new()
    {
        // Editing / Navigation
        { Key.Enter, 36 },
        { Key.Escape, 53 },
        { Key.Tab, 48 },
        { Key.Space, 49 },
        { Key.Backspace, 51 },
        { Key.Delete, 117 },
        { Key.Home, 115 },
        { Key.End, 119 },
        { Key.Pageup, 116 },
        { Key.Pagedown, 121 },
        { Key.Left, 123 },
        { Key.Right, 124 },
        { Key.Down, 125 },
        { Key.Up, 126 },
        { Key.Help, 114 },
        { Key.Clear, 71 },
        { Key.KpEnter, 76 },
        // Modifiers
        { Key.Shift, 56 },
        { Key.Ctrl, 59 },
        { Key.Alt, 58 },
        { Key.Meta, 55 },
        { Key.Capslock, 57 },
        // Function Keys
        { Key.F1, 122 },
        { Key.F2, 120 },
        { Key.F3, 99 },
        { Key.F4, 118 },
        { Key.F5, 96 },
        { Key.F6, 97 },
        { Key.F7, 98 },
        { Key.F8, 100 },
        { Key.F9, 101 },
        { Key.F10, 109 },
        { Key.F11, 103 },
        { Key.F12, 111 },
        // Letters
        { Key.A, 0 },
        { Key.B, 11 },
        { Key.C, 8 },
        { Key.D, 2 },
        { Key.E, 14 },
        { Key.F, 3 },
        { Key.G, 5 },
        { Key.H, 4 },
        { Key.I, 34 },
        { Key.J, 38 },
        { Key.K, 40 },
        { Key.L, 37 },
        { Key.M, 46 },
        { Key.N, 45 },
        { Key.O, 31 },
        { Key.P, 35 },
        { Key.Q, 12 },
        { Key.R, 15 },
        { Key.S, 1 },
        { Key.T, 17 },
        { Key.U, 32 },
        { Key.V, 9 },
        { Key.W, 13 },
        { Key.X, 7 },
        { Key.Y, 16 },
        { Key.Z, 6 },
        // Numbers & Symbols
        { Key.Key1, 18 },
        { Key.Key2, 19 },
        { Key.Key3, 20 },
        { Key.Key4, 21 },
        { Key.Key5, 23 },
        { Key.Key6, 22 },
        { Key.Key7, 26 },
        { Key.Key8, 28 },
        { Key.Key9, 25 },
        { Key.Key0, 29 },
        { Key.Minus, 27 },
        { Key.Equal, 24 },
        { Key.Quoteleft, 50 },
        { Key.Bracketleft, 33 },
        { Key.Bracketright, 30 },
        { Key.Backslash, 42 },
        { Key.Semicolon, 41 },
        { Key.Apostrophe, 39 },
        { Key.Comma, 43 },
        { Key.Period, 47 },
        { Key.Slash, 44 },
        // Numpad
        { Key.Kp0, 82 },
        { Key.Kp1, 83 },
        { Key.Kp2, 84 },
        { Key.Kp3, 85 },
        { Key.Kp4, 86 },
        { Key.Kp5, 87 },
        { Key.Kp6, 88 },
        { Key.Kp7, 89 },
        { Key.Kp8, 91 },
        { Key.Kp9, 92 },
        { Key.KpPeriod, 65 },
        { Key.KpDivide, 75 },
        { Key.KpMultiply, 67 },
        { Key.KpSubtract, 78 },
        { Key.KpAdd, 69 },
    };

    private static Dictionary<int, Key> BuildReverse(Dictionary<Key, int> source)
    {
        var dict = new Dictionary<int, Key>();
        foreach (var kv in source)
            if (!dict.ContainsKey(kv.Value))
                dict[kv.Value] = kv.Key;
        return dict;
    }

    private static readonly Dictionary<int, Key> MacReverse = BuildReverse(Mac);

    public static int ToLingo(Key key)
    {
        if (Mac.TryGetValue(key, out var code))
            return code;
        int val = (int)key;
        if (val >= 'a' && val <= 'z')
            return val - 32;
        if (val >= 'A' && val <= 'Z')
            return val;
        if (val >= '0' && val <= '9')
            return val;
        return val;
    }

    public static Key ToGodot(int lingoCode)
    {
        if (MacReverse.TryGetValue(lingoCode, out var key))
            return key;
        if (lingoCode >= 65 && lingoCode <= 90)
            return (Key)lingoCode;
        if (lingoCode >= 48 && lingoCode <= 57)
            return (Key)lingoCode;
        return (Key)lingoCode;
    }
}
