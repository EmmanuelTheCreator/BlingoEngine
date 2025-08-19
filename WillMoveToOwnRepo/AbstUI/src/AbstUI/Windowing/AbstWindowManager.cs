using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AbstUI.Components;
using AbstUI.Commands;
using AbstUI.Windowing.Commands;

namespace AbstUI.Windowing;

public interface IAbstFrameworkWindowManager
{
    void SetActiveWindow(IAbstWindowRegistration window);
    IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult);
    IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel);
    IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? dialog = null)
        where TDialog : class, IAbstDialog;
    IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type);
}

public interface IAbstWindowRegistration
{
    string WindowCode { get; }
}

public interface IAbstWindowManager
{
    IAbstWindowManager Register<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor)
         where TWindow : IAbstWindow;
    IAbstWindowManager Register<TWindow>(string windowCode)
         where TWindow : IAbstWindow;
    bool OpenWindow(string windowCode);
    bool SwapWindowOpenState(string windowCode);
    bool CloseWindow(string windowCode);
    void Init(IAbstFrameworkWindowManager frameworkWindowManager);
    void SetActiveWindow(string windowCode);
    IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult);
    IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel);
    IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? dialog = null)
        where TDialog : class, IAbstDialog;
    IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type);
    void SetWindowSize(string windowCode, int width, int height);
}

public interface IAbstDialog { }

public enum AbstUINotificationType
{
    Info,
    Warning,
    Error
}

public class AbstWindowManager : IAbstWindowManager,
    IAbstCommandHandler<OpenWindowCommand>,
    IAbstCommandHandler<CloseWindowCommand>
{
    private readonly Dictionary<string, WindowRegistration> _windowRegistrations = new();
    private readonly IServiceProvider _serviceProvider;
    private IAbstFrameworkWindowManager _frameworkWindowManager = null!;
    private IAbstWindowRegistration? _activeWindow;

    public AbstWindowManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Init(IAbstFrameworkWindowManager frameworkWindowManager)
        => _frameworkWindowManager = frameworkWindowManager;

    public IAbstWindowManager Register<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor)
        where TWindow : IAbstWindow
    {
        if (_windowRegistrations.ContainsKey(windowCode))
            throw new InvalidOperationException($"Window with code '{windowCode}' is already registered.");

        _windowRegistrations[windowCode] = new WindowRegistration(windowCode, () => constructor(_serviceProvider));
        return this;
    }

    public IAbstWindowManager Register<TWindow>(string windowCode)
        where TWindow : IAbstWindow
    {
        if (_windowRegistrations.ContainsKey(windowCode))
            throw new InvalidOperationException($"Window with code '{windowCode}' is already registered.");

        _windowRegistrations[windowCode] = new WindowRegistration(windowCode, () => _serviceProvider.GetRequiredService<TWindow>());
        return this;
    }

    public void SetActiveWindow(string windowCode) => SetActiveWindow(_windowRegistrations[windowCode]);

    public void SetActiveWindow(IAbstWindowRegistration window)
    {
        if (window is not WindowRegistration registration)
            throw new ArgumentException("Invalid window registration type.", nameof(window));
        _activeWindow = window;
        _frameworkWindowManager.SetActiveWindow(registration);
    }

    public IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult)
        => _frameworkWindowManager.ShowConfirmDialog(title, message, onResult);

    public IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel)
        => _frameworkWindowManager.ShowCustomDialog(title, panel);

    public IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? dialog = null)
        where TDialog : class, IAbstDialog
        => _frameworkWindowManager.ShowCustomDialog<TDialog>(title, panel, dialog);

    public IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type)
        => _frameworkWindowManager.ShowNotification(message, type);

    public bool SwapWindowOpenState(string windowCode)
    {
        if (!_windowRegistrations.TryGetValue(windowCode, out var registration)) return false;
        if (registration.Instance.IsOpen)
        {
            registration.Instance.CloseWindow();
            return true;
        }
        registration.Instance.OpenWindow();
        SetActiveWindow(registration);
        return true;
    }

    public bool OpenWindow(string windowCode)
    {
        if (!_windowRegistrations.TryGetValue(windowCode, out var registration)) return false;
        registration.Instance.OpenWindow();
        SetActiveWindow(registration);
        return true;
    }

    public IEnumerable<(string Code, IAbstWindow Instance)> EnumerateRegistrations()
    {
        foreach (var kv in _windowRegistrations)
            yield return (kv.Key, kv.Value.Instance);
    }

    public bool CloseWindow(string windowCode)
    {
        if (!_windowRegistrations.TryGetValue(windowCode, out var registration)) return false;
        registration.Instance.CloseWindow();
        return true;
    }

    public bool WindowExists(string windowCode) => _windowRegistrations.ContainsKey(windowCode);

    public bool CanExecute(OpenWindowCommand command) => WindowExists(command.WindowCode);
    public bool CanExecute(CloseWindowCommand command) => WindowExists(command.WindowCode);

    public bool Handle(OpenWindowCommand command) => OpenWindow(command.WindowCode);
    public bool Handle(CloseWindowCommand command) => CloseWindow(command.WindowCode);

    public void SetWindowSize(string windowCode, int width, int height)
    {
        if (!_windowRegistrations.TryGetValue(windowCode, out var registration)) return;
        registration.Instance.SetSize(width, height);
    }

    private class WindowRegistration : IAbstWindowRegistration
    {
        private readonly Func<IAbstWindow> _constructor;
        private IAbstWindow? _instance;
        public string WindowCode { get; }
        public WindowRegistration(string windowCode, Func<IAbstWindow> constructor)
        {
            WindowCode = windowCode;
            _constructor = constructor;
        }
        public IAbstWindow Instance
        {
            get
            {
                _instance ??= _constructor();
                return _instance;
            }
        }
    }
}
