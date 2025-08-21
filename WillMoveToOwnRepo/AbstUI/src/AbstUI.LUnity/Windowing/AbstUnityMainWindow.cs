using UnityEngine;
using AbstUI.Primitives;
using AbstUI.Windowing;

namespace AbstUI.LUnity.Windowing;

public class AbstUnityMainWindow : IAbstFrameworkMainWindow
{
    private AbstMainWindow _window = null!;

    public string Title
    {
        get => Application.productName;
        set => Application.productName = value;
    }

    public void Init(AbstMainWindow instance)
    {
        _window = instance;
        _window.SetTheSizeFromFW(GetTheSize());
    }

    public APoint GetTheSize() => new(Screen.width, Screen.height);

    public void SetTheSize(APoint size)
    {
        Screen.SetResolution((int)size.X, (int)size.Y, Screen.fullScreenMode);
    }
}

