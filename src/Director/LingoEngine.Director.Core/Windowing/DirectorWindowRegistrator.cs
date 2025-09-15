using AbstUI;
using LingoEngine.Director.Core.Bitmaps;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Behaviors;
using LingoEngine.Director.Core.Importer;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.Texts;
using LingoEngine.Director.Core.Tools.Commands;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Remote;

namespace LingoEngine.Director.Core.Windowing
{
    public static class DirectorWindowRegistrator
    {
        internal static IAbstFameworkWindowRegistrator RegisterDirectorWindows(this IAbstFameworkWindowRegistrator registrator)
        {
            registrator
                .AddSingletonWindow<DirectorProjectSettingsWindow>(DirectorMenuCodes.ProjectSettingsWindow)
                .AddSingletonWindow<DirectorToolsWindow>(DirectorMenuCodes.ToolsWindow, s => s.CreateShortCut(DirectorMenuCodes.ToolsWindow, "Ctrl+7", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorCastWindow>(DirectorMenuCodes.CastWindow, s => s.CreateShortCut(DirectorMenuCodes.CastWindow, "Ctrl+3", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorScoreWindow>(DirectorMenuCodes.ScoreWindow, s => s.CreateShortCut(DirectorMenuCodes.ScoreWindow, "Ctrl+4", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorPropertyInspectorWindow>(DirectorMenuCodes.PropertyInspector, s => s.CreateShortCut(DirectorMenuCodes.PropertyInspector, "Ctrl+Alt+S", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirBehaviorInspectorWindow>(DirectorMenuCodes.BehaviorInspectorWindow)
                .AddSingletonWindow<DirectorBinaryViewerWindow>(DirectorMenuCodes.BinaryViewerWindow)
                .AddSingletonWindow<DirectorBinaryViewerWindowV2>(DirectorMenuCodes.BinaryViewerWindowV2)
                .AddSingletonWindow<DirectorStageWindow>(DirectorMenuCodes.StageWindow, s => s.CreateShortCut(DirectorMenuCodes.StageWindow, "Ctrl+1", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorTextEditWindowV2>(DirectorMenuCodes.TextEditWindow, s => s.CreateShortCut(DirectorMenuCodes.TextEditWindow, "Ctrl+T", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorBitmapEditWindow>(DirectorMenuCodes.PictureEditWindow, s => s.CreateShortCut(DirectorMenuCodes.PictureEditWindow, "Ctrl+5", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorImportExportWindow>(DirectorMenuCodes.ImportExportWindow)
                .AddSingletonWindow<DirectorRemoteSettingsWindow>(DirectorMenuCodes.RemoteSettingsWindow)
                // .Register<DirectorMainMenu>(DirectorMenuCodes.MainMenu)
                ;

            return registrator;
        }
        internal static IAbstFameworkComponentRegistrator RegisterDirectorComponents(this IAbstFameworkComponentRegistrator registrator)
        {
            return registrator;
        }

    }
}
