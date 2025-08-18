using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Windowing
{
    public interface IDirFrameworkDialog : IFrameworkFor<ILingoDialog>
    {
        void Init();
    }
}
