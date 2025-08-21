using LingoEngine.Core;
using LingoEngine.Director.SDL2.Casts;
using LingoEngine.Director.SDL2.Inspector;
using LingoEngine.Director.SDL2.Stages;
using LingoEngine.Director.SDL2.Scores;
using LingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.SDL2.UI
{
    public class LingoSdlDirectorRoot : IDisposable
    {
        private readonly DirSdlCastWindow _castWindow;
        private readonly DirSdlStageWindow _stageWindow;

        private readonly DirSdlScoreWindow _scoreWindow;

        private readonly DirSdlPropertyInspectorWindow _inspectorWindow;


        public LingoSdlDirectorRoot(LingoPlayer player, IServiceProvider services, LingoProjectSettings settings)
        {
            //_castWindow = services.GetRequiredService<DirSdlCastWindow>();
            _inspectorWindow = services.GetRequiredService<DirSdlPropertyInspectorWindow>();
            //_stageWindow = services.GetRequiredService<DirSdlStageWindow>();
            //_scoreWindow = services.GetRequiredService<DirSdlScoreWindow>();

            //_castWindow.Popup();
            //_stageWindow.Popup();
            //_scoreWindow.Popup();
            _inspectorWindow.Popup();

        }

        public void Dispose()
        {
            _castWindow.Dispose();
            _stageWindow.Dispose();

            _scoreWindow.Dispose();
            _inspectorWindow.Dispose();

        }
    }
}
