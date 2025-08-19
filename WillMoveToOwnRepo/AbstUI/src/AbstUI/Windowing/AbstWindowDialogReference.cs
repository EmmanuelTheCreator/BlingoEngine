namespace AbstUI.Windowing;

public interface IAbstWindowDialogReference
{
    void Close();
}

public class AbstWindowDialogReference : IAbstWindowDialogReference
{
    private readonly Action _closeAction;
    public AbstWindowDialogReference(Action closeAction) => _closeAction = closeAction;
    public void Close() => _closeAction();
}
