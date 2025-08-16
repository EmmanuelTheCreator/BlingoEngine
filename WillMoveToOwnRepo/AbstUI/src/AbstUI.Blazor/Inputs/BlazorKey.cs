using AbstUI.Inputs;

namespace AbstUI.Blazor.Inputs;

public class BlazorKey : IAbstFrameworkKey
{
    private readonly HashSet<Blazor.Blazor_Keycode> _keys = new();
    private AbstKey? _lingoKey;
    private string _lastKey = string.Empty;
    private int _lastCode;

    public void SetKeyObj(AbstKey key) => _lingoKey = key;

    public void ProcessEvent(Blazor.Blazor_Event e)
    {
        if (e.type == Blazor.Blazor_EventType.Blazor_KEYDOWN && e.key.repeat == 0)
        {
            _keys.Add(e.key.keysym.sym);
            _lastCode = (int)e.key.keysym.sym;
            _lastKey = Blazor.Blazor_GetKeyName(e.key.keysym.sym);
            _lingoKey?.DoKeyDown();
        }
        else if (e.type == Blazor.Blazor_EventType.Blazor_KEYUP)
        {
            _keys.Remove(e.key.keysym.sym);
            _lastCode = (int)e.key.keysym.sym;
            _lastKey = Blazor.Blazor_GetKeyName(e.key.keysym.sym);
            _lingoKey?.DoKeyUp();
        }
    }

    public bool CommandDown => (Blazor.Blazor_GetModState() & Blazor.Blazor_Keymod.KMOD_GUI) != 0;
    public bool ControlDown => (Blazor.Blazor_GetModState() & Blazor.Blazor_Keymod.KMOD_CTRL) != 0;
    public bool OptionDown => (Blazor.Blazor_GetModState() & Blazor.Blazor_Keymod.KMOD_ALT) != 0;
    public bool ShiftDown => (Blazor.Blazor_GetModState() & Blazor.Blazor_Keymod.KMOD_SHIFT) != 0;

    public bool KeyPressed(AbstUIKeyType key) => key switch
    {
        AbstUIKeyType.BACKSPACE => _keys.Contains(Blazor.Blazor_Keycode.BlazorK_BACKSPACE),
        AbstUIKeyType.ENTER or AbstUIKeyType.RETURN => _keys.Contains(Blazor.Blazor_Keycode.BlazorK_RETURN),
        AbstUIKeyType.QUOTE => _keys.Contains(Blazor.Blazor_Keycode.BlazorK_QUOTE),
        AbstUIKeyType.SPACE => _keys.Contains(Blazor.Blazor_Keycode.BlazorK_SPACE),
        AbstUIKeyType.TAB => _keys.Contains(Blazor.Blazor_Keycode.BlazorK_TAB),
        _ => false
    };

    public bool KeyPressed(char key) => _keys.Contains((Blazor.Blazor_Keycode)char.ToUpperInvariant(key));

    public bool KeyPressed(int keyCode) => _keys.Contains((Blazor.Blazor_Keycode)keyCode);

    public string Key => _lastKey;
    public int KeyCode => _lastCode;
}
