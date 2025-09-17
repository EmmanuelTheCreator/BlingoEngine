using AbstUI.Inputs;
using Microsoft.AspNetCore.Components.Web;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Inputs;

/// <summary>
/// Keyboard wrapper using standard DOM events provided by Blazor.
/// </summary>
public class BlazorKey : IAbstFrameworkKey, IFrameworkFor<AbstKey>
{
    private readonly HashSet<string> _keys = new();
    private readonly HashSet<string> _codes = new();
    private AbstKey? _blingoKey;
    private string _lastKey = string.Empty;
    private int _lastCode;

    public void SetKeyObj(AbstKey key) => _blingoKey = key;

    public void KeyDown(KeyboardEventArgs e)
    {
        _keys.Add(e.Key);
        _codes.Add(e.Code);
        _lastKey = e.Key;
        _lastCode = BlazorKeyCodeMap.ToBlingo(e.Code);
        _blingoKey?.DoKeyDown();
    }

    public void KeyUp(KeyboardEventArgs e)
    {
        _keys.Remove(e.Key);
        _codes.Remove(e.Code);
        _lastKey = e.Key;
        _lastCode = BlazorKeyCodeMap.ToBlingo(e.Code);
        _blingoKey?.DoKeyUp();
    }

    public bool CommandDown => _keys.Contains("Meta");
    public bool ControlDown => _keys.Contains("Control");
    public bool OptionDown => _keys.Contains("Alt");
    public bool ShiftDown => _keys.Contains("Shift");

    public bool KeyPressed(AbstUIKeyType key) => key switch
    {
        AbstUIKeyType.BACKSPACE => _codes.Contains("Backspace"),
        AbstUIKeyType.ENTER or AbstUIKeyType.RETURN => _codes.Contains("Enter"),
        AbstUIKeyType.QUOTE => _codes.Contains("Quote"),
        AbstUIKeyType.SPACE => _codes.Contains("Space"),
        AbstUIKeyType.TAB => _codes.Contains("Tab"),
        _ => false
    };
    public bool KeyPressed(char key)
        => _codes.Contains(BlazorKeyCodeMap.ToBlazor(char.IsLetter(key) ? char.ToUpperInvariant(key) : key));

    public bool KeyPressed(int keyCode)
        => _codes.Contains(BlazorKeyCodeMap.ToBlazor(keyCode));

    public string Key => _lastKey;
    public int KeyCode => _lastCode;
}

