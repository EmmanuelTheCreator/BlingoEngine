using AbstUI.Components;
using AbstUI.Blazor.Components.Containers;
using AbstUI.FrameworkCommunication;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AbstUI.Blazor.Windowing;

public abstract class AbstBlazorDialog : AbstBlazorDialog<AbstDialog>
{
    protected AbstBlazorDialog(AbstBlazorComponentFactory factory) : base(factory)
    {
    }
}
/// <summary>
/// Blazor implementation of <see cref="IAbstFrameworkDialog"/> based on Bootstrap modals.
/// </summary>
public class AbstBlazorDialog<T> : AbstBlazorPanel, IAbstFrameworkDialog<T>, IFrameworkForInitializable<T>, IDisposable
    where T : IAbstDialog
{
    protected readonly AbstBlazorComponentFactory _factory;
    private IAbstDialog _dialog = null!;
    private string _title = string.Empty;
    private bool _isPopup;
    private bool _borderless;
    private IJSObjectReference? _module;

    [Inject] private IJSRuntime JS { get; set; } = default!;

    public string Title
    {
        get => _title;
        set => _title = value;
    }

    public AColor BackgroundColor { get; set; }

    public bool IsOpen => Visibility;

    public bool IsPopup
    {
        get => _isPopup;
        set => _isPopup = value;
    }

    public bool Borderless
    {
        get => _borderless;
        set => _borderless = value;
    }

    public bool IsActiveWindow => Visibility;

    public IAbstMouse Mouse => _dialog.Mouse;

    public IAbstKey Key => _dialog.Key;

    public event Action<bool>? OnWindowStateChanged;

    public AbstBlazorDialog(AbstBlazorComponentFactory factory)
    {
        _factory = factory;
        Visibility = false;
    }

    public void Init(T instance)
    {
        _dialog = instance;
        instance.Init(this);
        OnInit(instance);
    }
    protected virtual void OnInit(T instance) { }

    private void EnsureModule()
    {
        _module ??= JS.InvokeAsync<IJSObjectReference>(
                "import", "./_content/AbstUI.Blazor/scripts/abstUIScripts.js")
            .AsTask().GetAwaiter().GetResult();
    }

    public void Popup()
    {
        EnsureModule();
        _module!.InvokeVoidAsync("AbstUIWindow.showBootstrapModal", Name);
        Visibility = true;
        OnWindowStateChanged?.Invoke(true);
    }

    public void PopupCentered()
    {
        Popup();
    }

    public void Hide()
    {
        EnsureModule();
        _module!.InvokeVoidAsync("AbstUIWindow.hideBootstrapModal", Name);
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

    public void AddItem(IAbstFrameworkLayoutNode abstFrameworkLayoutNode)
        => Component.AddItem(abstFrameworkLayoutNode);

    public void RemoveItem(IAbstFrameworkLayoutNode abstFrameworkLayoutNode)
        => Component.RemoveItem(abstFrameworkLayoutNode);

    public IEnumerable<IAbstFrameworkLayoutNode> GetItems()
        => Component.GetItems();

    public override void Dispose()
    {
        _module?.DisposeAsync();
        base.Dispose();
    }
}

