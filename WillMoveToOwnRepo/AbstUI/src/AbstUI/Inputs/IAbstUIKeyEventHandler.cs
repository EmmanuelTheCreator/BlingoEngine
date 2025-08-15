namespace AbstUI.Inputs
{
    public interface IAbstUIKeyEventHandler
    {
        void RaiseKeyDown(AbstUIKey lingoKey);
        void RaiseKeyUp(AbstUIKey lingoKey);
    }
}
