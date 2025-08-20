using AbstUI.Components.Inputs;
using AbstUI.Primitives;

namespace AbstUI.Components.Buttons
{
    /// <summary>
    /// Framework specific toggle button control.
    /// </summary>
    public interface IAbstFrameworkStateButton : IAbstFrameworkNodeInput
    {
        /// <summary>Displayed text on the button.</summary>
        string Text { get; set; }
        /// <summary>Icon texture displayed on the button.</summary>
        IAbstTexture2D? TextureOn { get; set; }
        /// <summary>Whether the button is toggled on.</summary>
        bool IsOn { get; set; }
        IAbstTexture2D? TextureOff { get; set; }
    }
}
