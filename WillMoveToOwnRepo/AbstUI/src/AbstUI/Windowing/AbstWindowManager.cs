using Microsoft.Extensions.DependencyInjection;
using AbstUI.Commands;
using AbstUI.Windowing.Commands;
using AbstUI.Tools;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using System;

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
    IAbstWindowManager Register<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor, AbstShortCutMap? shortCutMap = null)
          where TWindow : IAbstWindow;
    IAbstWindowManager Register<TWindow>(string windowCode, AbstShortCutMap? shortCutMap = null)
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

public class AbstWindowManager : IAbstWindowManager,
    IAbstCommandHandler<OpenWindowCommand>,
    IAbstCommandHandler<CloseWindowCommand>
{
    private readonly Dictionary<string, WindowRegistration> _windowRegistrations = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly AbstMainWindow _mainWindow;
    private IAbstFrameworkWindowManager _frameworkWindowManager = null!;
    private WindowRegistration? _activeWindow;

    public AbstWindowManager(IServiceProvider serviceProvider, AbstMainWindow mainWindow)
    {
        _serviceProvider = serviceProvider;
        _mainWindow = mainWindow;
    }

    public void Init(IAbstFrameworkWindowManager frameworkWindowManager)
        => _frameworkWindowManager = frameworkWindowManager;
    public IAbstWindowManager Register<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor, AbstShortCutMap? shortCutMap = null)
            where TWindow : IAbstWindow
    {
        if (_windowRegistrations.ContainsKey(windowCode))
            throw new InvalidOperationException($"Window with code '{windowCode}' is already registered.");

        _windowRegistrations[windowCode] = new WindowRegistration(windowCode, () =>
        {
            var instance = constructor(_serviceProvider);
            return instance;
        }, shortCutMap);

        return this;
    }
    public IAbstWindowManager Register<TWindow>(string windowCode, AbstShortCutMap? shortCutMap = null)
            where TWindow : IAbstWindow
    {
        if (_windowRegistrations.ContainsKey(windowCode))
            throw new InvalidOperationException($"Window with code '{windowCode}' is already registered.");

        _windowRegistrations[windowCode] = new WindowRegistration(windowCode, () =>
        {
            var instance = _serviceProvider.GetRequiredService<TWindow>();
            return instance;
        }, shortCutMap);

        return this;
    }

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
        if (_activeWindow != null && _activeWindow.Instance != null)
            _activeWindow.Instance.SetActivated(false);

        registration.Instance.SetActivated(true);
        _activeWindow = registration;
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
        var instance = registration.Instance;
        if (instance.IsOpen)
        {
            instance.CloseWindow();
            return true;
        }
        instance.OpenWindow();
        EnsureWindowInBounds(instance);
        SetActiveWindow(registration);
        return true;
    }

    public bool OpenWindow(string windowCode)
    {
        if (!_windowRegistrations.TryGetValue(windowCode, out var registration)) return false;
        var instance = registration.Instance;
        bool wasOpen = instance.IsOpen;
        instance.OpenWindow();
        if (!wasOpen)
            EnsureWindowInBounds(instance);
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

    private void EnsureWindowInBounds(IAbstWindow window)
    {
        var size = _mainWindow.GetSize();
        int maxX = (int)(size.X - window.Width);
        int maxY = (int)(size.Y - window.Height);
        maxX = Math.Max(0, maxX);
        maxY = Math.Max(0, maxY);
        int x = Math.Min(Math.Max(window.X, 0), maxX);
        int y = Math.Min(Math.Max(window.Y, 0), maxY);
        if (x != window.X || y != window.Y)
            window.MoveWindow(x, y);
    }

    private class WindowRegistration : IAbstWindowRegistration
    {
        private readonly Func<IAbstWindow> _constructor;
        private IAbstWindow? _instance;
        public string WindowCode { get; }
        public AbstShortCutMap? ShortCutMap { get; }
        public WindowRegistration(string windowCode, Func<IAbstWindow> constructor, AbstShortCutMap? shortCutMap = null)
        {
            WindowCode = windowCode;
            _constructor = constructor;
            ShortCutMap = shortCutMap;
        }
        public IAbstWindow Instance
        {
            get
            {
                if (_instance == null)
                    _instance = _constructor();

                return _instance;
            }
        }
    }
}

