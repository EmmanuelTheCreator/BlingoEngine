using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.Windowing;
public interface IAbstWindowRegistration
{
    bool HasBeenCreated { get; }
    string WindowCode { get; }
    IAbstWindow Instance { get; }
}
public interface IAbstWindowFactory
{
    event Action<IAbstWindowRegistration>? NewWindowRegistred;
    event Action<IAbstWindow>? NewWindowCreated;

    IAbstWindow? GetWindow(string windowCode);
    IAbstWindowRegistration? GetWindowRegistration(string windowCode);
    bool TryGetRegistration(string windowCode, out IAbstWindowRegistration registration);
    IAbstWindowFactory Register<TWindow>(string windowCode) where TWindow : IAbstWindow;
    IAbstWindowFactory Register<TWindow>(string windowCode, AbstShortCutMap? shortCutMap = null) where TWindow : IAbstWindow;
    IAbstWindowFactory Register<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor) where TWindow : IAbstWindow;
    IAbstWindowFactory Register<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor, AbstShortCutMap? shortCutMap = null) where TWindow : IAbstWindow;
    bool WindowExists(string windowCode);
    IEnumerable<IAbstWindow> GetWindows();
    IEnumerable<(string Code, ARect Rect)> GetRects();
}

public class AbstWindowFactory : IAbstWindowFactory
{
    private readonly Dictionary<string, WindowRegistration> _windowRegistrations = new();
    private readonly Lazy<IServiceProvider> _serviceProvider;
    private readonly Lazy<IAbstComponentFactory> _componentFactory;

    public event Action<IAbstWindowRegistration>? NewWindowRegistred;
    public event Action<IAbstWindow>? NewWindowCreated;

    public AbstWindowFactory(Lazy<IServiceProvider> serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _componentFactory = new Lazy<IAbstComponentFactory>(serviceProvider.Value.GetRequiredService<IAbstComponentFactory>);
    }


    public IAbstWindowFactory Register<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor, AbstShortCutMap? shortCutMap = null)
           where TWindow : IAbstWindow 
        => RegisterNew(new WindowRegistration(OnNewWindowCreated, windowCode, () =>
                                               {
                                                   var instance = constructor(_serviceProvider.Value);
                                                   return instance;
                                               }, shortCutMap));



    public IAbstWindowFactory Register<TWindow>(string windowCode, AbstShortCutMap? shortCutMap = null)
            where TWindow : IAbstWindow 
        => RegisterNew(new WindowRegistration(OnNewWindowCreated, windowCode, () =>
                                                {
                                                    //var instance = _serviceProvider.GetRequiredService<TWindow>();
                                                    var instance = _componentFactory.Value.CreateElement<TWindow>();
                                                    return instance;
                                                }, shortCutMap));

    public IAbstWindowFactory Register<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor)
        where TWindow : IAbstWindow 
        => RegisterNew(new WindowRegistration(OnNewWindowCreated, windowCode, () => constructor(_serviceProvider.Value)));

    public IAbstWindowFactory Register<TWindow>(string windowCode)
        where TWindow : IAbstWindow 
        => RegisterNew(new WindowRegistration(OnNewWindowCreated, windowCode, () => _componentFactory.Value.CreateElement<TWindow>()));


    private IAbstWindowFactory RegisterNew(WindowRegistration registration)
    {
        if (_windowRegistrations.ContainsKey(registration.WindowCode))
            throw new InvalidOperationException($"Window with code '{registration.WindowCode}' is already registered.");
        _windowRegistrations[registration.WindowCode] = registration;
        NewWindowRegistred?.Invoke(registration);
        return this;
    }

    public IAbstWindow? GetWindow(string windowCode)
    {
        if (_windowRegistrations.TryGetValue(windowCode, out var registration))
            return registration.Instance;
        return null;
    }
    public IAbstWindowRegistration? GetWindowRegistration(string windowCode)
    {
        if (_windowRegistrations.TryGetValue(windowCode, out var registration))
            return registration;
        return null;
    }

    public bool TryGetRegistration(string windowCode, out IAbstWindowRegistration registration)
    {
        if (_windowRegistrations.TryGetValue(windowCode, out var reg))
        {
            registration = reg;
            return true;
        }
        registration = null!;
        return false;
    }

    public bool WindowExists(string windowCode) => _windowRegistrations.ContainsKey(windowCode);

    public IEnumerable<(string Code, ARect Rect)> GetRects()
    {
        foreach (var kv in _windowRegistrations) {
            var window = kv.Value.Instance;
            yield return (kv.Key, ARect.New(window.X, window.Y, window.Width, window.Height));
        }

    }

    public IEnumerable<IAbstWindow> GetWindows() => _windowRegistrations.Values.Select(x => x.Instance);
    
    private void OnNewWindowCreated(IAbstWindow window)
    {
        NewWindowCreated?.Invoke(window);
    }

    private class WindowRegistration : IAbstWindowRegistration
    {
        private readonly Action<IAbstWindow> _createdInstance;
        private readonly Func<IAbstWindow> _constructor;
        private IAbstWindow? _instance;
        public string WindowCode { get; }
        public AbstShortCutMap? ShortCutMap { get; }
        public WindowRegistration(Action<IAbstWindow> createdInstance, string windowCode, Func<IAbstWindow> constructor, AbstShortCutMap? shortCutMap = null)
        {
            _createdInstance = createdInstance;
            WindowCode = windowCode;
            _constructor = constructor;
            ShortCutMap = shortCutMap;
        }
        public IAbstWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = _constructor();
                    _createdInstance.Invoke(_instance);
                }

                return _instance;
            }
        }

        public bool HasBeenCreated => _instance != null;
    }
}

