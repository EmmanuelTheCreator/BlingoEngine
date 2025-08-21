using Godot;
using LingoEngine.Director.LGodot.Scores;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Core;
using LingoEngine.LGodot;
using LingoEngine.Director.LGodot.Casts;
using LingoEngine.Director.LGodot.Inspector;
using LingoEngine.Director.LGodot.Movies;
using LingoEngine.Director.LGodot.Pictures;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.LGodot.Styles;
using LingoEngine.Director.LGodot.Projects;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Projects;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Director.Core.UI;
using AbstEngine.Director.LGodot;
using AbstUI.Windowing;

namespace LingoEngine.Director.LGodot.UI
{
    public class LingoGodotDirectorRoot : IDisposable
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
        private readonly DirGodotTextableMemberWindow _textWindow;
        private readonly DirGodotPictureMemberEditorWindow _picture;
        private List<BaseGodotWindow> _windows = new List<BaseGodotWindow>();

        public LingoGodotDirectorRoot(LingoPlayer player, IServiceProvider serviceProvider, LingoProjectSettings startupProjectSettings)
        {

            _directorParent.Name = "DirectorRoot";
            // set up root
            var parent = (Node2D)serviceProvider.GetRequiredService<LingoGodotRootNode>().RootNode;
            parent.AddChild(_directorParent);

            // Apply Director UI theme from IoC
            var style = serviceProvider.GetRequiredService<DirectorGodotStyle>();
            _directorParent.Theme = style.Theme;

            // Create windows
            _dirGodotMainMenu = serviceProvider.GetRequiredService<DirGodotMainMenu>();
            _stageWindow = serviceProvider.GetRequiredService<DirGodotStageWindow>();
            _castViewer = serviceProvider.GetRequiredService<DirGodotCastWindow>();
            _scoreWindow = serviceProvider.GetRequiredService<DirGodotScoreWindow>();
            _propertyInspector = serviceProvider.GetRequiredService<DirGodotPropertyInspector>();
            _toolsWindow = serviceProvider.GetRequiredService<DirGodotToolsWindow>();
            _binaryViewer = serviceProvider.GetRequiredService<DirGodotBinaryViewerWindow>();
            _binaryViewerV2 = serviceProvider.GetRequiredService<DirGodotBinaryViewerWindowV2>();
            _importExportWindow = serviceProvider.GetRequiredService<DirGodotImportExportWindow>();
            _textWindow = serviceProvider.GetRequiredService<DirGodotTextableMemberWindow>();
            _picture = serviceProvider.GetRequiredService<DirGodotPictureMemberEditorWindow>();
            _projectSettingsWindow = serviceProvider.GetRequiredService<DirGodotProjectSettingsWindow>();

            _windows.Add(_stageWindow);
            _windows.Add(_castViewer);
            _windows.Add(_scoreWindow);
            _windows.Add(_propertyInspector);
            _windows.Add(_toolsWindow);
            _windows.Add(_binaryViewer);
            _windows.Add(_binaryViewerV2);
            _windows.Add(_importExportWindow);
            _windows.Add(_textWindow);
            _windows.Add(_picture);
            _windows.Add(_projectSettingsWindow);

            _directorParent.AddChild(_dirGodotMainMenu);
            foreach (var item in _windows)
                _directorParent.AddChild(item);


            foreach (var item in _windows)
                item.EnsureInBounds();

            // close some windows
            _projectSettingsWindow.CloseWindow();
            _binaryViewer.CloseWindow();
            _binaryViewerV2.CloseWindow();
            _importExportWindow.CloseWindow();
            _textWindow.CloseWindow();
            _picture.CloseWindow();

            if (startupProjectSettings.StageWidth > 0 && startupProjectSettings.StageHeight > 0)
            {
                var stageWidth = Math.Min(startupProjectSettings.StageWidth, 800);// scrollbars
                var stageHeight = Math.Min(startupProjectSettings.StageHeight, 600); // bottombar
                                                                                     //var size = new Vector2(stageWidth, stageHeight);
                                                                                     // _stageWindow.Size = size;
                                                                                     //_stageWindow.CustomMinimumSize = size;
                _scoreWindow.SetThePosition(new Vector2(_scoreWindow.Position.X, 640 - 560 + stageHeight));
                serviceProvider.GetRequiredService<IAbstWindowManager>()
                    .SetWindowSize(DirectorMenuCodes.StageWindow, stageWidth, stageHeight);
                foreach (var item in _windows)
                    item.EnsureInBounds();
            }
        }

        public void Dispose()
        {
            foreach (var item in _windows)
                item.QueueFree();
        }
    }
}
