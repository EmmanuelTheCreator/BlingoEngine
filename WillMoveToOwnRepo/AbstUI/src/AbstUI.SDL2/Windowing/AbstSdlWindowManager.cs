using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Windowing;
using AbstUI.SDL2.Components;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.SDL2.Windowing
{
    internal interface IAbstSdlWindowManager : IAbstFrameworkWindowManager
    {
        void Register(AbstSdlWindow window);
        void Unregister(AbstSdlWindow window);
        void SetActiveWindow(AbstSdlWindow window);
        AbstSdlWindow? ActiveWindow { get; }
    }

    /// <summary>
    /// SDL2 implementation of window manager. Currently a minimal
    /// placeholder that tracks active windows but does not provide
    /// OS level dialogs or notifications.
    /// </summary>
    internal class AbstSdlWindowManager : IAbstSdlWindowManager
    {
        private readonly IAbstWindowManager _windowManager;
        private readonly IServiceProvider _services;
        private readonly Dictionary<string, AbstSdlWindow> _windows = new();
        private AbstSdlComponentFactory? _factory;
        private bool _syncing;

        public AbstSdlWindow? ActiveWindow { get; private set; }

        public AbstSdlWindowManager(IAbstWindowManager windowManager, IServiceProvider services)
        {
            _windowManager = windowManager;
            _services = services;
            windowManager.Init(this);
        }

        public void Register(AbstSdlWindow window)
            => _windows[window.WindowCode] = window;

        public void Unregister(AbstSdlWindow window)
        {
            _windows.Remove(window.WindowCode);
            if (ActiveWindow == window)
                ActiveWindow = null;
        }

        public void SetActiveWindow(AbstSdlWindow window)
        {
            if (ActiveWindow == window)
                return;

            ActiveWindow = window;
            window.BringToFront();

            if (_syncing)
                return;

            _syncing = true;
            _windowManager.SetActiveWindow(window.WindowCode);
            _syncing = false;
        }

        public void SetActiveWindow(IAbstWindowRegistration window)
        {
            if (_windows.TryGetValue(window.WindowCode, out var w))
            {
                ActiveWindow = w;
                w.BringToFront();
            }
        }

        public IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult)
            => null;

        public IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel)
        {
            var factory = _factory ??= _services.GetRequiredService<AbstSdlComponentFactory>();
            var dialogAbst = factory.CreateElement<IAbstDialog>();
            var dialog = dialogAbst.FrameworkObj<AbstSdlDialog>();
            dialog.Title = title;
            dialog.SetSize((int)panel.Width, (int)panel.Height);
            dialog.AddItem(panel);
            dialog.PopupCentered();
            return new AbstWindowDialogReference(() => { dialog.Hide(); dialog.Dispose(); }, dialog);
        }

        public IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? dialog = null)
            where TDialog : class, IAbstDialog
        {
            var factory = _factory ??= _services.GetRequiredService<AbstSdlComponentFactory>();
            AbstSdlDialog sdlDialog;
            if (dialog != null)
            {
                sdlDialog = dialog.FrameworkObj<AbstSdlDialog>();
            }
            else
            {
                dialog = factory.CreateElement<TDialog>();
                sdlDialog = dialog.FrameworkObj<AbstSdlDialog>();
            }

            sdlDialog.Title = title;
            sdlDialog.SetSize((int)panel.Width, (int)panel.Height);
            sdlDialog.AddItem(panel);
            sdlDialog.PopupCentered();
            return new AbstWindowDialogReference(() => { sdlDialog.Hide(); sdlDialog.Dispose(); }, sdlDialog);
        }

        public IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type)
            => null;
    }
}
