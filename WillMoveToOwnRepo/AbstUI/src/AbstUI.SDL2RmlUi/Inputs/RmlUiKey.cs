using System.Collections.Generic;
using AbstUI.Inputs;
using RmlUiNet.Input;

namespace AbstUI.SDL2RmlUi.Inputs;

/// <summary>
/// Keyboard handling using the RmlUi.NET context.
/// </summary>
public class RmlUiKey : IAbstFrameworkKey
{
    private readonly HashSet<KeyIdentifier> _keys = new();
    private AbstKey? _lingoKey;
    private string _lastKey = string.Empty;
    private int _lastCode;
    private KeyModifier _modifierState;

    public void SetKeyObj(AbstKey key) => _lingoKey = key;

    public void ProcessKeyDown(KeyIdentifier key, KeyModifier modifiers)
    {
        _keys.Add(key);
        _modifierState = modifiers;
        _lastCode = (int)key;
        _lastKey = key.ToString();
        _lingoKey?.DoKeyDown();
    }

    public void ProcessKeyUp(KeyIdentifier key, KeyModifier modifiers)
    {
        _keys.Remove(key);
        _modifierState = modifiers;
        _lastCode = (int)key;
        _lastKey = key.ToString();
        _lingoKey?.DoKeyUp();
    }

    public bool CommandDown => (_modifierState & KeyModifier.KM_META) != 0;
    public bool ControlDown => (_modifierState & KeyModifier.KM_CTRL) != 0;
    public bool OptionDown => (_modifierState & KeyModifier.KM_ALT) != 0;
    public bool ShiftDown => (_modifierState & KeyModifier.KM_SHIFT) != 0;

    public bool KeyPressed(AbstUIKeyType key) => key switch
    {
        AbstUIKeyType.BACKSPACE => _keys.Contains(KeyIdentifier.KI_BACK),
        AbstUIKeyType.ENTER or AbstUIKeyType.RETURN => _keys.Contains(KeyIdentifier.KI_RETURN),
        AbstUIKeyType.QUOTE => _keys.Contains(KeyIdentifier.KI_OEM_7),
        AbstUIKeyType.SPACE => _keys.Contains(KeyIdentifier.KI_SPACE),
        AbstUIKeyType.TAB => _keys.Contains(KeyIdentifier.KI_TAB),
        _ => false
    };

    public bool KeyPressed(char key)
    {
        var c = char.ToUpperInvariant(key);
        if (c >= 'A' && c <= 'Z')
        {
            var enumValue = (KeyIdentifier)((int)KeyIdentifier.KI_A + (c - 'A'));
            return _keys.Contains(enumValue);
        }
        if (c >= '0' && c <= '9')
        {
            var enumValue = (KeyIdentifier)((int)KeyIdentifier.KI_0 + (c - '0'));
            return _keys.Contains(enumValue);
        }
        return false;
    }

    public bool KeyPressed(int keyCode) => _keys.Contains((KeyIdentifier)keyCode);

    public string Key => _lastKey;
    public int KeyCode => _lastCode;
}
