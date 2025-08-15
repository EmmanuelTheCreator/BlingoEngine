using AbstUI.Inputs;

namespace AbstUI.Inputs
{
    /// <summary>
    /// Keyboard state abstraction provided by the framework.
    /// </summary>
    public interface IAbstUIFrameworkKey
    {
        /// <summary>Indicates whether the Command key is held.</summary>
        bool CommandDown { get; }
        bool ControlDown { get; }
        bool OptionDown { get; }
        bool ShiftDown { get; }
        /// <summary>Checks if a key of the given type is pressed.</summary>
        bool KeyPressed(AbstUIKeyType key);
        /// <summary>Checks if a character key is pressed.</summary>
        bool KeyPressed(char key);
        /// <summary>Checks if a key code is pressed.</summary>
        bool KeyPressed(int keyCode);
        /// <summary>Gets the last pressed key as a string.</summary>
        string Key { get; }
        /// <summary>Gets the numeric code of the last pressed key.</summary>
        int KeyCode { get; }
    }
}
