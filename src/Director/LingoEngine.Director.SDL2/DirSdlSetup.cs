using LingoEngine.SDL2;
using LingoEngine.Core;
using LingoEngine.Director.Core;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Behaviors;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Stages;
using AbstUI.SDL2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LingoEngine.Director.SDL2.Casts;
using LingoEngine.Director.SDL2.Behaviors;
using LingoEngine.Director.SDL2.Icons;
using LingoEngine.Director.SDL2.Inspector;
using LingoEngine.Director.SDL2.UI;
using LingoEngine.Director.SDL2.Stages;
using LingoEngine.Director.SDL2.Scores;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Projects;
using LingoEngine.Setup;

namespace LingoEngine.Director.SDL2
{
    public static class DirSdlSetup
    {
        public static ILingoEngineRegistration WithDirectorSdlEngine(this ILingoEngineRegistration reg, string windowTitle, int width, int height, Action<DirectorProjectSettings>? directorSettings = null)
        {
            LingoEngineGlobal.IsRunningDirector = true;
            reg.WithLingoSdlEngine(windowTitle, width, height)
               .WithDirectorEngine(directorSettings)
               .RegisterComponents(c =>
                    c
                        .AddSingleton<DirectorCastWindow, DirSdlCastWindow>()
                        .AddSingleton<DirectorStageWindow, DirSdlStageWindow>()
                        .AddSingleton<DirectorScoreWindow, DirSdlScoreWindow>()
                        .AddSingleton<DirectorPropertyInspectorWindow, DirSdlPropertyInspectorWindow>()
                        .AddSingleton<DirBehaviorInspectorWindow, DirSdlBehaviorInspectorWindow>()
                    )
               .ServicesMain(s =>
               {
                   s
                        .AddTransient<IDirFrameworkCastWindow>(p => p.GetRequiredService<DirSdlCastWindow>())
                        .AddTransient<IDirFrameworkStageWindow>(p => p.GetRequiredService<DirSdlStageWindow>())
                        .AddTransient<IDirFrameworkScoreWindow>(p => p.GetRequiredService<DirSdlScoreWindow>())
                        .AddTransient<IDirFrameworkPropertyInspectorWindow>(p => p.GetRequiredService<DirSdlPropertyInspectorWindow>())
                        .AddTransient<IDirFrameworkBehaviorInspectorWindow>(p => p.GetRequiredService<DirSdlBehaviorInspectorWindow>())
                    ;

                   s.AddSingleton<IDirectorIconManager>(p =>
                   {
                       var mgr = new DirSdlIconManager(p.GetRequiredService<ILogger<DirSdlIconManager>>(), p.GetRequiredService<LingoSdlRootContext>());
                       mgr.LoadSheet("Media/Icons/General_Icons.png", 20, 16, 16, 8);
                       mgr.LoadSheet("Media/Icons/Painter_Icons.png", 20, 16, 16, 8);
                       mgr.LoadSheet("Media/Icons/Painter2_Icons.png", 20, 16, 16, 8);
                       mgr.LoadSheet("Media/Icons/Director_Icons.png", 20, 16, 16, 8);
                       return mgr;
                   });
               })
               .AddBuildAction(p =>
               {
                   new LingoSdlDirectorRoot(p.GetRequiredService<LingoPlayer>(), p, p.GetRequiredService<LingoProjectSettings>());
               });
            return reg;
        }
    }
}
