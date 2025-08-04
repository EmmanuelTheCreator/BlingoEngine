using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Importer
{
    public class DirectorImportExportWindow : DirectorWindow<IDirFrameworkImportExportWindow>
    {
        public DirectorImportExportWindow(ILingoFrameworkFactory factory) : base(factory) { }
    }
}
