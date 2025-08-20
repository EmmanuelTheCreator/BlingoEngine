using AbstUI.Primitives;

namespace AbstUI.Components.Buttons
{
    /// <summary>
    /// Framework specific button control.
    /// </summary>
    public interface IAbstFrameworkButton : IAbstFrameworkNode
    {
        AColor BorderColor { get; set; }
        AColor BackgroundColor { get; set; }
        AColor BackgroundHoverColor { get; set; }
        AColor TextColor { get; set; }
        string Text { get; set; }
        bool Enabled { get; set; }
        IAbstTexture2D? IconTexture { get; set; }

        event Action? Pressed;
    }
}
