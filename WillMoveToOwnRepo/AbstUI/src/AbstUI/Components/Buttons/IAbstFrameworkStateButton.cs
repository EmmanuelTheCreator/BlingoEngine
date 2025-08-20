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

        /// <summary>Color of the border when in normal state.</summary>
        AColor BorderColor { get; set; }

        /// <summary>Color of the border when the mouse hovers the button.</summary>
        AColor BorderHoverColor { get; set; }

        /// <summary>Color of the border while the button is pressed or active.</summary>
        AColor BorderPressedColor { get; set; }

        /// <summary>Background color in normal state.</summary>
        AColor BackgroundColor { get; set; }

        /// <summary>Background color while the mouse hovers the button.</summary>
        AColor BackgroundHoverColor { get; set; }

        /// <summary>Background color when the button is pressed or active.</summary>
        AColor BackgroundPressedColor { get; set; }

        /// <summary>Text color of the button.</summary>
        AColor TextColor { get; set; }

        /// <summary>Icon texture displayed when the button is toggled on.</summary>
        IAbstTexture2D? TextureOn { get; set; }

        /// <summary>Whether the button is toggled on.</summary>
        bool IsOn { get; set; }

        /// <summary>Icon texture displayed when the button is toggled off.</summary>
        IAbstTexture2D? TextureOff { get; set; }
    }
}
