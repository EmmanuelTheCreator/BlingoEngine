using AbstUI.Components.Containers;
using AbstUI.LUnity.Components.Base;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Graphics;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkHorizontalLineSeparator"/>.
/// </summary>
internal sealed class AbstUnityHorizontalLineSeparator : AbstUnityComponent, IAbstFrameworkHorizontalLineSeparator
{
    public AbstUnityHorizontalLineSeparator() : base(CreateGameObject())
    {
    }

    private static GameObject CreateGameObject()
    {
        var go = new GameObject("HorizontalLineSeparator");
        var image = go.AddComponent<Image>();
        image.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        return go;
    }
}
