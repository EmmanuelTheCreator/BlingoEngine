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

    protected AbstUnityComponent(GameObject? gameObject = null)
    {
        GameObject = gameObject ?? new GameObject();
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

    private float _width;
    private float _height;

    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            var scale = GameObject.transform.localScale;
            scale.x = value;
            GameObject.transform.localScale = scale;
        }
    }

    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            var scale = GameObject.transform.localScale;
            scale.y = value;
            GameObject.transform.localScale = scale;
        }
    }

    public AMargin Margin { get; set; } = AMargin.Zero;

    private float _x;
    private float _y;

    public float X
    {
        get => _x;
        set
        {
            _x = value;
            var pos = GameObject.transform.position;
            pos.x = value;
            GameObject.transform.position = pos;
        }
    }

    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            var pos = GameObject.transform.position;
            pos.y = value;
            GameObject.transform.position = pos;
        }
    }

    public object FrameworkNode => GameObject;

    public virtual void Dispose()
    {
        UnityEngine.Object.Destroy(GameObject);
    }
}
