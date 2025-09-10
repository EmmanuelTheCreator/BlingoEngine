using System.Collections.Generic;

namespace AbstUI.Blazor.Inputs;

/// <summary>
/// Maps between DOM keyboard <c>code</c> strings and Lingo numeric key codes.
/// </summary>
public static class BlazorKeyCodeMap
{
    private static readonly Dictionary<string, int> Mac = new()
    {
        // Editing / Navigation
        { "Enter", 36 },
        { "Escape", 53 },
        { "Tab", 48 },
        { "Space", 49 },
        { "Backspace", 51 },
        { "Delete", 117 },
        { "Home", 115 },
        { "End", 119 },
        { "PageUp", 116 },
        { "PageDown", 121 },
        { "ArrowLeft", 123 },
        { "ArrowRight", 124 },
        { "ArrowDown", 125 },
        { "ArrowUp", 126 },
        { "Help", 114 },
        { "Clear", 71 },
        { "NumpadEnter", 76 },
        // Modifiers
        { "ShiftLeft", 56 },
        { "ControlLeft", 59 },
        { "AltLeft", 58 },
        { "MetaLeft", 55 },
        { "CapsLock", 57 },
        // Function keys
        { "F1", 122 },
        { "F2", 120 },
        { "F3", 99 },
        { "F4", 118 },
        { "F5", 96 },
        { "F6", 97 },
        { "F7", 98 },
        { "F8", 100 },
        { "F9", 101 },
        { "F10", 109 },
        { "F11", 103 },
        { "F12", 111 },
        // Letters
        { "KeyA", 0 },
        { "KeyB", 11 },
        { "KeyC", 8 },
        { "KeyD", 2 },
        { "KeyE", 14 },
        { "KeyF", 3 },
        { "KeyG", 5 },
        { "KeyH", 4 },
        { "KeyI", 34 },
        { "KeyJ", 38 },
        { "KeyK", 40 },
        { "KeyL", 37 },
        { "KeyM", 46 },
        { "KeyN", 45 },
        { "KeyO", 31 },
        { "KeyP", 35 },
        { "KeyQ", 12 },
        { "KeyR", 15 },
        { "KeyS", 1 },
        { "KeyT", 17 },
        { "KeyU", 32 },
        { "KeyV", 9 },
        { "KeyW", 13 },
        { "KeyX", 7 },
        { "KeyY", 16 },
        { "KeyZ", 6 },
        // Numbers & Symbols
        { "Digit1", 18 },
        { "Digit2", 19 },
        { "Digit3", 20 },
        { "Digit4", 21 },
        { "Digit5", 23 },
        { "Digit6", 22 },
        { "Digit7", 26 },
        { "Digit8", 28 },
        { "Digit9", 25 },
        { "Digit0", 29 },
        { "Minus", 27 },
        { "Equal", 24 },
        { "Backquote", 50 },
        { "BracketLeft", 33 },
        { "BracketRight", 30 },
        { "Backslash", 42 },
        { "Semicolon", 41 },
        { "Quote", 39 },
        { "Comma", 43 },
        { "Period", 47 },
        { "Slash", 44 },
        // Numpad
        { "Numpad0", 82 },
        { "Numpad1", 83 },
        { "Numpad2", 84 },
        { "Numpad3", 85 },
        { "Numpad4", 86 },
        { "Numpad5", 87 },
        { "Numpad6", 88 },
        { "Numpad7", 89 },
        { "Numpad8", 91 },
        { "Numpad9", 92 },
        { "NumpadDecimal", 65 },
        { "NumpadDivide", 75 },
        { "NumpadMultiply", 67 },
        { "NumpadSubtract", 78 },
        { "NumpadAdd", 69 },
    };

    private static Dictionary<int, string> _reverse = BuildReverse(Mac);

    private static Dictionary<int, string> BuildReverse(Dictionary<string, int> source)
    {
        var dict = new Dictionary<int, string>();
        foreach (var kv in source)
            if (!dict.ContainsKey(kv.Value))
                dict[kv.Value] = kv.Key;
        return dict;
    }

    public static int ToLingo(string code)
    {
        if (Mac.TryGetValue(code, out var val))
            return val;
        if (code.Length == 1)
        {
            char c = char.ToUpperInvariant(code[0]);
            if (char.IsLetterOrDigit(c))
                return c;
        }
        if (code.StartsWith("Key") && code.Length == 4)
        {
            char c = code[3];
            if (char.IsLetter(c))
                return char.ToUpperInvariant(c);
        }
        if (code.StartsWith("Digit") && code.Length == 6)
        {
            char c = code[5];
            if (char.IsDigit(c))
                return c;
        }
        return 0;
    }

    public static string ToBlazor(int lingoCode)
    {
        if (_reverse.TryGetValue(lingoCode, out var code))
            return code;
        if (lingoCode >= 65 && lingoCode <= 90)
            return "Key" + (char)lingoCode;
        if (lingoCode >= 48 && lingoCode <= 57)
            return "Digit" + (char)lingoCode;
        return ((char)lingoCode).ToString();
    }
}
