using System;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.LUnity.Components.Containers;
using AbstUI.Inputs;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LUnity.Windowing;

public class AbstUnityDialog : AbstUnityDialog<AbstDialog>
{
   
}
/// <summary>
/// Simplistic Unity implementation of <see cref="IAbstFrameworkDialog"/>.
/// Provides basic popup behaviour for custom dialogs.
/// </summary>
public class AbstUnityDialog<TAbstDialog> : AbstUnityPanel, IAbstFrameworkDialog<TAbstDialog>, IFrameworkFor<TAbstDialog>
    where TAbstDialog : AbstDialog
{
    private IAbstDialog _dialog = null!;

    public string Title { get; set; } = string.Empty;
    public bool IsOpen => Visibility;
    public bool IsPopup { get; set; }
    public bool Borderless { get; set; }
    public bool IsActiveWindow => Visibility;
    public new AColor BackgroundColor
    {
        get => base.BackgroundColor ?? AColors.White;
        set => base.BackgroundColor = value;
    }
    public IAbstMouse Mouse => _dialog.Mouse;
    public IAbstKey Key => _dialog.Key;
    public event Action<bool>? OnWindowStateChanged;

    public void Init(IAbstDialog instance)
    {
        _dialog = instance;
        instance.Init(this);
    }

    public void Popup()
    {
        Visibility = true;
        OnWindowStateChanged?.Invoke(true);
    }

    public void PopupCentered() => Popup();

    public void Hide()
    {
        Visibility = false;
        OnWindowStateChanged?.Invoke(false);
    }

    public void SetPositionAndSize(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public APoint GetPosition() => new(X, Y);
    public APoint GetSize() => new(Width, Height);
    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
