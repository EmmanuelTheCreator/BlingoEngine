using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.FrameworkCommunication;
using System.Numerics;

namespace BlingoEngine.Director.Core.Importer
{
    public class DirectorImportExportWindow : DirectorWindow<IDirFrameworkImportExportWindow>
    {
        public DirectorImportExportWindow(IServiceProvider serviceProvider) : base(serviceProvider, DirectorMenuCodes.ImportExportWindow)
        {
            Width = 400;
            Height = 300;
            MinimumWidth = 200;
            MinimumHeight = 150;
            X = 120;
            Y = 120;
        }
    }
}

