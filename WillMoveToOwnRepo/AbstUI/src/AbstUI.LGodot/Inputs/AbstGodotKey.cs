using AbstUI.Inputs;
using Godot;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Inputs;

public partial class AbstGodotKey : Node, IAbstFrameworkKey, IFrameworkFor<AbstKey>
{
    private readonly List<Key> _pressed = new();
    private Lazy<AbstKey> _blingoKey;
    private string _lastKey = string.Empty;
    private int _lastCode;

    public AbstGodotKey(Node root, Lazy<AbstKey> key)
    {
        Name = "KeyConnector";
        _blingoKey = key;
        root.AddChild(this);
    }

    public void SetKeyObj(AbstKey key)
    {
        _blingoKey = new Lazy<AbstKey>(() => key);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey k)
        {
            if (k.Pressed)
            {
                if (!_pressed.Contains(k.Keycode))
                    _pressed.Add(k.Keycode);
                _lastCode = GodotKeyCodeMap.ToBlingo(k.Keycode);
                _lastKey = k.KeyLabel.ToString();
                _blingoKey.Value.DoKeyDown();
            }
            else
            {
                _pressed.Remove(k.Keycode);
                _lastCode = GodotKeyCodeMap.ToBlingo(k.Keycode);
                _lastKey = k.KeyLabel.ToString();
                _blingoKey.Value.DoKeyUp();
            }
        }
    }

    public bool CommandDown => Input.IsKeyPressed(Godot.Key.Meta);
    public bool ControlDown => Input.IsKeyPressed(Godot.Key.Ctrl);
    public bool OptionDown => Input.IsKeyPressed(Godot.Key.Alt);
    public bool ShiftDown => Input.IsKeyPressed(Godot.Key.Shift);

    public bool KeyPressed(AbstUIKeyType key) => key switch
    {
        AbstUIKeyType.BACKSPACE => _pressed.Contains(Godot.Key.Backspace),
        AbstUIKeyType.ENTER or AbstUIKeyType.RETURN => _pressed.Contains(Godot.Key.Enter),
        AbstUIKeyType.QUOTE => _pressed.Contains(Godot.Key.Quoteleft),
        AbstUIKeyType.SPACE => _pressed.Contains(Godot.Key.Space),
        AbstUIKeyType.TAB => _pressed.Contains(Godot.Key.Tab),
        _ => false
    };

    public bool KeyPressed(char key)
        => _pressed.Contains(GodotKeyCodeMap.ToGodot(char.IsLetter(key) ? char.ToUpperInvariant(key) : key));

    public bool KeyPressed(int keyCode)
        => _pressed.Contains(GodotKeyCodeMap.ToGodot(keyCode));

    public string Key => _lastKey;
    public int KeyCode => _lastCode;
}

