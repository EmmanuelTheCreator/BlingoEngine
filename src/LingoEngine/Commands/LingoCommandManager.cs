using LingoEngine.Core;

namespace LingoEngine.Commands;

internal sealed class LingoCommandManager : ILingoCommandManager
{
    private readonly ILingoServiceProvider _provider;
    private readonly Dictionary<Type, List<Type>> _handlers = new();

    public LingoCommandManager(ILingoServiceProvider provider) => _provider = provider;

    public ILingoCommandManager Register<THandler, TCommand>()
        where THandler : ICommandHandler<TCommand>
        where TCommand : ILingoCommand
        => Register(typeof(THandler));

    public ILingoCommandManager Register(Type handlerType)
    {
        foreach (var iface in handlerType.GetInterfaces()
                     .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
        {
            var cmdType = iface.GetGenericArguments()[0];
            if (_handlers.TryGetValue(cmdType, out var handlers))
            {
                if (handlers.Contains(handlerType))
                    return this; // Already registered
                handlers.Add(handlerType);
            }
            else
                _handlers[cmdType] = new List<Type> { handlerType };
        }
        return this;
    }

    public bool Handle(ILingoCommand command)
    {
        var type = command.GetType();
        var ok = false;
        if (_handlers.TryGetValue(type, out var handlerTypes))
        {
            foreach (var handlerType in handlerTypes)
            {
                var handlerObj = _provider.GetService(handlerType);// ?? ActivatorUtilities.CreateInstance(_provider, handlerType);
                if (handlerObj == null)
                    throw new Exception("Handler not found for command: " + type.FullName);
                dynamic h = handlerObj;
                dynamic c = command;
                if (h.CanExecute(c))
                {
                    ok = h.Handle(c);
                    if (!ok) return false;
                }
            }
            return ok;
        }
        return false;
    }

    public void Clear(string? preserveNamespaceFragment = null)
    {
        if (string.IsNullOrEmpty(preserveNamespaceFragment))
        {
            _handlers.Clear();
            return;
        }

        foreach (var cmdType in _handlers.Keys.ToList())
        {
            var list = _handlers[cmdType];
            list.RemoveAll(t => t.Namespace == null ||
                t.Namespace.IndexOf(preserveNamespaceFragment, StringComparison.OrdinalIgnoreCase) < 0);
            if (list.Count == 0)
                _handlers.Remove(cmdType);
        }
    }

    public ILingoCommandManager Preload<TCommand>() where TCommand : ILingoCommand
    {
        var type = typeof(TCommand);
        if (!_handlers.TryGetValue(type, out var handlerTypes))
            return this;
        foreach (var handlerType in handlerTypes)
            _provider.GetService(handlerType);
        return this;
    }
}
