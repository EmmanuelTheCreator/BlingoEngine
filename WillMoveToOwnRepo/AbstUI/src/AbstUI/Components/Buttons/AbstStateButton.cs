using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Styles.Components;

namespace AbstUI.Components.Buttons
{
    /// <summary>
    /// Engine level wrapper for a state (toggle) button.
    /// </summary>
    //public class AbstStateButton : AbstInputBase<IAbstFrameworkStateButton>
    public class AbstStateButton : AbstNodeBase<IAbstFrameworkStateButton>
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
                if (_framework.TextureOn == value) return;
                _textureOnSubscription?.Release();
                if (value != null)
                {
                    _framework.TextureOn = value;
                    _textureOnSubscription = _framework.TextureOn.AddUser(this);
                }
                else                 
                    _framework.TextureOn = null;
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
                if (_framework.TextureOff == value) return;
                _textureOffSubscription?.Release();
                if (value != null)
                {
                    _framework.TextureOff = value;
                    _textureOffSubscription = _framework.TextureOff.AddUser(this);
                }
                else
                    _framework.TextureOff = null;
            }
        }
        protected override void OnSetStyle(AbstComponentStyle componentStyle)
        {
            base.OnSetStyle(componentStyle);
            return;
            if (componentStyle is AbstInputStyle style)
            {
                if (style.FontSize.HasValue)
                {
                    if (_framework is IAbstFrameworkInputText text)
                        text.FontSize = style.FontSize.Value;
                    if (_framework is IAbstFrameworkInputNumber number)
                        number.FontSize = style.FontSize.Value;
                }

                if (style.Font is not null && _framework is IAbstFrameworkInputText textWithFont)
                    textWithFont.Font = style.Font;

                if (style.TextColor.HasValue && _framework is IAbstFrameworkInputText textWithColor)
                    textWithColor.TextColor = style.TextColor.Value;
            }
        }

        protected override void OnGetStyle(AbstComponentStyle componentStyle)
        {
            base.OnGetStyle(componentStyle);
            return;
            if (componentStyle is AbstInputStyle style)
            {
                if (_framework is IAbstFrameworkInputText text)
                {
                    style.FontSize = text.FontSize;
                    style.Font = text.Font;
                    style.TextColor = text.TextColor;
                }

                if (_framework is IAbstFrameworkInputNumber number)
                    style.FontSize = number.FontSize;
            }
        }

        /// <summary>Whether the control is enabled.</summary>
        public bool Enabled { get => _framework.Enabled; set => _framework.Enabled = value; }

        /// <summary>Event raised when the input value changes.</summary>
        public event Action? ValueChanged
        {
            add { _framework.ValueChanged += value; }
            remove { _framework.ValueChanged -= value; }
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
