using AbstUI;
using AbstUI.Components;
using AbstUI.LGodot;
using AbstUI.LGodot.Styles;
using AbstUI.LGodot.Windowing;
using AbstUI.Styles;
using AbstUI.Windowing;
using Godot;
using BlingoEngine.Core;
using BlingoEngine.Director.Core;
using BlingoEngine.Director.Core.Bitmaps;
using BlingoEngine.Director.Core.Casts;
using BlingoEngine.Director.Core.Behaviors;
using BlingoEngine.Director.Core.FileSystems;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Importer;
using BlingoEngine.Director.Core.Inspector;
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.Core.Scores;
using BlingoEngine.Director.Core.Stages;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.Texts;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.LGodot.Casts;
using BlingoEngine.Director.LGodot.Behaviors;
using BlingoEngine.Director.LGodot.FileSystems;
using BlingoEngine.Director.LGodot.Gfx;
using BlingoEngine.Director.LGodot.Icons;
using BlingoEngine.Director.LGodot.Inspector;
using BlingoEngine.Director.LGodot.Movies;
using BlingoEngine.Director.LGodot.Pictures;
using BlingoEngine.Director.LGodot.Projects;
using BlingoEngine.Director.LGodot.Scores;
using BlingoEngine.Director.LGodot.Styles;
using BlingoEngine.Director.LGodot.Tools;
using BlingoEngine.Director.LGodot.UI;
using BlingoEngine.Director.LGodot.Windowing;
using BlingoEngine.LGodot;
using BlingoEngine.Projects;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Director.LGodot
{
    public static class DirGodotSetup
    {
        public static IBlingoEngineRegistration WithDirectorGodotEngine(this IBlingoEngineRegistration engineRegistration, Node rootNode, Action<DirectorProjectSettings>? directorSettingsConfig = null, Action<IAbstFameworkComponentWinRegistrator>? windowRegistrations = null)
        {
            BlingoEngineGlobal.IsRunningDirector = true;
            engineRegistration
                .WithBlingoGodotEngine(rootNode, true, null, windowRegistrations)
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
                    .AddSingleton<DirGodotBehaviorInspectorWindow>()
                    .AddSingleton<DirGodotBinaryViewerWindow>()
                    .AddSingleton<DirGodotBinaryViewerWindowV2>()
                    .AddSingleton<DirGodotImportExportWindow>()
                    .AddSingleton<DirGodotPropertyInspector>()
                    .AddSingleton<DirGodotTextableMemberWindowV2>()
                    .AddSingleton<DirGodotPictureMemberEditorWindow>()
                    .AddSingleton<DirGodotMainMenu>()
                    .AddSingleton<IDirFilePicker, GodotFilePicker>()
                    .AddSingleton<IDirFolderPicker, GodotFolderPicker>()
                    .AddTransient<GodotBlingoCSharpConverterPopup>()

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
                    s.AddTransient<IDirFrameworkBehaviorInspectorWindow>(p => p.GetRequiredService<DirGodotBehaviorInspectorWindow>());
                    s.AddTransient<IDirFrameworkBinaryViewerWindow>(p => p.GetRequiredService<DirGodotBinaryViewerWindow>());
                    s.AddTransient<IDirFrameworkBinaryViewerWindowV2>(p => p.GetRequiredService<DirGodotBinaryViewerWindowV2>());
                    s.AddTransient<IDirFrameworkPropertyInspectorWindow>(p => p.GetRequiredService<DirGodotPropertyInspector>());
                    s.AddTransient<IDirFrameworkTextEditWindow>(p => p.GetRequiredService<DirGodotTextableMemberWindowV2>());
                    s.AddTransient<IDirFrameworkBitmapEditWindow>(p => p.GetRequiredService<DirGodotPictureMemberEditorWindow>());
                    s.AddTransient<IDirFrameworkImportExportWindow>(p => p.GetRequiredService<DirGodotImportExportWindow>());



                })
                .AddPreBuildAction(p =>
                {
                    //p.WithAbstUIGodot();
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
                    //.Register<IAbstDialog<>, IAbstGodotDialog<BlingoCodeImporterPopup>>()
                    // .Register<DirCodeHighlichter,DirGodotCodeHighlighter>()
                    ;
                    new BlingoGodotDirectorRoot(p.GetRequiredService<BlingoPlayer>(), p, p.GetRequiredService<BlingoProjectSettings>());
                });
            return engineRegistration;
        }

    }
}

