using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Demo.TetriGrounds.Core;

public class TetriGroundsProjectFactory : ILingoProjectFactory
{
    public const string MovieName = "Aaark Noid";
    private ILingoMovie? _movie;

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

    

    public void LoadCastLibs(ILingoCastLibsContainer castlibContainer, LingoPlayer lingoPlayer)
    {
        lingoPlayer
            .LoadCastLibFromCsv("Data", Path.Combine("Medias", "Data", "Members.csv"))
            .LoadCastLibFromCsv("InternalExt", Path.Combine("Medias", "InternalExt", "Members.csv"), true);
    }
    public ILingoMovie? LoadStartupMovie(IServiceProvider serviceProvider, LingoPlayer lingoPlayer)
    {
        _movie = LoadMovie(lingoPlayer);
        return _movie;
    }
    public void Run(ILingoMovie movie, bool autoPlayMovie)
    {
        if (autoPlayMovie)
            movie.Play();
    }

    public ILingoMovie LoadMovie(ILingoPlayer lingoPlayer)
    {
        _movie = lingoPlayer.NewMovie(MovieName);

        AddLabels();
        InitSprites();
        return _movie;
    }
    private void AddLabels()
    {
        if (_movie == null) return;
        _movie.SetScoreLabel(11, "Game");
    }
    public void InitSprites()
    {
        if (_movie == null) return;
        var MyBG = _movie.Member["Game"];
        //_movie.AddFrameBehavior<GameStopBehavior>(3);
        _movie.AddFrameBehavior<WaiterFrameScript>(1);
        _movie.AddFrameBehavior<StayOnFrameFrameScript>(4);
        //_movie.AddFrameBehavior<MouseDownNavigateWithStayBehavior>(11, b => b.TickWait = 60);
        _movie.AddSprite(1, 1, 15, 320, 240)
                //.AddBehavior<AppliBgBehavior>()
                .SetMember(MyBG);
        _movie.AddSprite(2, 1, 5, 320, 240).SetMember(25);
        //_movie.AddSprite(4, 3, 15, 320, 240).AddBehavior<BgScriptBehavior>().SetMember("Game");
        _movie.AddSprite(5, 1, 5, 320, 240).SetMember("TetriGrounds_s");
        _movie.AddSprite(7, 11, 15, 320, 240).SetMember("T_data");
        _movie.AddSprite(9, 11, 15, 320, 240).SetMember("B_Play");

    }
}
