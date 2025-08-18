using AbstUI.Components;
using AbstUI.Primitives;
using UnityEngine;

namespace AbstUI.LUnity.Components;

/// <summary>
/// Base Unity component providing common node properties.
/// </summary>
internal class AbstUnityComponent : IAbstFrameworkLayoutNode
{
    protected readonly GameObject GameObject;

    public AbstUnityComponent()
    {
        GameObject = new GameObject();
    }

    public string Name
    {
        get => GameObject.name;
        set => GameObject.name = value;
    }

    public bool Visibility
    {
        get => GameObject.activeSelf;
        set => GameObject.SetActive(value);
    }

    public float Width { get; set; }
    public float Height { get; set; }

    public AMargin Margin { get; set; } = AMargin.Zero;

    public float X { get; set; }
    public float Y { get; set; }

    public object FrameworkNode => GameObject;

    public virtual void Dispose()
    {
        UnityEngine.Object.Destroy(GameObject);
    }
}
