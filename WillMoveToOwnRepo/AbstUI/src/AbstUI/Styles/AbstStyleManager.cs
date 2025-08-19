using System;
using System.Collections.Generic;
using AbstUI.Components;

namespace AbstUI.Styles;

/// <summary>
/// Manages default styles for AbstUI components.
/// </summary>
public interface IAbstStyleManager
{
    /// <summary>
    /// Registers a default style for the given component type.
    /// </summary>
    /// <typeparam name="TComponent">Component type.</typeparam>
    /// <param name="style">Style to register.</param>
    void Register<TComponent>(AbstComponentStyle style) where TComponent : IAbstNode;

    /// <summary>
    /// Applies a registered style to the specified component if available.
    /// </summary>
    /// <param name="component">Component to style.</param>
    void ApplyStyle(IAbstNode component);
}

public class AbstStyleManager : IAbstStyleManager
{
    private readonly Dictionary<Type, AbstComponentStyle> _styles = new();

    public void Register<TComponent>(AbstComponentStyle style) where TComponent : IAbstNode
        => _styles[typeof(TComponent)] = style;

    public void ApplyStyle(IAbstNode component)
    {
        if (_styles.TryGetValue(component.GetType(), out var style))
        {
            if (component is AbstNodeBase<IAbstFrameworkNode> node)
            {
                node.SetStyle(style);
            }
        }
    }
}
