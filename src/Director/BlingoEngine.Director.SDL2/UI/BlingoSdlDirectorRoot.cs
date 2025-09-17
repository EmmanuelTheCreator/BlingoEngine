using AbstUI.Windowing;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.SDL2.Casts;
using BlingoEngine.Director.SDL2.Inspector;
using BlingoEngine.Director.SDL2.Scores;
using BlingoEngine.Director.SDL2.Stages;
using BlingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Director.SDL2.UI
{
    public class BlingoSdlDirectorRoot : IDisposable
    {
        private readonly DirSdlCastWindow _castWindow;
        private readonly DirSdlStageWindow _stageWindow;

        private readonly DirSdlScoreWindow _scoreWindow;

        private readonly DirSdlPropertyInspectorWindow _inspectorWindow;


        public BlingoSdlDirectorRoot(BlingoPlayer player, IServiceProvider services, BlingoProjectSettings settings)
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

