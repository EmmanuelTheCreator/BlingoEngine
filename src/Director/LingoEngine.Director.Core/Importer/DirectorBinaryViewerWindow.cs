using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using System.Numerics;

namespace LingoEngine.Director.Core.Importer
{
    public class DirectorBinaryViewerWindow : DirectorWindow<IDirFrameworkBinaryViewerWindow>
    {
        public DirectorBinaryViewerWindow(IServiceProvider serviceProvider) : base(serviceProvider, DirectorMenuCodes.BinaryViewerWindow)
        {
            Width = 1400;
            Height = 600;
            MinimumWidth = 200;
            MinimumHeight = 150;
            X = 20;
            Y = 120;
        }
    }
}
