using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Importer
{
    public class DirectorBinaryViewerWindow : DirectorWindow<IDirFrameworkBinaryViewerWindow>
    {
        public DirectorBinaryViewerWindow(ILingoFrameworkFactory factory) : base(factory) { }
    }
}
