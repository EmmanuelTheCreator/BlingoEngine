using AbstUI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkLayoutWrapper"/>.
/// </summary>
internal class AbstUnityLayoutWrapper : AbstUnityComponent, IAbstFrameworkLayoutWrapper
{
    public AbstUnityLayoutWrapper() : base(CreateGameObject())
    {
    }

    private static GameObject CreateGameObject()
    {
        var go = new GameObject("LayoutWrapper", typeof(RectTransform));
        go.AddComponent<Image>();
        return go;
    }
}
