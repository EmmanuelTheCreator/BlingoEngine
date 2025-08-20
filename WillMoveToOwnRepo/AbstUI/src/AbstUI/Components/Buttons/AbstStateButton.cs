using AbstUI.Components.Inputs;
using AbstUI.Primitives;

namespace AbstUI.Components.Buttons
{
    /// <summary>
    /// Engine level wrapper for a state (toggle) button.
    /// </summary>
    public class AbstStateButton : AbstInputBase<IAbstFrameworkStateButton>
    {
        /// <summary>Displayed text on the button.</summary>
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        /// <summary>Color of the border when in normal state.</summary>
        public AColor BorderColor { get => _framework.BorderColor; set => _framework.BorderColor = value; }
        /// <summary>Border color when hovered.</summary>
        public AColor BorderHoverColor { get => _framework.BorderHoverColor; set => _framework.BorderHoverColor = value; }
        /// <summary>Border color when pressed or active.</summary>
        public AColor BorderPressedColor { get => _framework.BorderPressedColor; set => _framework.BorderPressedColor = value; }
        /// <summary>Background color in normal state.</summary>
        public AColor BackgroundColor { get => _framework.BackgroundColor; set => _framework.BackgroundColor = value; }
        /// <summary>Background color on hover.</summary>
        public AColor BackgroundHoverColor { get => _framework.BackgroundHoverColor; set => _framework.BackgroundHoverColor = value; }
        /// <summary>Background color when pressed or active.</summary>
        public AColor BackgroundPressedColor { get => _framework.BackgroundPressedColor; set => _framework.BackgroundPressedColor = value; }
        /// <summary>Color of the text.</summary>
        public AColor TextColor { get => _framework.TextColor; set => _framework.TextColor = value; }
        /// <summary>Icon texture displayed when the button is on.</summary>
        private IAbstUITextureUserSubscription? _textureOnSubscription;
        public IAbstTexture2D? TextureOn
        {
            get => _framework.TextureOn;
            set
            {
                _textureOnSubscription?.Release();
                _textureOnSubscription = value?.AddUser(this);
                _framework.TextureOn = value;
            }
        }
        /// <summary>Current toggle state.</summary>
        public bool IsOn { get => _framework.IsOn; set => _framework.IsOn = value; }
        private IAbstUITextureUserSubscription? _textureOffSubscription;
        public IAbstTexture2D? TextureOff
        {
            get => _framework.TextureOff;
            set
            {
                _textureOffSubscription?.Release();
                _textureOffSubscription = value?.AddUser(this);
                _framework.TextureOff = value;
            }
        }

        public override void Dispose()
        {
            _textureOnSubscription?.Release();
            _textureOffSubscription?.Release();
            _textureOnSubscription = null;
            _textureOffSubscription = null;
            _framework.TextureOn = null;
            _framework.TextureOff = null;
            base.Dispose();
        }
    }
}
