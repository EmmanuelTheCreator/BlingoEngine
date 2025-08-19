using AbstUI.Styles;

namespace AbstUI.Components;

/// <summary>
/// Base class for framework specific component factories.
/// </summary>
public abstract class AbstComponentFactoryBase
{
    protected AbstComponentFactoryBase(IAbstStyleManager styleManager, IAbstFontManager fontManager)
    {
        StyleManager = styleManager;
        FontManager = fontManager;
    }

    protected IAbstStyleManager StyleManager { get; }
    protected IAbstFontManager FontManager { get; }

    /// <summary>
    /// Applies a registered style to the component if available.
    /// </summary>
    protected void ApplyStyle<T>(T component) where T : IAbstNode
        => StyleManager.ApplyStyle(component);

    /// <summary>
    /// Performs common initialization for newly created components.
    /// </summary>
    protected void InitComponent<T>(T component) where T : IAbstNode
        => ApplyStyle(component);
}
