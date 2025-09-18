using AbstUI.Primitives;
using AbstUI.Styles.Components;

namespace AbstUI.Components.Buttons
{
    /// <summary>
    /// Engine level wrapper for a button control.
    /// </summary>
    public class AbstButton : AbstNodeBase<IAbstFrameworkButton>
    {
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        /// <summary>Color of the border when in normal state.</summary>
        public AColor BorderColor { get => _framework.BorderColor; set => _framework.BorderColor = value; }
        /// <summary>Border color when hovered.</summary>
        //public AColor BorderHoverColor { get => _framework.BorderHoverColor; set => _framework.BorderHoverColor = value; }
        /// <summary>Border color when pressed or active.</summary>
       // public AColor BorderPressedColor { get => _framework.BorderPressedColor; set => _framework.BorderPressedColor = value; }
        /// <summary>Background color in normal state.</summary>
        public AColor BackgroundColor { get => _framework.BackgroundColor; set => _framework.BackgroundColor = value; }
        /// <summary>Background color on hover.</summary>
        public AColor BackgroundHoverColor { get => _framework.BackgroundHoverColor; set => _framework.BackgroundHoverColor = value; }
        /// <summary>Background color when pressed or active.</summary>
       // public AColor BackgroundPressedColor { get => _framework.BackgroundPressedColor; set => _framework.BackgroundPressedColor = value; }
        /// <summary>Color of the text.</summary>
        public AColor TextColor { get => _framework.TextColor; set => _framework.TextColor = value; }
        public bool Enabled { get => _framework.Enabled; set => _framework.Enabled = value; }
        private IAbstUITextureUserSubscription? _iconTextureSubscription;
        public IAbstTexture2D? IconTexture
        {
            get => _framework.IconTexture;
            set
            {
                if (_framework.IconTexture == value) return;
                _iconTextureSubscription?.Release();
                _iconTextureSubscription = value?.AddUser(this);
                _framework.IconTexture = value;
            }
        }

        public event Action? Pressed
        {
            add { _framework.Pressed += value; }
            remove { _framework.Pressed -= value; }
        }

        public override void Dispose()
        {
            _iconTextureSubscription?.Release();
            _iconTextureSubscription = null;
            _framework.IconTexture = null;
            base.Dispose();
        }
        protected override void OnSetStyle(AbstComponentStyle componentStyle)
        {
            base.OnSetStyle(componentStyle);
            return;
            //if (componentStyle is AbstInputStyle style)
            //{
            //    if (style.FontSize.HasValue)
            //    {
            //        if (_framework is IAbstFrameworkInputText text)
            //            text.FontSize = style.FontSize.Value;
            //        if (_framework is IAbstFrameworkInputNumber number)
            //            number.FontSize = style.FontSize.Value;
            //    }

            //    if (style.Font is not null && _framework is IAbstFrameworkInputText textWithFont)
            //        textWithFont.Font = style.Font;

            //    if (style.TextColor.HasValue && _framework is IAbstFrameworkInputText textWithColor)
            //        textWithColor.TextColor = style.TextColor.Value;
            //}
        }

        protected override void OnGetStyle(AbstComponentStyle componentStyle)
        {
            base.OnGetStyle(componentStyle);
            return;
            //if (componentStyle is AbstInputStyle style)
            //{
            //    if (_framework is IAbstFrameworkInputText text)
            //    {
            //        style.FontSize = text.FontSize;
            //        style.Font = text.Font;
            //        style.TextColor = text.TextColor;
            //    }

            //    if (_framework is IAbstFrameworkInputNumber number)
            //        style.FontSize = number.FontSize;
            //}
        }
    }
}
