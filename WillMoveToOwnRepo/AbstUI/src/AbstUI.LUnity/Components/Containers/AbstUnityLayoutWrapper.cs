using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Containers;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkLayoutWrapper"/> that wraps a non-layout
/// component so it can participate in absolute positioning containers like <see cref="AbstPanel"/>.
/// </summary>
internal class AbstUnityLayoutWrapper : AbstUnityComponent, IAbstFrameworkLayoutWrapper, IFrameworkFor<AbstLayoutWrapper>
{
    private readonly AbstLayoutWrapper _layoutWrapper;

    /// <summary>
    /// Creates a new layout wrapper for the given AbstUI layout wrapper.
    /// The wrapped content's <see cref="GameObject"/> is parented to this wrapper so it can be
    /// positioned independently inside Unity.
    /// </summary>
    public AbstUnityLayoutWrapper(AbstLayoutWrapper layoutWrapper) : base(CreateGameObject())
    {
        _layoutWrapper = layoutWrapper;
        layoutWrapper.Init(this);

        var contentNode = layoutWrapper.Content.FrameworkObj.FrameworkNode;
        if (contentNode is GameObject go)
        {
            go.transform.SetParent(GameObject.transform, false);
        }
    }

    public new float Width
    {
        get => _layoutWrapper.Width;
        set
        {
            _layoutWrapper.Width = value;
            base.Width = value;
        }
    }

    public new float Height
    {
        get => _layoutWrapper.Height;
        set
        {
            _layoutWrapper.Height = value;
            base.Height = value;
        }
    }

    private static GameObject CreateGameObject()
    {
        var go = new GameObject("LayoutWrapper", typeof(RectTransform));
        go.AddComponent<Image>();
        return go;
    }
}
