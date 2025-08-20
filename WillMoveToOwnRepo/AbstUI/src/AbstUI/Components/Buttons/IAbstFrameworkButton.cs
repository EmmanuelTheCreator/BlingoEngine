using AbstUI.Primitives;

namespace AbstUI.Components.Buttons
{
    /// <summary>
    /// Framework specific button control.
    /// </summary>
    public interface IAbstFrameworkButton : IAbstFrameworkNode
    {
        string Text { get; set; }
        bool Enabled { get; set; }
        IAbstTexture2D? IconTexture { get; set; }

        event Action? Pressed;
    }
}
