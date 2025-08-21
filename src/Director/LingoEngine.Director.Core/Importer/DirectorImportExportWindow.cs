using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using System.Numerics;

namespace LingoEngine.Director.Core.Importer
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
