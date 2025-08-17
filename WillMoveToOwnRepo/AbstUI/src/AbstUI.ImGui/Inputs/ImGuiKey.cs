using AbstUI.Inputs;

namespace AbstUI.ImGui.Inputs;

/// <summary>
/// Basic keyboard input placeholder for the ImGui backend.
/// </summary>
public class ImGuiKey : IAbstFrameworkKey
{
    public bool CommandDown => false;
    public bool ControlDown => false;
    public bool OptionDown => false;
    public bool ShiftDown => false;

    public bool KeyPressed(AbstUIKeyType key) => false;
    public bool KeyPressed(char key) => false;
    public bool KeyPressed(int keyCode) => false;

    public string Key => string.Empty;
    public int KeyCode => 0;
}

