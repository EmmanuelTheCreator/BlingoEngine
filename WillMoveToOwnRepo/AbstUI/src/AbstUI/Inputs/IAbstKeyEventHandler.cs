namespace AbstUI.Inputs
{
    public interface IAbstKeyEventHandler
    {
        void RaiseKeyDown(AbstKey lingoKey);
        void RaiseKeyUp(AbstKey lingoKey);
    }
}
