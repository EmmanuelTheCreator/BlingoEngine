using AbstUI.Windowing;
using AbstUI.Components.Containers;
using AbstUI.Components;
using AbstUI.SDL2.SDLL;
using System;
using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Windowing
{
    internal interface IAbstSdlWindowManager : IAbstFrameworkWindowManager
    {
        AbstSdlWindow? ActiveWindow { get; }
    }

    /// <summary>
    /// SDL2 implementation of window manager. Currently a minimal
    /// placeholder that tracks active windows but does not provide
    /// OS level dialogs or notifications.
    /// </summary>
    internal class AbstSdlWindowManager : IAbstSdlWindowManager, IFrameworkFor<AbstWindowManager>
    {
        private readonly IAbstWindowManager _windowManager;
        private readonly IAbstComponentFactory _componentFactory;

        public AbstSdlWindow? ActiveWindow => _windowManager.ActiveWindow?.FrameworkObj as AbstSdlWindow;

        public AbstSdlWindowManager(IAbstWindowManager windowManager, IAbstComponentFactory componentFactory)
        {
            _windowManager = windowManager;
            _componentFactory = componentFactory;
            windowManager.Init(this);
        }

       

        public void SetActiveWindow(IAbstWindow window)
        {
            ((AbstSdlWindow)window.FrameworkObj).BringToFront();
        }

        public IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult)
        {
            var buttons = new SDL.SDL_MessageBoxButtonData[]
            {
                new SDL.SDL_MessageBoxButtonData { buttonid = 1, text = "OK" },
                new SDL.SDL_MessageBoxButtonData { buttonid = 0, text = "Cancel" }
            };
            var data = new SDL.SDL_MessageBoxData
            {
                flags = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_WARNING,
                title = title,
                message = message,
                numbuttons = buttons.Length,
                buttons = buttons
            };
            SDL.SDL_ShowMessageBox(ref data, out int buttonId);
            onResult(buttonId == 1);
            return new AbstWindowDialogReference(() => { });
        }

        public IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel, APoint? position = null)
        {
            var dialogAbst = _componentFactory.CreateElement<AbstDialog>();
            var dialog = (AbstSdlDialog)dialogAbst.FrameworkObj; // dialogAbst.FrameworkObj<AbstSdlDialog>();
            dialog.Init(dialogAbst);
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
            AbstSdlDialog sdlDialog;
            if (dialog != null)
            {
                sdlDialog = dialog.FrameworkObj<AbstSdlDialog>();
            }
            else
            {
                dialog = _componentFactory.CreateElement<TDialog>();
                sdlDialog = dialog.FrameworkObj<AbstSdlDialog>();
            }
            sdlDialog.Init(dialog);

            sdlDialog.Title = title;
            sdlDialog.SetSize((int)panel.Width, (int)panel.Height);
            sdlDialog.AddItem(panel);
            if (position == null)
                dialog.PopupCentered();
            else
            {
                sdlDialog.SetPositionAndSize((int)position.Value.X, (int)position.Value.Y, (int)panel.Width, (int)panel.Height);
                dialog.Popup();
            }
            return new AbstWindowDialogReference(() => { sdlDialog.Hide(); sdlDialog.Dispose(); }, sdlDialog);
        }

        public IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type)
        {
            var flag = type switch
            {
                AbstUINotificationType.Error => SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
                AbstUINotificationType.Info => SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION,
                _ => SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_WARNING
            };
            SDL.SDL_ShowSimpleMessageBox(flag, "Notification", message, IntPtr.Zero);
            return new AbstWindowDialogReference(() => { });
        }
    }
}
