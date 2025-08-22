using AbstUI.Windowing;
using AbstUI.Components.Containers;
using AbstUI.Components;
using AbstUI.Components.Buttons;
using AbstUI.Components.Texts;
using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstUI.LUnity.Windowing;

/// <summary>
/// Minimal Unity implementation of <see cref="IAbstFrameworkWindowManager"/>.
/// Currently provides basic integration with the core window manager but does not
/// show platform specific dialogs.
/// </summary>
internal class AbstUnityWindowManager : IAbstFrameworkWindowManager, IFrameworkFor<AbstWindowManager>
{
    private readonly IAbstWindowManager _windowManager;
    private readonly IAbstComponentFactory _componentFactory;

    public AbstUnityWindowManager(IAbstWindowManager windowManager, IAbstComponentFactory componentFactory)
    {
        _windowManager = windowManager;
        _componentFactory = componentFactory;
        windowManager.Init(this);
    }

    public void SetActiveWindow(IAbstWindow window)
    {
        if (window.FrameworkObj is AbstUnityWindow unityWindow &&
            unityWindow.FrameworkNode is GameObject go)
        {
            go.transform.SetAsLastSibling();
        }
    }

    public IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult)
    {
        Debug.Log($"Confirm: {title} - {message}");
        onResult(true);
        return new AbstWindowDialogReference(() => { });
    }

    public IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel)
    {
        var dialogAbst = _componentFactory.CreateElement<IAbstDialog>();
        var dialog = dialogAbst.FrameworkObj<AbstUnityDialog>();
        dialog.Title = title;
        dialog.SetSize((int)panel.Width, (int)panel.Height);
        dialog.AddItem(panel);
        dialog.PopupCentered();
        return new AbstWindowDialogReference(dialog.Hide, dialog);
    }

    public IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? dialog = null)
        where TDialog : class, IAbstDialog
    {
        AbstUnityDialog unityDialog;
        if (dialog != null)
        {
            unityDialog = dialog.FrameworkObj<AbstUnityDialog>();
        }
        else
        {
            dialog = _componentFactory.CreateElement<TDialog>();
            unityDialog = dialog.FrameworkObj<AbstUnityDialog>();
        }

        unityDialog.Title = title;
        unityDialog.SetSize((int)panel.Width, (int)panel.Height);
        unityDialog.AddItem(panel);
        unityDialog.PopupCentered();
        return new AbstWindowDialogReference(unityDialog.Hide, unityDialog);
    }

    public IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type)
    {
        Debug.Log($"Notification {type}: {message}");
        return new AbstWindowDialogReference(() => { });
    }
}
