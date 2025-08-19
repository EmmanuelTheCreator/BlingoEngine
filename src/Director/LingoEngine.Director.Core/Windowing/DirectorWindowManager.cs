using AbstUI.Components;
using AbstUI.Commands;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Tools.Commands;
using LingoEngine.Director.Core.Windowing.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.Core.Windowing
{
    public interface IDirFrameworkWindowManager
    {
        void SetActiveWindow(IDirectorWindowRegistration window);
        IDirectorWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult);
        IDirectorWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel);
        IDirectorWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? lingoDialog = null)
            where TDialog : class, ILingoDialog;
        IDirectorWindowDialogReference? ShowNotification(string message, DirUINotificationType type);
    }
    public interface IDirectorWindowRegistration
    {
        string WindowCode { get; }
    }
    public interface IDirectorWindowManager
    {
        IDirectorWindowManager Register<TWindow>(string windowCode, Func<IServiceProvider, IDirectorWindow> constructor, DirectorShortCutMap? shortCutMap = null)
             where TWindow : IDirectorWindow;
        IDirectorWindowManager Register<TWindow>(string windowCode, DirectorShortCutMap? shortCutMap = null)
             where TWindow : IDirectorWindow;
        bool OpenWindow(string windowCode);
        bool SwapWindowOpenState(string windowCode);
        bool CloseWindow(string windowCode);
        void Init(IDirFrameworkWindowManager frameworkWindowManager);
        void SetActiveWindow(string windowCode);
        IDirectorWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult);
        IDirectorWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel);
        IDirectorWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? lingoDialog = null)
            where TDialog : class, ILingoDialog;
        IDirectorWindowDialogReference? ShowNotification(string message, DirUINotificationType type);
        void SetWindowSize(string windowCode, int stageWidth, int stageHeight);
    }
    public class DirectorWindowManager : IDirectorWindowManager,
        IAbstCommandHandler<OpenWindowCommand>,
        IAbstCommandHandler<CloseWindowCommand>,
        IAbstCommandHandler<ExecuteShortCutCommand>
    {
        private readonly Dictionary<string, WindowRegistration> _windowRegistrations = new();
        private readonly IServiceProvider _serviceProvider;
        private IDirFrameworkWindowManager _frameworkWindowManager;
        private IDirectorWindowRegistration _ActiveWindow;

#pragma warning disable CS8618
        public DirectorWindowManager(IServiceProvider serviceProvider)
#pragma warning restore CS8618 
        {
            _serviceProvider = serviceProvider;
        }

        public void Init(IDirFrameworkWindowManager frameworkWindowManager)
        {
            _frameworkWindowManager = frameworkWindowManager;
        }

        public IDirectorWindowManager Register<TWindow>(string windowCode, Func<IServiceProvider, IDirectorWindow> constructor, DirectorShortCutMap? shortCutMap = null)
            where TWindow : IDirectorWindow
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



        public IDirectorWindowManager Register<TWindow>(string windowCode, DirectorShortCutMap? shortCutMap = null)
            where TWindow : IDirectorWindow
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

        public void SetActiveWindow(string windowCode)
            => SetActiveWindow(_windowRegistrations[windowCode]);
        public void SetActiveWindow(IDirectorWindowRegistration window)
        {
            if (window is not WindowRegistration registration)
                throw new ArgumentException("Invalid window registration type.", nameof(window));
            _ActiveWindow = window;
            _frameworkWindowManager.SetActiveWindow(registration);
        }

        public IDirectorWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult)
            => _frameworkWindowManager.ShowConfirmDialog(title, message, onResult);

        public IDirectorWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel)
            => _frameworkWindowManager.ShowCustomDialog(title, panel);

        public IDirectorWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? lingoDialog = null)
            where TDialog : class, ILingoDialog
            => _frameworkWindowManager.ShowCustomDialog<TDialog>(title, panel, lingoDialog);

        public IDirectorWindowDialogReference? ShowNotification(string message, DirUINotificationType type)
            => _frameworkWindowManager.ShowNotification(message, type);


        public bool SwapWindowOpenState(string windowCode)
        {
            if (!_windowRegistrations.TryGetValue(windowCode, out var registration)) return false;
            if (registration.Instance.IsOpen)
            {
                registration.Instance.CloseWindow();
                return true;
            }
            else
            {
                registration.Instance.OpenWindow();
                SetActiveWindow(registration);
                return true;
            }
        }
        public bool OpenWindow(string windowCode)
        {
            if (!_windowRegistrations.TryGetValue(windowCode, out var registration)) return false;
            registration.Instance.OpenWindow();
            SetActiveWindow(registration);
            return true;
        }

        public IEnumerable<(string Code, IDirectorWindow Instance)> EnumerateRegistrations()
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
        public bool WindowExists(DirectorShortCutMap shortCutMap) => _windowRegistrations.Values.Any(X => X.ShortCutMap == shortCutMap);

        public bool CanExecute(OpenWindowCommand command) => WindowExists(command.WindowCode);
        public bool CanExecute(CloseWindowCommand command) => WindowExists(command.WindowCode);
        public bool CanExecute(ExecuteShortCutCommand command) => WindowExists(command.ShortCut);

        public bool Handle(OpenWindowCommand command) => OpenWindow(command.WindowCode);
        public bool Handle(CloseWindowCommand command) => CloseWindow(command.WindowCode);
        public bool Handle(ExecuteShortCutCommand command)
        {
            var registration = _windowRegistrations.Values.First(x => x.ShortCutMap == command.ShortCut);
            registration.Instance.CloseWindow();
            return true;
        }

        public void SetWindowSize(string windowCode, int stageWidth, int stageHeight)
        {
            if (!_windowRegistrations.TryGetValue(windowCode, out var registration)) return;
            registration.Instance.SetSize(stageWidth, stageHeight);
        }

        private class WindowRegistration : IDirectorWindowRegistration
        {
            private Func<IDirectorWindow> _constructor;
            private IDirectorWindow? _instance;
            public string WindowCode { get; }
            public DirectorShortCutMap? ShortCutMap { get; }
            public WindowRegistration(string windowCode, Func<IDirectorWindow> constructor, DirectorShortCutMap? shortCutMap)
            {
                WindowCode = windowCode;
                _constructor = constructor;
                ShortCutMap = shortCutMap;
            }
            public IDirectorWindow Instance
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
}
