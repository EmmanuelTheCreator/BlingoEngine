using AbstUI.Primitives;

namespace AbstUI.Components.Inputs;

/// <summary>
/// Base class for inputs that expose a selectable collection of items.
/// </summary>
public abstract class AbstSelectableCollection<TFramework> : AbstInputBase<TFramework>, IAbstSdlHasSeletectableCollectionStyle
    where TFramework : IAbstFrameworkNodeInput, IAbstSdlHasSeletectableCollectionStyle
{
    public string? ItemFont { get => _framework.ItemFont; set => _framework.ItemFont = value; }
    public int ItemFontSize { get => _framework.ItemFontSize; set => _framework.ItemFontSize = value; }
    public AColor ItemTextColor { get => _framework.ItemTextColor; set => _framework.ItemTextColor = value; }
    public AColor ItemSelectedTextColor { get => _framework.ItemSelectedTextColor; set => _framework.ItemSelectedTextColor = value; }
    public AColor ItemSelectedBackgroundColor { get => _framework.ItemSelectedBackgroundColor; set => _framework.ItemSelectedBackgroundColor = value; }
    public AColor ItemSelectedBorderColor { get => _framework.ItemSelectedBorderColor; set => _framework.ItemSelectedBorderColor = value; }
    public AColor ItemHoverTextColor { get => _framework.ItemHoverTextColor; set => _framework.ItemHoverTextColor = value; }
    public AColor ItemHoverBackgroundColor { get => _framework.ItemHoverBackgroundColor; set => _framework.ItemHoverBackgroundColor = value; }
    public AColor ItemHoverBorderColor { get => _framework.ItemHoverBorderColor; set => _framework.ItemHoverBorderColor = value; }
    public AColor ItemPressedTextColor { get => _framework.ItemPressedTextColor; set => _framework.ItemPressedTextColor = value; }
    public AColor ItemPressedBackgroundColor { get => _framework.ItemPressedBackgroundColor; set => _framework.ItemPressedBackgroundColor = value; }
    public AColor ItemPressedBorderColor { get => _framework.ItemPressedBorderColor; set => _framework.ItemPressedBorderColor = value; }

}

