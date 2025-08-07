using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.FileSystems;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Importer;
using LingoEngine.Director.Core.Texts;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Bitmaps;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Setup;
using LingoEngine.Director.Core.Scripts;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Scripts.Commands;
using LingoEngine.Director.Core.Sprites;

namespace LingoEngine.Director.Core
{
    public static class DirectorSetup
    {
        public static ILingoEngineRegistration WithDirectorEngine(this ILingoEngineRegistration engineRegistration)
        {
            engineRegistration.Services(s => s
                    .AddSingleton<IDirectorEventMediator, DirectorEventMediator>()
                    .AddSingleton<IDirectorShortCutManager, DirectorShortCutManager>()
                    .AddSingleton<IHistoryManager, HistoryManager>()
                    .AddSingleton<DirectorWindowManager>()
                    .AddSingleton<DirectorProjectManager>()
                    .AddSingleton<DirectorProjectSettings>()
                    .AddTransient<IDirectorWindowManager>(p => p.GetRequiredService<DirectorWindowManager>())
                    .AddTransient<IDirectorBehaviorDescriptionManager, DirectorBehaviorDescriptionManager>()

                    // File system
                    .AddSingleton<IIdePathResolver, IdePathResolver>()
                    .AddSingleton<IIdeLauncher, IdeLauncher>()
                    .AddSingleton<ProjectSettingsEditorState, ProjectSettingsEditorState>()

                    .AddSingleton<DirectorScriptsManager>()
                    .AddTransient(p => new Lazy<IDirectorScriptsManager>(() => p.GetRequiredService<DirectorScriptsManager>()))

                    // Windows
                    .AddSingleton<DirectorMainMenu>()

                    .AddSingleton<DirectorProjectSettingsWindow>()
                    .AddSingleton<DirectorToolsWindow>()
                    .AddSingleton<DirectorCastWindow>()
                    .AddSingleton<DirectorScoreWindow>()
                    .AddSingleton<DirectorPropertyInspectorWindow>()
                    .AddSingleton<DirectorStageGuides>()
                    .AddSingleton<DirectorBinaryViewerWindow>()
                    .AddSingleton<DirectorBinaryViewerWindowV2>()
                    .AddSingleton<DirectorStageWindow>()
                    .AddSingleton<DirectorTextEditWindow>()
                    .AddSingleton<DirectorBitmapEditWindow>()
                    .AddSingleton<DirectorImportExportWindow>()
                    .AddSingleton<DirStageManager>()
                    .AddTransient<IDirStageManager>(p => p.GetRequiredService<DirStageManager>())
                    .AddSingleton<DirScoreManager>()
                    .AddSingleton<DirSpritesManager>()
                    .AddSingleton<DirCastManager>()
                    .AddTransient<IDirSpritesManager>(p => p.GetRequiredService<DirSpritesManager>())
                    .AddTransient(p => new Lazy<IDirSpritesManager>(() => p.GetRequiredService<DirSpritesManager>()))
                    );
            engineRegistration.AddBuildAction(
                (serviceProvider) =>
                {
                    serviceProvider.RegisterDirectorWindows();
                });
            return engineRegistration;
        }

    }
}
