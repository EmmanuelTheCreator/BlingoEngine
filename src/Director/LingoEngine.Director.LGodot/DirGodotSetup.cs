using AbstUI;
using AbstUI.Components;
using AbstUI.LGodot;
using AbstUI.LGodot.Styles;
using AbstUI.LGodot.Windowing;
using AbstUI.Styles;
using AbstUI.Windowing;
using Godot;
using LingoEngine.Core;
using LingoEngine.Director.Core;
using LingoEngine.Director.Core.Bitmaps;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Behaviors;
using LingoEngine.Director.Core.FileSystems;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Importer;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.LGodot.Casts;
using LingoEngine.Director.LGodot.Behaviors;
using LingoEngine.Director.LGodot.FileSystems;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.LGodot.Icons;
using LingoEngine.Director.LGodot.Inspector;
using LingoEngine.Director.LGodot.Movies;
using LingoEngine.Director.LGodot.Pictures;
using LingoEngine.Director.LGodot.Projects;
using LingoEngine.Director.LGodot.Scores;
using LingoEngine.Director.LGodot.Styles;
using LingoEngine.Director.LGodot.Tools;
using LingoEngine.Director.LGodot.UI;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.LGodot;
using LingoEngine.Projects;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LingoEngine.Director.LGodot
{
    public static class DirGodotSetup
    {
        public static ILingoEngineRegistration WithDirectorGodotEngine(this ILingoEngineRegistration engineRegistration, Node rootNode, Action<DirectorProjectSettings>? directorSettingsConfig = null, Action<IAbstFameworkComponentWinRegistrator>? windowRegistrations = null)
        {
            LingoEngineGlobal.IsRunningDirector = true;
            engineRegistration
                .WithLingoGodotEngine(rootNode, true, null, windowRegistrations)
                .WithDirectorEngine(directorSettingsConfig)
                .ServicesMain(s =>
                {
                    s
                    .AddSingleton<DirGodotWindowManagerDecorator>()
                    .AddSingleton<DirectorGodotStyle>()
                    .AddSingleton<DirGodotProjectSettingsWindow>()
                    .AddSingleton<DirGodotToolsWindow>()
                    .AddSingleton<DirGodotCastWindow>()
                    .AddSingleton<DirGodotScoreWindow>()
                    .AddSingleton<DirGodotStageWindow>()
                    .AddSingleton<DirGodotBehaviorLibraryWindow>()
                    .AddSingleton<DirGodotBinaryViewerWindow>()
                    .AddSingleton<DirGodotBinaryViewerWindowV2>()
                    .AddSingleton<DirGodotImportExportWindow>()
                    .AddSingleton<DirGodotPropertyInspector>()
                    .AddSingleton<DirGodotTextableMemberWindow>()
                    .AddSingleton<DirGodotPictureMemberEditorWindow>()
                    .AddSingleton<DirGodotMainMenu>()
                    .AddSingleton<IDirFilePicker, GodotFilePicker>()
                    .AddSingleton<IDirFolderPicker, GodotFolderPicker>()
                    .AddTransient<GodotLingoCSharpConverterPopup>()

                    .AddTransient<DirCodeHighlichter>()
                    .AddTransient<DirGodotCodeHighlighter>()

                    //.AddSingleton<AbstGodotFrameworkFactory>()
                    .AddSingleton<IDirectorIconManager>(p =>
                    {
                        var iconManager = new DirGodotIconManager(p.GetRequiredService<ILogger<DirGodotIconManager>>());
                        iconManager.LoadSheet("Media/Icons/General_Icons.png", 20, 16, 16, 8);
                        iconManager.LoadSheet("Media/Icons/Painter_Icons.png", 20, 16, 16, 8);
                        iconManager.LoadSheet("Media/Icons/Painter2_Icons.png", 20, 16, 16, 8);
                        iconManager.LoadSheet("Media/Icons/Director_Icons.png", 20, 16, 16, 8);
                        return iconManager;
                    });

                    s.AddTransient<IDirFrameworkProjectSettingsWindow>(p => p.GetRequiredService<DirGodotProjectSettingsWindow>());
                    s.AddTransient<IDirFrameworkToolsWindow>(p => p.GetRequiredService<DirGodotToolsWindow>());
                    s.AddTransient<IDirFrameworkCastWindow>(p => p.GetRequiredService<DirGodotCastWindow>());
                    s.AddTransient<IDirFrameworkScoreWindow>(p => p.GetRequiredService<DirGodotScoreWindow>());
                    s.AddTransient<IDirFrameworkStageWindow>(p => p.GetRequiredService<DirGodotStageWindow>());
                    s.AddTransient<IDirFrameworkBehaviorLibraryWindow>(p => p.GetRequiredService<DirGodotBehaviorLibraryWindow>());
                    s.AddTransient<IDirFrameworkBinaryViewerWindow>(p => p.GetRequiredService<DirGodotBinaryViewerWindow>());
                    s.AddTransient<IDirFrameworkBinaryViewerWindowV2>(p => p.GetRequiredService<DirGodotBinaryViewerWindowV2>());
                    s.AddTransient<IDirFrameworkPropertyInspectorWindow>(p => p.GetRequiredService<DirGodotPropertyInspector>());
                    s.AddTransient<IDirFrameworkBitmapEditWindow>(p => p.GetRequiredService<DirGodotPictureMemberEditorWindow>());
                    s.AddTransient<IDirFrameworkImportExportWindow>(p => p.GetRequiredService<DirGodotImportExportWindow>());



                })
                .AddPreBuildAction(p =>
                {
                    p.WithAbstUIGodot();
                    p.GetRequiredService<DirGodotWindowManagerDecorator>(); // register for window events;
                })
                .AddBuildAction(p =>
                {
                    var styles = p.GetRequiredService<DirectorGodotStyle>();
                    p.GetRequiredService<IAbstFontManager>().SetDefaultFont(DirectorGodotStyle.DefaultFont);
                    p.GetRequiredService<IAbstGodotStyleManager>()
                           .Register(AbstGodotThemeElementType.Tabs, styles.GetTabContainerTheme())
                           .Register(AbstGodotThemeElementType.TabItem, styles.GetTabItemTheme())
                           .Register(AbstGodotThemeElementType.PopupWindow, styles.GetPopupWindowTheme());
                    p.GetRequiredService<IAbstComponentFactory>()
                        .DiscoverInAssembly(typeof(DirGodotSetup).Assembly)
                    //.Register<IAbstDialog<>, IAbstGodotDialog<LingoCodeImporterPopup>>()
                    // .Register<DirCodeHighlichter,DirGodotCodeHighlighter>()
                    ;
                    new LingoGodotDirectorRoot(p.GetRequiredService<LingoPlayer>(), p, p.GetRequiredService<LingoProjectSettings>());
                });
            return engineRegistration;
        }

    }
}
