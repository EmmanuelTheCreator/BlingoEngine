using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Texts
{
    public class DirectorTextEditWindow : DirectorWindow<IDirFrameworkTextEditWindow>
    {
        public DirectorTextEditWindow(ILingoFrameworkFactory factory) : base(factory) { }
    }
}
