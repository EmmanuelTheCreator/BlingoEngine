using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Projects
{
    public class DirectorProjectSettingsWindow : DirectorWindow<IDirFrameworkProjectSettingsWindow>
    {
        public DirectorProjectSettingsWindow(ILingoFrameworkFactory factory) : base(factory) { }
    }
}
