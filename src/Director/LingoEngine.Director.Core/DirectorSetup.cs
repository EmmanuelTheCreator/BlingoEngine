using AbstUI.Commands;
using AbstUI.Components;
using AbstUI.Windowing;
using LingoEngine.Director.Core.Bitmaps;
using LingoEngine.Director.Core.Bitmaps.Commands;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Casts.Commands;
using LingoEngine.Director.Core.Compilers;
using LingoEngine.Director.Core.Compilers.Commands;
using LingoEngine.Director.Core.FileSystems;
using LingoEngine.Director.Core.Importer;
using LingoEngine.Director.Core.Importer.Commands;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Inspector.Commands;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Projects.Commands;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Scripts;
using LingoEngine.Director.Core.Scripts.Commands;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Sprites.Behaviors;
using LingoEngine.Director.Core.Behaviors;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.Stages.Commands;
using LingoEngine.Director.Core.Texts;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Director.Core.Remote;
using LingoEngine.Director.Core.Remote.Commands;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Net.RNetHost.Common;
using LingoEngine.Net.RNetProjectClient;
using LingoEngine.Net.RNetPipeClient;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Sprites.BehaviorLibrary;

namespace LingoEngine.Director.Core
{
    public static class DirectorSetup
    {
        public static ILingoEngineRegistration WithDirectorEngine(this ILingoEngineRegistration engineRegistration, Action<DirectorProjectSettings>? directorSettingsConfig = null)
        {
            engineRegistration
                .RegisterWindows(r => DirectorWindowRegistrator.RegisterDirectorWindows(r))
                .RegisterComponents(r => DirectorWindowRegistrator.RegisterDirectorComponents(r))
                .ServicesMain(s => s
                    .AddSingleton<IDirectorEventMediator, DirectorEventMediator>()


                    .AddSingleton<DirectorProjectManager>()
                    .AddSingleton<LingoScriptCompiler>()
                    .AddSingleton<DirectorProjectSettings>()
                    .AddSingleton<DirectorProjectSettingsRepository>()
                    .AddSingleton<LingoProjectSettingsRepository>()
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
                    .AddSingleton<DirBehaviorInspectorWindow>()
                    .AddSingleton<DirectorStageGuides>()
                    .AddSingleton<DirectorBinaryViewerWindow>()
                    .AddSingleton<DirectorBinaryViewerWindowV2>()
                    .AddSingleton<DirectorStageWindow>()
                    .AddSingleton<DirectorTextEditWindowV2>()
                    .AddSingleton<DirectorBitmapEditWindow>()
                    .AddSingleton<DirectorImportExportWindow>()
                    .AddSingleton<RNetConfiguration>()
                    .AddSingleton<IRNetConfiguration>(p => p.GetRequiredService<RNetConfiguration>())
                    .AddTransient<RNetSettingsDialog>()
                    .AddSingleton<RNetSettingsDialogHandler>()
                    .AddDirectorDummyProjectServer()
                    .AddSingleton<ILingoRNetProjectClient, LingoRNetProjectClient>()
                    .AddSingleton<ILingoRNetPipeClient, RNetPipeClient>()
                    .AddSingleton<DirectorRNetServer>()
                    .AddSingleton<DirectorRNetClient>()
                    .AddSingleton<DirStageManager>()
                    .AddTransient<IDirStageManager>(p => p.GetRequiredService<DirStageManager>())
                    .AddSingleton<DirScoreManager>()
                    .AddSingleton<DirSpritesManager>()
                    .AddSingleton<DirCastManager>()
                    .AddTransient<IDirSpritesManager>(p => p.GetRequiredService<DirSpritesManager>())
                    .AddTransient(p => new Lazy<IDirSpritesManager>(() => p.GetRequiredService<DirSpritesManager>()))
                    // Handlers
                    .AddTransient<CompileProjectCommandHandler>()
                    .AddTransient<LingoCSharpConverterPopup>()
                    .AddTransient<LingoCSharpConverterPopupHandler>()
                    .AddTransient<LingoCodeImporterPopup>()
                    .AddTransient<LingoCodeImporterPopupHandler>()

                );
            engineRegistration.AddBuildAction(
                (serviceProvider) =>
                {
                    //serviceProvider.RegisterDirectorWindows();
                    //serviceProvider.GetRequiredService<IAbstComponentFactory>();
                    //serviceProvider.GetRequiredService< LingoCSharpConverterPopupHandler>();
                    var behaviorLib = serviceProvider.GetRequiredService<ILingoBehaviorLibrary>();
                    behaviorLib.Register(new LingoBehaviorDefinition("GetServerIP", typeof(Sprites.Behaviors.GetServerIPBehavior), "Network"));

                    if (directorSettingsConfig != null)
                    {
                        //var settings = new DirectorProjectSettings();
                        //directorSettingsConfig(settings)
                        //serviceProvider.GetRequiredService<IAbstWindowManager>()


                    }
                    var cmdManager = serviceProvider.GetRequiredService<IAbstCommandManager>();
                    cmdManager
                        .Register<DirStageManager, StageChangeBackgroundColorCommand>()
                        .Register<DirectorStageWindow, StageToolSelectCommand>()
                        .Register<DirectorStageWindow, MoveSpritesCommand>()
                        .Register<DirectorStageWindow, RotateSpritesCommand>()
                        .Register<DirCastManager, CreateFilmLoopCommand>()
                        .Register<DirectorScriptsManager, OpenScriptCommand>()
                        .Register<LingoCodeImporterPopupHandler, OpenLingoCodeImporterCommand>()
                        .Register<LingoCSharpConverterPopupHandler, OpenLingoCSharpConverterCommand>()
                        .Register<DirectorProjectManager, SaveDirProjectSettingsCommand>()
                        .Register<DirectorPropertyInspectorWindow, OpenBehaviorPopupCommand>()
                        .Register<CompileProjectCommandHandler, CompileProjectCommand>()
                        .Register<DirSpritesManager, ChangeSpriteRangeCommand>()
                        .Register<DirSpritesManager, AddSpriteCommand>()
                        .Register<DirSpritesManager, RemoveSpriteCommand>()
                        .Register<DirectorBitmapEditWindow, PainterToolSelectCommand>()
                        .Register<DirectorBitmapEditWindow, PainterDrawPixelCommand>()
                        .Register<DirectorBitmapEditWindow, PainterFillCommand>()
                        .Register<DirectorRNetServer, ConnectRNetServerCommand>()
                        .Register<DirectorRNetServer, DisconnectRNetServerCommand>()
                        .Register<DirectorRNetClient, ConnectRNetClientCommand>()
                        .Register<DirectorRNetClient, DisconnectRNetClientCommand>()
                        .Register<RNetSettingsDialogHandler, OpenRNetSettingsCommand>();

                });
            return engineRegistration;
        }

    }
}
