using LingoEngine.SDL2;
using LingoEngine.Core;
using LingoEngine.Director.Core;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Stages;
using AbstUI.SDL2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LingoEngine.Director.SDL2.Casts;
using LingoEngine.Director.SDL2.Icons;
using LingoEngine.Director.SDL2.Inspector;
using LingoEngine.Director.SDL2.UI;
using LingoEngine.Director.SDL2.Stages;
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
               .ServicesMain(s =>
               {
                   s.AddSingleton<DirSdlCastWindow>();
                   s.AddTransient<IDirFrameworkCastWindow>(p => p.GetRequiredService<DirSdlCastWindow>());
                   s.AddSingleton<DirSdlStageWindow>();
                   s.AddTransient<IDirFrameworkStageWindow>(p => p.GetRequiredService<DirSdlStageWindow>());
                   s.AddSingleton<DirSdlPropertyInspectorWindow>();
                   s.AddTransient<IDirFrameworkPropertyInspectorWindow>(p => p.GetRequiredService<DirSdlPropertyInspectorWindow>());
                   s.AddSingleton<IDirectorIconManager>(p =>
                   {
                       var mgr = new DirSdlIconManager(p.GetRequiredService<ILogger<DirSdlIconManager>>(), p.GetRequiredService<SdlRootContext>());
                       mgr.LoadSheet("Media/Icons/General_Icons.png", 20, 16, 16, 8);
                       mgr.LoadSheet("Media/Icons/Painter_Icons.png", 20, 16, 16, 8);
                       mgr.LoadSheet("Media/Icons/Painter2_Icons.png", 20, 16, 16, 8);
                       mgr.LoadSheet("Media/Icons/Director_Icons.png", 20, 16, 16, 8);
                       return mgr;
                   });
               })
               .AddBuildAction(p =>
               {
                   p.WithAbstUISdl();
                   new LingoSdlDirectorRoot(p.GetRequiredService<LingoPlayer>(), p, p.GetRequiredService<LingoProjectSettings>());
               });
            return reg;
        }
    }
}
