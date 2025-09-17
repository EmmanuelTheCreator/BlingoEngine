using AbstUI.Windowing;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.FrameworkCommunication;

namespace BlingoEngine.Director.Core.Texts
{
    public class DirectorTextEditWindow : DirectorWindow<IDirFrameworkTextEditWindow>
    {
        public TextEditIconBar IconBar { get; }

        public DirectorTextEditWindow(IBlingoFrameworkFactory factory, IServiceProvider serviceProvider) : base(serviceProvider, DirectorMenuCodes.TextEditWindow)
        {
            IconBar = new TextEditIconBar(factory);
            Width = 550;
            Height = 200;
            MinimumWidth = 400;
            MinimumHeight = 150;
            X = 1000;
            Y = 700;
        }

        protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
        {
            base.OnInit(frameworkWindow);
            Title = "Text Editor";
        }
    }
}

