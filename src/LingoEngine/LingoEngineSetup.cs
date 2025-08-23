using AbstUI.Commands;
using LingoEngine.Casts;
using LingoEngine.ColorPalettes;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Transitions;
using LingoEngine.Transitions.TransitionLibrary;
using LingoEngine.Xtras.BuddyApi;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine
{
    internal static class LingoEngineSetup
    {
        public static IServiceCollection WithLingoEngineBase(this IServiceCollection services)
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
                   .AddSingleton<ILingoTransitionLibrary, LingoTransitionLibrary>()
                   .AddTransient<ILingoTransitionPlayer, LingoTransitionPlayer>()

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

                   .AddTransient<LingoJoystickKeyboard>()
                   ;

            return services;
        }
    }
}
