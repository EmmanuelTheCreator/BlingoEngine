using AbstUI.Primitives;

namespace AbstUI.Components.Inputs;

public interface IHasTextBackgroundBorderColor
{
    AColor TextColor { get; set; }
    AColor BackgroundColor { get; set; }
    AColor BorderColor { get; set; }
}
