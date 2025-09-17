using AbstUI.Windowing;
using LingoEngine.Core;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.SDL2.Casts;
using LingoEngine.Director.SDL2.Inspector;
using LingoEngine.Director.SDL2.Scores;
using LingoEngine.Director.SDL2.Stages;
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
            var windowManager= services.GetRequiredService<IAbstWindowManager>();

            windowManager.OpenWindow(DirectorMenuCodes.PropertyInspector);
            windowManager.OpenWindow(DirectorMenuCodes.CastWindow);
            windowManager.OpenWindow(DirectorMenuCodes.ScoreWindow);
            windowManager.OpenWindow(DirectorMenuCodes.StageWindow);
            //windowManager.OpenWindow(DirectorMenuCodes.ToolsWindow);
            windowManager.OpenWindow(DirectorMenuCodes.MainMenu);
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
