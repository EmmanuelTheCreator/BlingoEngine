using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Demo.TetriGrounds.Core;

public class TetriGroundsProjectFactory : ILingoProjectFactory
{
    public const string MovieName = "Aaark Noid";
    private LingoProjectSettings _settings;
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
                    s.StageWidth = 730;
                    s.StageHeight = 547;
                })
                .ForMovie(MovieName, s => s
                    .AddScriptsFromAssembly()

                // As an example, you can add them manually too:

                // .AddMovieScript<StartMoviesScript>() => MovieScript
                // .AddBehavior<MouseDownNavigateBehavior>()  -> Behavior
                // .AddParentScript<BlockParentScript>() -> Parent script
                )
                .ServicesLingo(s => s
                    .AddSingleton<IArkCore, TetriGroundsCore>()
                    .AddSingleton<GlobalVars>()
                );
    }



    public void LoadCastLibs(ILingoCastLibsContainer castlibContainer, LingoPlayer lingoPlayer)
    {
        lingoPlayer
            .LoadCastLibFromCsv("Data", Path.Combine("Media", "Data", "Members.csv"))
            .LoadCastLibFromCsv("InternalExt", Path.Combine("Media", "InternalExt", "Members.csv"), true);
    }
    public ILingoMovie? LoadStartupMovie(ILingoServiceProvider serviceProvider, LingoPlayer lingoPlayer)
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
        _movie.SetScoreLabel(2, "Intro");
        _movie.SetScoreLabel(60, "Game");
        _movie.SetScoreLabel(75, "FilmLoop Test");
    }
    public void InitSprites()
    {
        if (_movie == null) return;

        _movie.AddSprite(1, 1, 64, 519, 343) //, c => c.InkType = Primitives.LingoInkType.BackgroundTransparent)
            .SetMember("B_Play")
            .AddBehavior<ButtonStartGameBehavior>();
        _movie.AddFrameBehavior<GameStopBehavior>(10);
        return;

        var MyBG = _movie.Member["Game"];
        _movie.AddFrameBehavior<GameStopBehavior>(60);
        //_movie.AddFrameBehavior<WaiterFrameScript>(1);
        //_movie.AddFrameBehavior<StayOnFrameFrameScript>(4);
        //_movie.AddFrameBehavior<MouseDownNavigateWithStayBehavior>(11, b => b.TickWait = 60);
        _movie.AddFrameBehavior<MouseDownNavigateWithStayBehavior>(2, b => { b.TickWait = 1; b.FrameOffsetOnClick = 40; });
        _movie.AddSprite(4, 54, 64, 336, 241).AddBehavior<BgScriptBehavior>().SetMember("Game");// BG GAme
        _movie.AddSprite(5, 56, 64, 591, 36,c => { c.Width = 193; c.Height = 35; }).SetMember("TetriGrounds_s"); // LOGO
        _movie.AddSprite(6, 59, 64, 503, 438).SetMember(9,2); // copyright text
        _movie.AddSprite(7, 60, 64, 441, 92).SetMember("T_data");
        _movie.AddSprite(9, 60, 64, 519, 343, c => c.InkType = Primitives.LingoInkType.BackgroundTransparent)
            .SetMember("B_Play")
            .AddBehavior<ButtonStartGameBehavior>();

    }
}
