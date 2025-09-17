using AbstEngine.Director.LGodot;
using AbstUI.LGodot.Windowing;
using AbstUI.Windowing;
using Godot;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.Director.LGodot.Casts;
using BlingoEngine.Director.LGodot.Gfx;
using BlingoEngine.Director.LGodot.Inspector;
using BlingoEngine.Director.LGodot.Movies;
using BlingoEngine.Director.LGodot.Pictures;
using BlingoEngine.Director.LGodot.Projects;
using BlingoEngine.Director.LGodot.Scores;
using BlingoEngine.Director.LGodot.Styles;
using BlingoEngine.LGodot;
using BlingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Director.LGodot.UI
{
    public class BlingoGodotDirectorRoot : IDisposable
    {
        private readonly Control _directorParent = new();
        private readonly DirGodotCastWindow _castViewer;
        private readonly DirGodotScoreWindow _scoreWindow;
        private readonly DirGodotPropertyInspector _propertyInspector;
        private readonly DirGodotToolsWindow _toolsWindow;
        private readonly DirGodotStageWindow _stageWindow;
        private readonly DirGodotMainMenu _dirGodotMainMenu;
        private readonly DirGodotProjectSettingsWindow _projectSettingsWindow;
        private readonly DirGodotBinaryViewerWindow _binaryViewer;
        private readonly DirGodotBinaryViewerWindowV2 _binaryViewerV2;
        private readonly DirGodotImportExportWindow _importExportWindow;
        private readonly DirGodotTextableMemberWindowV2 _textWindow;
        private readonly DirGodotPictureMemberEditorWindow _picture;
        private List<BaseGodotWindow> _windows = new List<BaseGodotWindow>();

        public BlingoGodotDirectorRoot(BlingoPlayer player, IServiceProvider serviceProvider, BlingoProjectSettings startupProjectSettings)
        {

            // set up root
            //var parent = (Node2D)serviceProvider.GetRequiredService<BlingoGodotRootNode>().RootNode;
            //parent.AddChild(_directorParent);

            var godotWindowManager = serviceProvider.GetRequiredService<IAbstGodotWindowManager>();
            // Apply Director UI theme from IoC
            var style = serviceProvider.GetRequiredService<DirectorGodotStyle>();
            godotWindowManager.RootNode.Theme = style.Theme;

            var windowManager = serviceProvider.GetRequiredService<IAbstWindowManager>();
            windowManager.OpenWindow(DirectorMenuCodes.MainMenu);
            windowManager.OpenWindow(DirectorMenuCodes.CastWindow);
            windowManager.OpenWindow(DirectorMenuCodes.ScoreWindow);
            windowManager.OpenWindow(DirectorMenuCodes.PropertyInspector);
            windowManager.OpenWindow(DirectorMenuCodes.ToolsWindow);
            windowManager.OpenWindow(DirectorMenuCodes.StageWindow);

            // Create windows
            _dirGodotMainMenu = serviceProvider.GetRequiredService<DirGodotMainMenu>();
            godotWindowManager.RootNode.AddChild(_dirGodotMainMenu);
            //_stageWindow = serviceProvider.GetRequiredService<DirGodotStageWindow>();
            //_castViewer = serviceProvider.GetRequiredService<DirGodotCastWindow>();
            _scoreWindow = serviceProvider.GetRequiredService<DirGodotScoreWindow>();

            // todo : execute somewhere else
            //if (startupProjectSettings.StageWidth > 0 && startupProjectSettings.StageHeight > 0)
            //{
            //    var stageWidth = Math.Min(startupProjectSettings.StageWidth, 800);// scrollbars
            //    var stageHeight = Math.Min(startupProjectSettings.StageHeight, 600); // bottombar
            //                                                                         //var size = new Vector2(stageWidth, stageHeight);
            //                                                                         // _stageWindow.Size = size;
            //                                                                         //_stageWindow.CustomMinimumSize = size;
            //    _scoreWindow.SetThePosition(new Vector2(_scoreWindow.Position.X, 640 - 560 + stageHeight));
            //    serviceProvider.GetRequiredService<IAbstWindowManager>()
            //        .SetWindowSize(DirectorMenuCodes.StageWindow, stageWidth, stageHeight);
            //    foreach (var item in _windows)
            //        item.EnsureInBounds();
            //}
        }

        public void Dispose()
        {
            foreach (var item in _windows)
                item.QueueFree();
        }
    }
}

