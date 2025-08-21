using AbstUI.Primitives;

namespace AbstUI.Components.Inputs;

public interface IAbstSdlHasSeletectableCollectionStyle
{
    string? ItemFont { get; set; }
    int ItemFontSize { get; set; }
    AColor ItemTextColor { get; set; }
    AColor ItemSelectedTextColor { get; set; }
    AColor ItemSelectedBackgroundColor { get; set; }
    AColor ItemSelectedBorderColor { get; set; }
    AColor ItemHoverTextColor { get; set; }
    AColor ItemHoverBackgroundColor { get; set; }
    AColor ItemHoverBorderColor { get; set; }
    AColor ItemPressedTextColor { get; set; }
    AColor ItemPressedBackgroundColor { get; set; }
    AColor ItemPressedBorderColor { get; set; }
}
