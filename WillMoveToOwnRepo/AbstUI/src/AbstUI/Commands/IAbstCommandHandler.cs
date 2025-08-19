namespace AbstUI.Commands;

public interface IAbstCommandHandler<TCommand> where TCommand : IAbstCommand
{
#if NET48
    bool CanExecute(TCommand command);
#else
    bool CanExecute(TCommand command) => true;
#endif
    bool Handle(TCommand command);
}
