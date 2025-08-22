using AbstUI.Inputs;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LUnity.Inputs;

public class AbstUIUnityKey : IAbstFrameworkKey, IFrameworkFor<AbstKey>
{
    private readonly HashSet<int> _keys = new();
    private AbstKey? _lingoKey;
    private string _lastKey = string.Empty;
    private int _lastCode;

    public void SetKeyObj(AbstKey key) => _lingoKey = key;

    public void KeyDown(int keyCode, string keyName)
    {
        _keys.Add(keyCode);
        _lastCode = keyCode;
        _lastKey = keyName;
        _lingoKey?.DoKeyDown();
    }

    public void KeyUp(int keyCode, string keyName)
    {
        _keys.Remove(keyCode);
        _lastCode = keyCode;
        _lastKey = keyName;
        _lingoKey?.DoKeyUp();
    }

    private static bool ContainsAny(HashSet<int> set, params int[] codes)
    {
        foreach (var c in codes)
            if (set.Contains(c)) return true;
        return false;
    }

    public bool CommandDown => ContainsAny(_keys, 310, 309);
    public bool ControlDown => ContainsAny(_keys, 17);
    public bool OptionDown => ContainsAny(_keys, 18);
    public bool ShiftDown => ContainsAny(_keys, 16);

    public bool KeyPressed(AbstUIKeyType key) => key switch
    {
        AbstUIKeyType.BACKSPACE => _keys.Contains(8),
        AbstUIKeyType.ENTER or AbstUIKeyType.RETURN => _keys.Contains(13),
        AbstUIKeyType.QUOTE => _keys.Contains(39),
        AbstUIKeyType.SPACE => _keys.Contains(32),
        AbstUIKeyType.TAB => _keys.Contains(9),
        _ => false
    };

    public bool KeyPressed(char key) => _keys.Contains(char.ToUpperInvariant(key));

    public bool KeyPressed(int keyCode) => _keys.Contains(keyCode);

    public string Key => _lastKey;
    public int KeyCode => _lastCode;
}
