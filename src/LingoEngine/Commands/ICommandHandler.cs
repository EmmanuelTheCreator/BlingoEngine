namespace LingoEngine.Commands;

public interface ICommandHandler<TCommand> where TCommand : ILingoCommand
{
#if NET48
    bool CanExecute(TCommand command);
#else
    bool CanExecute(TCommand command) => true;
#endif
    bool Handle(TCommand command);
}
