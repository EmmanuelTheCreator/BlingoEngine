using BlingoEngine.Director.Core.Inspector;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.FrameworkCommunication;
using System.Numerics;

namespace BlingoEngine.Director.Core.Importer
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

