using AbstUI.Components.Containers;
using AbstUI.LUnity.Components.Base;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Graphics;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkVerticalLineSeparator"/>.
/// </summary>
internal sealed class AbstUnityVerticalLineSeparator : AbstUnityComponent, IAbstFrameworkVerticalLineSeparator
{
    public AbstUnityVerticalLineSeparator() : base(CreateGameObject())
    {
    }

    private static GameObject CreateGameObject()
    {
        var go = new GameObject("VerticalLineSeparator");
        var image = go.AddComponent<Image>();
        image.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        return go;
    }
}
