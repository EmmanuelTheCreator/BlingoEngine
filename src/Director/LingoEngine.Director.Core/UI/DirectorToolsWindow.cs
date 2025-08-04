using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.UI
{
    public class DirectorToolsWindow : DirectorWindow<IDirFrameworkToolsWindow>
    {
        public DirectorToolsWindow(ILingoFrameworkFactory factory) : base(factory) { }
    }
}
