using LingoEngine.Core;
using LingoEngine.Director.SDL2.Casts;
using LingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.SDL2.UI
{
    public class LingoSdlDirectorRoot : IDisposable
    {
        private readonly DirSdlCastWindow _castWindow;

        public LingoSdlDirectorRoot(LingoPlayer player, IServiceProvider services, LingoProjectSettings settings)
        {
            _castWindow = services.GetRequiredService<DirSdlCastWindow>();
            _castWindow.Popup();
        }

        public void Dispose()
        {
            _castWindow.Dispose();
        }
    }
}
