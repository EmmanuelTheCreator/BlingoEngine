using AbstUI.FrameworkCommunication;
using AbstUI.Styles;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AbstUI.Components;

/// <summary>
/// Base class for framework specific component factories.
/// </summary>
public interface IAbstComponentFactoryBase
{
    IAbstComponentFactoryBase Register<TComponent, TFramework>() where TFramework : IFrameworkFor<TComponent>;
    IAbstComponentFactoryBase DiscoverInAssembly(Assembly assembly);
    TReturnType CreateElement<TReturnType>()
        where TReturnType : notnull;
    TReturnType CreateElement<TReturnType, TForType>()
        where TReturnType : TForType
        where TForType : notnull;

    T GetRequiredService<T>() where T : notnull;
}
public abstract class AbstComponentFactoryBase : IAbstComponentFactoryBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _frameworkMap = new();

    public IAbstStyleManager StyleManager { get; }
    public IAbstFontManager FontManager { get; }

    protected AbstComponentFactoryBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    
        StyleManager = serviceProvider.GetRequiredService<IAbstStyleManager>();
        FontManager = serviceProvider.GetRequiredService<IAbstFontManager>();
    }
    public T GetRequiredService<T>() where T: notnull
        => _serviceProvider.GetRequiredService<T>();


    /// <summary>
    /// Applies a registered style to the component if available.
    /// </summary>
    protected void ApplyStyle<T>(T component) where T : IAbstNode
        => StyleManager.ApplyStyle(component);

    /// <summary>
    /// Performs common initialization for newly created components.
    /// </summary>
    protected void InitComponent<T>(T component) where T : IAbstNode
        => ApplyStyle(component);



    /// <summary>
    /// Scans the assembly of the derived factory for types implementing
    /// <see cref="IFrameworkFor{T}"/> and caches their mapping.
    /// </summary>
    public void DiscoverInAssembly()
        => DiscoverInAssembly(GetType().Assembly);

    public IAbstComponentFactoryBase DiscoverInAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IFrameworkFor<>))
                {
                    var blingoType = iface.GenericTypeArguments[0];
                    _frameworkMap[blingoType] = type;
                }
            }
        }
        return this;
    }

    public IAbstComponentFactoryBase Register<TComponent, TFamework>()
        where TFamework : IFrameworkFor<TComponent>
    {
        var componentType = typeof(TComponent);
        if (_frameworkMap.ContainsKey(componentType))
            _frameworkMap.Remove(componentType);
        _frameworkMap[componentType] = typeof(TFamework);
        return this;
    }

    public TReturnType CreateElement<TReturnType>()
        where TReturnType : notnull
    {
        var type = typeof(TReturnType);
        var component = _serviceProvider.GetRequiredService<TReturnType>();
        var fw = GetFrameworkFor<IFrameworkFor<TReturnType>>(type);
        if (component is IAbstNode abstNode1 && fw is IAbstFrameworkNode fwNode)
            abstNode1.FrameworkObj = fwNode;
        if (fw is IFrameworkForInitializable<TReturnType> initable)
            initable.Init(component);
        if (component is IAbstNode abstNode)
            InitComponent(abstNode);
        return component;
    }
    public TReturnType CreateElement<TReturnType, TForType>()
        where TForType : notnull
        where TReturnType : TForType
    {
        var component = _serviceProvider.GetRequiredService<TReturnType>();
        var fw = GetFrameworkFor<IFrameworkFor<TReturnType>>(component);
        if (fw is IFrameworkForInitializable<TForType> initable)
            initable.Init(component);
        if (component is IAbstNode abstNode)
            InitComponent(abstNode);
        return component;
    }
    /// <summary>
    /// Resolves the framework counterpart for the specified Lingo dialog.
    /// </summary>
    public TReturnType GetFrameworkFor<TReturnType>(object component) => (TReturnType)GetFrameworkFor(component.GetType());
    public TReturnType GetFrameworkFor<TReturnType>(Type type) => (TReturnType)GetFrameworkFor(type);
    public object GetFrameworkFor(Type type)
    {
        if (!_frameworkMap.TryGetValue(type, out var frameworkType))
            throw new InvalidOperationException($"No framework registered for {type.Name}");

        var instance = _serviceProvider.GetRequiredService(frameworkType);
        return instance;
    }


    //public virtual IAbstWindow CreateWindow<T>()
    //{
    //    var window = GetFrameworkFor<T>();
    //    InitComponent(window);
    //    window.Name = name;
    //    window.Title = title;
    //    return window;
    //}
}

