using System;
using System.IO;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Projects;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Demo.TetriGrounds.Core;

public class TetriGroundsProjectFactory : ILingoProjectFactory
{
    public void Setup(ILingoEngineRegistration config)
    {
        config
            //.AddFont("Arcade", Path.Combine("Media", "Fonts", "arcade.ttf"))
            //.AddFont("Bikly", Path.Combine("Media", "Fonts", "bikly.ttf"))
            //.AddFont("8Pin Matrix", Path.Combine("Media", "Fonts", "8PinMatrix.ttf"))
            //.AddFont("Earth", Path.Combine("Media", "Fonts", "earth.ttf"))
            //.AddFont("Tahoma", Path.Combine("Media", "Fonts", "Tahoma.ttf"))
            .WithProjectSettings(s =>
                {
                    s.ProjectFolder = "TetriGrounds";
                    s.ProjectName = "TetriGrounds";
                    s.MaxSpriteChannelCount = 300;
                })
                .ForMovie(TetriGroundsGame.MovieName, s => s
                    .AddScriptsFromAssembly()

                    // As an example, you can add them manually too:

                    // .AddMovieScript<StartMoviesScript>() => MovieScript
                    // .AddBehavior<MouseDownNavigateBehavior>()  -> Behavior
                    // .AddParentScript<BlockParentScript>() -> Parent script
                )
                .ServicesLingo(s => s
                    .AddSingleton<IArkCore, TetriGroundsCore>()
                    .AddSingleton<TetriGroundsGame, TetriGroundsGame>()
                    .AddSingleton<GlobalVars>()
                );
    }

    public void Run(IServiceProvider serviceProvider, ILingoPlayer player, bool autoPlayMovie)
    {
        var game = serviceProvider.GetRequiredService<TetriGroundsGame>();
        game.LoadMovie();
        if (autoPlayMovie)
            game.Play();
    }

    public void LoadCastLibs(ILingoCastLibsContainer castlibContainer, LingoPlayer lingoPlayer)
    {
        lingoPlayer
            .LoadCastLibFromCsv("Data", Path.Combine("Medias", "Data", "Members.csv"))
            .LoadCastLibFromCsv("InternalExt", Path.Combine("Medias", "InternalExt", "Members.csv"), true);
    }
}
