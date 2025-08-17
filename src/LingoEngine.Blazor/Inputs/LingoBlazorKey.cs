using AbstUI.Inputs;
using LingoEngine.Inputs;

namespace LingoEngine.Blazor.Inputs;

public class LingoBlazorKey : ILingoFrameworkKey
{
    public bool CommandDown => false;
    public bool ControlDown => false;
    public bool OptionDown => false;
    public bool ShiftDown => false;
    public string Key => string.Empty;
    public int KeyCode => 0;

    public bool KeyPressed(AbstUIKeyType key) => false;
    public bool KeyPressed(char key) => false;
    public bool KeyPressed(int keyCode) => false;
}
