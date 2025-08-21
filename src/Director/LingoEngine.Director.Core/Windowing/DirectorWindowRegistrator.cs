using AbstUI;
using LingoEngine.Director.Core.Bitmaps;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Importer;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.Texts;
using LingoEngine.Director.Core.Tools.Commands;
using LingoEngine.Director.Core.UI;

namespace LingoEngine.Director.Core.Windowing
{
    public static class DirectorWindowRegistrator
    {
        internal static IAbstFameworkWindowRegistrator RegisterDirectorWindows(this IAbstFameworkWindowRegistrator registrator)
        {
            registrator
                .AddSingleton<DirectorProjectSettingsWindow>(DirectorMenuCodes.ProjectSettingsWindow)
                .AddSingleton<DirectorToolsWindow>(DirectorMenuCodes.ToolsWindow, s => s.CreateShortCut(DirectorMenuCodes.ToolsWindow, "Ctrl+7", sc => new ExecuteShortCutCommand(sc)))
                .AddSingleton<DirectorCastWindow>(DirectorMenuCodes.CastWindow, s => s.CreateShortCut(DirectorMenuCodes.CastWindow, "Ctrl+3", sc => new ExecuteShortCutCommand(sc)))
                .AddSingleton<DirectorScoreWindow>(DirectorMenuCodes.ScoreWindow, s => s.CreateShortCut(DirectorMenuCodes.ScoreWindow, "Ctrl+4", sc => new ExecuteShortCutCommand(sc)))
                .AddSingleton<DirectorPropertyInspectorWindow>(DirectorMenuCodes.PropertyInspector, s => s.CreateShortCut(DirectorMenuCodes.PropertyInspector, "Ctrl+Alt+S", sc => new ExecuteShortCutCommand(sc)))
                .AddSingleton<DirectorBinaryViewerWindow>(DirectorMenuCodes.BinaryViewerWindow)
                .AddSingleton<DirectorBinaryViewerWindowV2>(DirectorMenuCodes.BinaryViewerWindowV2)
                .AddSingleton<DirectorStageWindow>(DirectorMenuCodes.StageWindow, s => s.CreateShortCut(DirectorMenuCodes.StageWindow, "Ctrl+1", sc => new ExecuteShortCutCommand(sc)))
                .AddSingleton<DirectorTextEditWindow>(DirectorMenuCodes.TextEditWindow, s => s.CreateShortCut(DirectorMenuCodes.TextEditWindow, "Ctrl+T", sc => new ExecuteShortCutCommand(sc)))
                .AddSingleton<DirectorBitmapEditWindow>(DirectorMenuCodes.PictureEditWindow, s => s.CreateShortCut(DirectorMenuCodes.PictureEditWindow, "Ctrl+5", sc => new ExecuteShortCutCommand(sc)))
                .AddSingleton<DirectorImportExportWindow>(DirectorMenuCodes.ImportExportWindow)
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
