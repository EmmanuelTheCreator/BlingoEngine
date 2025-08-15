using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific button control.
    /// </summary>
    public interface IAbstUIFrameworkGfxButton : IAbstUIFrameworkGfxNode
    {
        string Text { get; set; }
        bool Enabled { get; set; }
        IAbstUITexture2D? IconTexture { get; set; }

        event Action? Pressed;
    }
}
