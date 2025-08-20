using AbstUI.Primitives;
using AbstUI.Styles.Components;

namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Engine level wrapper for a color picker input.
    /// </summary>
    public class AbstColorPicker : AbstInputBase<IAbstFrameworkColorPicker>
    {
        public AColor Color { get => _framework.Color; set => _framework.Color = value; }

        protected override void OnSetStyle(AbstComponentStyle componentStyle)
        {
            base.OnSetStyle(componentStyle);
            if (componentStyle is AbstColorPickerStyle style && style.DefaultColor.HasValue)
                Color = style.DefaultColor.Value;
        }

        protected override void OnGetStyle(AbstComponentStyle componentStyle)
        {
            base.OnGetStyle(componentStyle);
            if (componentStyle is AbstColorPickerStyle style)
                style.DefaultColor = Color;
        }
    }
}
