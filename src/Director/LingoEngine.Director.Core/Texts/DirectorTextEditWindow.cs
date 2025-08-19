using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Texts
{
    public class DirectorTextEditWindow : DirectorWindow<IDirFrameworkTextEditWindow>
    {
        public TextEditIconBar IconBar { get; }

        public DirectorTextEditWindow(ILingoFrameworkFactory factory, IServiceProvider serviceProvider) : base(serviceProvider, DirectorMenuCodes.TextEditWindow)
        {
            IconBar = new TextEditIconBar(factory);
        }
    }
}
