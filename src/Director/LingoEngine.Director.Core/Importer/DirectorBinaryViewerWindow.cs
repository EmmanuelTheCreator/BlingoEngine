using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using System.Numerics;

namespace LingoEngine.Director.Core.Importer
{
    public class DirectorBinaryViewerWindow : DirectorWindow<IDirFrameworkBinaryViewerWindow>
    {
        public DirectorBinaryViewerWindow(IServiceProvider serviceProvider) : base(serviceProvider, DirectorMenuCodes.BinaryViewerWindow) {
            Width = 1400;
            Height = 600;
        }
    }
}
