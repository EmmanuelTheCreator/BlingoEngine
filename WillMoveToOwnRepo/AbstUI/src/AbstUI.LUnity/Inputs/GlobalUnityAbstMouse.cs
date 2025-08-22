using System;
using AbstUI.Inputs;
using AbstUI.Primitives;

namespace AbstUI.LUnity.Inputs;

/// <summary>
/// Global mouse implementation for Unity.
/// </summary>
public sealed class GlobalUnityAbstMouse : AbstMouse, IAbstGlobalMouse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalUnityAbstMouse"/> class.
    /// </summary>
    public GlobalUnityAbstMouse()
        : base(CreateFrameworkMouse(out var framework))
    {
        framework.ReplaceMouseObj(this);
    }

    private static IAbstFrameworkMouse CreateFrameworkMouse(out AbstUnityGlobalMouse framework)
    {
        framework = new AbstUnityGlobalMouse();
        return framework;
    }
}

/// <summary>
/// Framework mouse used by <see cref="GlobalUnityAbstMouse"/>.
/// </summary>
public sealed class AbstUnityGlobalMouse : AbstUnityMouse<GlobalUnityAbstMouse, AbstMouseEvent>
{
    private static GlobalUnityAbstMouse? _mouse;

    public AbstUnityGlobalMouse()
        : base(new Lazy<GlobalUnityAbstMouse>(() => _mouse!))
    {
    }

    /// <inheritdoc />
    public new void ReplaceMouseObj(IAbstMouse lingoMouse)
    {
        _mouse = (GlobalUnityAbstMouse)lingoMouse;
        base.ReplaceMouseObj(lingoMouse);
    }
}
