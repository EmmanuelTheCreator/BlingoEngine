namespace AbstUI.Windowing;

public interface IAbstWindowDialogReference
{
    IAbstFrameworkDialog? Dialog { get; }
    void Close();
}

public class AbstWindowDialogReference : IAbstWindowDialogReference
{
    private readonly Action _closeAction;
    public IAbstFrameworkDialog? Dialog { get; }

    public AbstWindowDialogReference(Action closeAction, IAbstFrameworkDialog? dialog = null)
    {
        _closeAction = closeAction;
        Dialog = dialog;
    }


    public void Close() => _closeAction();
}
