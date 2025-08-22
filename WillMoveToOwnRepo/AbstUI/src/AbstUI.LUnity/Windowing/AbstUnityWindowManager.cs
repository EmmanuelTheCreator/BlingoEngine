using AbstUI.Windowing;
using AbstUI.Components.Containers;
using AbstUI.Components;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LUnity.Windowing;

/// <summary>
/// Minimal Unity implementation of <see cref="IAbstFrameworkWindowManager"/>.
/// Currently provides basic integration with the core window manager but does not
/// show platform specific dialogs.
/// </summary>
internal class AbstUnityWindowManager : IAbstFrameworkWindowManager, IFrameworkFor<AbstWindowManager>
{
    private readonly IAbstWindowManager _windowManager;

    public AbstUnityWindowManager(IAbstWindowManager windowManager)
    {
        _windowManager = windowManager;
        windowManager.Init(this);
    }

    public void SetActiveWindow(IAbstWindow window)
    {
        // Unity has no native window ordering; nothing to do here for now.
    }

    public IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult)
        => null;

    public IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel)
        => null;

    public IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? dialog = null)
        where TDialog : class, IAbstDialog
        => null;

    public IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type)
        => null;
}
