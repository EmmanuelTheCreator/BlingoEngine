using AbstUI.Inputs;

namespace AbstUI.LUnity.Inputs;

/// <summary>
/// Global key handler for Unity, bridging <see cref="AbstKey"/> to Unity's input system.
/// </summary>
public sealed class GlobalUnityAbstKey : AbstKey, IAbstGlobalKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalUnityAbstKey"/> class.
    /// </summary>
    public GlobalUnityAbstKey()
    {
        var framework = new AbstUIUnityKey();
        framework.SetKeyObj(this);
        Init(framework);
    }
}
