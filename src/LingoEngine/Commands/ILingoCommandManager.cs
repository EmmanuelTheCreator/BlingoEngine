namespace LingoEngine.Commands;

public interface ILingoCommandManager
{
    public ILingoCommandManager Register<THandler, TCommand>()
        where THandler : ICommandHandler<TCommand>
        where TCommand : ILingoCommand;
    ILingoCommandManager Register(Type handlerType);
    bool Handle(ILingoCommand command);
    void Clear(string? preserveNamespaceFragment = null);
    /// <summary>
    /// Services that needs to be preloaded and resolved to subscribe to the command handler.
    /// </summary>
    ILingoCommandManager Preload<TCommand>() where TCommand : ILingoCommand;
}
