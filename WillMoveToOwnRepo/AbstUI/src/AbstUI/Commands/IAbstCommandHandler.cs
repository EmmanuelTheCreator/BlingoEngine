namespace AbstUI.Commands;

public interface IAbstCommandHandler<TCommand> where TCommand : IAbstCommand
{
    bool CanExecute(TCommand command);
    bool Handle(TCommand command);
}
