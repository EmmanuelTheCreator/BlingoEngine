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
using Microsoft.Extensions.DependencyInjection.Extensions;
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
                .ServicesMain(s =>
                {
                    s.AddSingleton<IDirectorEventMediator, DirectorEventMediator>();


                    s.AddSingleton<DirectorProjectManager>();
                    s.AddSingleton<LingoScriptCompiler>();
                    s.AddSingleton<DirectorProjectSettings>();
                    s.AddSingleton<DirectorProjectSettingsRepository>();
                    s.AddSingleton<LingoProjectSettingsRepository>();
                    s.AddTransient<IDirectorBehaviorDescriptionManager, DirectorBehaviorDescriptionManager>();

                    // File system
                    s.AddSingleton<IIdePathResolver, IdePathResolver>();
                    s.AddSingleton<IIdeLauncher, IdeLauncher>();
                    s.AddSingleton<ProjectSettingsEditorState, ProjectSettingsEditorState>();

                    s.AddSingleton<DirectorScriptsManager>();

                    s.AddTransient(p => new Lazy<IDirectorScriptsManager>(() => p.GetRequiredService<DirectorScriptsManager>()));

                    // Windows
                    s.AddSingleton<DirectorMainMenu>();

                    s.AddSingleton<DirectorProjectSettingsWindow>();
                    s.AddSingleton<DirectorToolsWindow>();
                    s.AddSingleton<DirectorCastWindow>();
                    s.AddSingleton<DirectorScoreWindow>();
                    s.AddSingleton<DirectorPropertyInspectorWindow>();
                    s.AddSingleton<DirBehaviorInspectorWindow>();
                    s.AddSingleton<DirectorStageGuides>();
                    s.AddSingleton<DirectorBinaryViewerWindow>();
                    s.AddSingleton<DirectorBinaryViewerWindowV2>();
                    s.AddSingleton<DirectorStageWindow>();
                    s.AddSingleton<DirectorTextEditWindowV2>();
                    s.AddSingleton<DirectorBitmapEditWindow>();
                    s.AddSingleton<DirectorImportExportWindow>();
                    s.AddSingleton<RNetConfiguration>();
                    s.AddSingleton<IRNetConfiguration>(p => p.GetRequiredService<RNetConfiguration>());
                    s.AddTransient<RNetSettingsDialog>();
                    s.AddSingleton<RNetSettingsDialogHandler>();
                    s.TryAddSingleton<IRNetProjectServer, DummyRNetProjectServer>();
                    s.AddSingleton<ILingoRNetProjectClient, LingoRNetProjectClient>();
                    s.AddSingleton<ILingoRNetPipeClient, RNetPipeClient>();
                    s.AddSingleton<DirectorRNetServer>();
                    s.AddSingleton<DirectorRNetClient>();
                    s.AddSingleton<DirStageManager>();
                    s.AddTransient<IDirStageManager>(p => p.GetRequiredService<DirStageManager>());
                    s.AddSingleton<DirScoreManager>();
                    s.AddSingleton<DirSpritesManager>();
                    s.AddSingleton<DirCastManager>();
                    s.AddTransient<IDirSpritesManager>(p => p.GetRequiredService<DirSpritesManager>());
                    s.AddTransient(p => new Lazy<IDirSpritesManager>(() => p.GetRequiredService<DirSpritesManager>()));
                    // Handlers
                    s.AddTransient<CompileProjectCommandHandler>();
                    s.AddTransient<LingoCSharpConverterPopup>();
                    s.AddTransient<LingoCSharpConverterPopupHandler>();
                    s.AddTransient<LingoCodeImporterPopup>();
                    s.AddTransient<LingoCodeImporterPopupHandler>();

                });
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
