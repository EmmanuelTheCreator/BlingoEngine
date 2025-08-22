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
    private AbstKey? _lingoKey;
    private string _lastKey = string.Empty;
    private int _lastCode;

    public void SetKeyObj(AbstKey key) => _lingoKey = key;

    public void KeyDown(KeyboardEventArgs e)
    {
        _keys.Add(e.Key);
        _lastKey = e.Key;
        _lastCode = e.Key.Length == 1 ? char.ToUpperInvariant(e.Key[0]) : 0;
        _lingoKey?.DoKeyDown();
    }

    public void KeyUp(KeyboardEventArgs e)
    {
        _keys.Remove(e.Key);
        _lastKey = e.Key;
        _lastCode = e.Key.Length == 1 ? char.ToUpperInvariant(e.Key[0]) : 0;
        _lingoKey?.DoKeyUp();
    }

    public bool CommandDown => _keys.Contains("Meta");
    public bool ControlDown => _keys.Contains("Control");
    public bool OptionDown => _keys.Contains("Alt");
    public bool ShiftDown => _keys.Contains("Shift");

    public bool KeyPressed(AbstUIKeyType key) => key switch
    {
        AbstUIKeyType.BACKSPACE => _keys.Contains("Backspace"),
        AbstUIKeyType.ENTER or AbstUIKeyType.RETURN => _keys.Contains("Enter"),
        AbstUIKeyType.QUOTE => _keys.Contains("'"),
        AbstUIKeyType.SPACE => _keys.Contains(" "),
        AbstUIKeyType.TAB => _keys.Contains("Tab"),
        _ => false
    };

    public bool KeyPressed(char key) => _keys.Contains(key.ToString().ToUpperInvariant());

    public bool KeyPressed(int keyCode) => _keys.Contains(((char)keyCode).ToString().ToUpperInvariant());

    public string Key => _lastKey;
    public int KeyCode => _lastCode;
}
