using AbstUI.Windowing;
using AbstUI.Components.Containers;
using AbstUI.Components;
using AbstUI.FrameworkCommunication;
using AbstUI.Blazor.Components.Containers;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Windowing;

internal class AbstBlazorWindowManager : IAbstFrameworkWindowManager, IFrameworkFor<AbstWindowManager>
{
    private readonly IAbstWindowManager _windowManager;
    private readonly IJSRuntime _js;
    private readonly IAbstComponentFactory _factory;
    private readonly Dictionary<string, IAbstWindow> _windows = new();
    private IJSObjectReference? _module;

    public AbstBlazorWindowManager(IAbstWindowManager windowManager, IJSRuntime js, IAbstComponentFactory factory)
    {
        _windowManager = windowManager;
        _js = js;
        _factory = factory;
        windowManager.NewWindowCreated += NewWindowCreated;
        windowManager.Init(this);
    }

    private void NewWindowCreated(IAbstWindow window)
        => _windows[window.WindowCode] = window;

    public void SetActiveWindow(IAbstWindow window)
    {
        if (window.FrameworkObj is AbstBlazorWindow blazorWindow)
            blazorWindow.Popup();
    }

    public IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult)
    {
        EnsureModule();
        var result = _module!.InvokeAsync<bool>("AbstUIWindow.showBootstrapConfirm", title, message)
            .AsTask().GetAwaiter().GetResult();
        onResult(result);
        return new AbstWindowDialogReference(() => { });
    }

    public IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel, APoint? position = null)
    {
        var dialogAbst = _factory.CreateElement<IAbstDialog>();
        var dialog = dialogAbst.FrameworkObj<AbstBlazorDialog>();
        dialog.Title = title;
        dialog.SetSize((int)panel.Width, (int)panel.Height);
        dialog.AddItem(panel);
        if (position == null)
            dialog.PopupCentered();
        else
        {
            dialog.SetPositionAndSize((int)position.Value.X, (int)position.Value.Y, (int)panel.Width, (int)panel.Height);
            dialog.Popup();
        }
        return new AbstWindowDialogReference(() => { dialog.Hide(); dialog.Dispose(); }, dialog);
    }

    public IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? dialog = null, APoint? position = null)
        where TDialog : class, IAbstDialog
    {
        AbstBlazorDialog blazorDialog;
        if (dialog != null)
            blazorDialog = dialog.FrameworkObj<AbstBlazorDialog>();
        else
        {
            dialog = _factory.CreateElement<TDialog>();
            blazorDialog = dialog.FrameworkObj<AbstBlazorDialog>();
        }

        blazorDialog.Title = title;
        blazorDialog.SetSize((int)panel.Width, (int)panel.Height);
        blazorDialog.AddItem(panel);
        if (position == null)
            blazorDialog.PopupCentered();
        else
        {
            blazorDialog.SetPositionAndSize((int)position.Value.X, (int)position.Value.Y, (int)panel.Width, (int)panel.Height);
            blazorDialog.Popup();
        }
        return new AbstWindowDialogReference(() => { blazorDialog.Hide(); blazorDialog.Dispose(); }, blazorDialog);
    }

    public IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type)
    {
        EnsureModule();
        var typeStr = type switch
        {
            AbstUINotificationType.Error => "error",
            AbstUINotificationType.Info => "info",
            _ => "warning"
        };
        _module!.InvokeVoidAsync("AbstUIWindow.showBootstrapToast", message, typeStr);
        return new AbstWindowDialogReference(() => { });
    }

    private void EnsureModule()
    {
        _module ??= _js.InvokeAsync<IJSObjectReference>(
                "import", "./_content/AbstUI.Blazor/scripts/abstUIScripts.js")
            .AsTask().GetAwaiter().GetResult();
    }
}
