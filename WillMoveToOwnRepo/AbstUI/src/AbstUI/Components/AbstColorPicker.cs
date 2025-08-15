using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a color picker input.
    /// </summary>
    public class AbstColorPicker : AbstInputBase<IAbstFrameworkColorPicker>
    {
        public AColor Color { get => _framework.Color; set => _framework.Color = value; }
    }
}
