using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Xtras.BuddyApi;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Members;
using LingoEngine.Casts;
using LingoEngine.Sprites;
using AbstUI.Commands;
using LingoEngine.Projects;
using LingoEngine.ColorPalettes;

namespace LingoEngine
{
    internal static class LingoEngineSetup
    {
        public static IServiceCollection WithGodotEngine(this IServiceCollection services)
        {

            services
                   .AddSingleton<LingoPlayer>()
                   .AddSingleton<LingoProjectSettings>()
                   .AddSingleton<LingoCastLibsContainer>()
                   .AddSingleton<LingoWindow>()
                   .AddSingleton<LingoClock>()
                   .AddSingleton<LingoSystem>()
                   .AddSingleton<LingoFrameLabelManager>()
                   .AddSingleton<ILingoColorPaletteDefinitions, LingoColorPaletteDefinitions>()
                   .AddTransient<ILingoPlayer>(p => p.GetRequiredService<LingoPlayer>())
                   .AddTransient<ILingoCastLibsContainer>(p => p.GetRequiredService<LingoCastLibsContainer>())
                   .AddTransient<ILingoWindow>(p => p.GetRequiredService<LingoWindow>())
                   .AddTransient<ILingoClock>(p => p.GetRequiredService<LingoClock>())
                   .AddTransient<ILingoSystem>(p => p.GetRequiredService<LingoSystem>())
                   .AddTransient<ILingoFrameLabelManager>(p => p.GetRequiredService<LingoFrameLabelManager>())
                   .AddTransient<ILingoMemberFactory, LingoMemberFactory>()
                   .AddTransient(p => new Lazy<ILingoMemberFactory>(() => p.GetRequiredService<ILingoMemberFactory>()))
                   .AddScoped<ILingoMovieEnvironment, LingoMovieEnvironment>()
                   .AddScoped<ILingoEventMediator, LingoEventMediator>()
                   // Xtras
                   .AddScoped<IBuddyAPI, BuddyAPI>()
                   .AddSingleton<IAbstCommandManager, AbstCommandManager>()
                   ;

            return services;
        }
    }
}
