using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a color picker input.
    /// </summary>
    public class AbstUIGfxColorPicker : AbstUIGfxInputBase<IAbstUIFrameworkGfxColorPicker>
    {
        public AColor Color { get => _framework.Color; set => _framework.Color = value; }
    }
}
