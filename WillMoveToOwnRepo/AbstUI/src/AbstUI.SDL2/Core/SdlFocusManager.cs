using AbstUI.SDL2.Components;

namespace AbstUI.SDL2.Core;

/// <summary>
/// Manages which SDL component currently owns keyboard focus.
/// Instances are registered through dependency injection and shared
/// via the root context.
/// </summary>
public class SdlFocusManager
{
    private ISdlFocusable? _focused;

    /// <summary>
    /// Assigns focus to the given component, notifying the previous
    /// and new targets accordingly.
    /// </summary>
    public void SetFocus(ISdlFocusable? component)
    {
        if (_focused == component)
            return;
        _focused?.SetFocus(false);
        _focused = component;
        _focused?.SetFocus(true);
    }

    /// <summary>
    /// Gets the currently focused component.
    /// </summary>
    public ISdlFocusable? Focused => _focused;
}
