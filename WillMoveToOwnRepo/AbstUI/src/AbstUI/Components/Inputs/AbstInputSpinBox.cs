using AbstUI.Primitives;

namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Engine level wrapper for a spinbox input.
    /// </summary>
    public class AbstInputSpinBox : AbstInputBase<IAbstFrameworkSpinBox>, IHasTextBackgroundBorderColor
    {
        public float Value { get => _framework.Value; set => _framework.Value = value; }
        public float Min { get => _framework.Min; set => _framework.Min = value; }
        public float Max { get => _framework.Max; set => _framework.Max = value; }
        public AColor TextColor { get => ((IHasTextBackgroundBorderColor)_framework).TextColor; set => ((IHasTextBackgroundBorderColor)_framework).TextColor = value; }
        public AColor BackgroundColor { get => ((IHasTextBackgroundBorderColor)_framework).BackgroundColor; set => ((IHasTextBackgroundBorderColor)_framework).BackgroundColor = value; }
        public AColor BorderColor { get => ((IHasTextBackgroundBorderColor)_framework).BorderColor; set => ((IHasTextBackgroundBorderColor)_framework).BorderColor = value; }
    }
}
