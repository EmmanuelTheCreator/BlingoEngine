using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Setup;
using LingoEngine.Texts;
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
            .LoadCastLibFromCsv("InternalExt", Path.Combine("Media", "InternalExt", "Members.csv"), true)
            .LoadCastLibFromCsv("Data", Path.Combine("Media", "Data", "Members.csv"));
        InitMembers(lingoPlayer);
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
    public void InitMembers(LingoPlayer player)
    {
        var textColor = AColor.FromHex("#999966");
        var text = player.CastLib(2).GetMember<LingoMemberText>("T_data");
        text!.TextColor = textColor;
    }
    public void InitSprites()
    {
        if (_movie == null) return;


        //_movie.AddSprite(1, 1, 64, 519, 343).SetMember("bell0039")
        //    .AddBehavior<AnimationScriptBehavior>(b =>
        //    {
        //        b.myStartMembernum = 67;
        //        b.myEndMembernum = 108;
        //        b.mySlowDown = 2;
        //        b.myValue = -1;
        //        // My Sprite that contains info
        //        b.myDataSpriteNum = 1;
        //        // Name Info
        //        b.myDataName = "1";
        //        b.myWaitbeforeExecute = 0;
        //        //b.myFunction = 70;
        //    });
        //_movie.AddFrameBehavior<GameStopBehavior>(10);


        //_movie.AddSprite(1, 1, 64, 519, 343) //, c => c.InkType = Primitives.LingoInkType.BackgroundTransparent)
        //    .SetMember("B_Play")
        //    .AddBehavior<ButtonStartGameBehavior>();
        //_movie.AddFrameBehavior<GameStopBehavior>(10);
        //return;

        

        var MyBG = _movie.Member["Game"];
        _movie.AddFrameBehavior<GameStopBehavior>(60);
        //_movie.AddFrameBehavior<WaiterFrameScript>(1);
        //_movie.AddFrameBehavior<StayOnFrameFrameScript>(4);
        //_movie.AddFrameBehavior<MouseDownNavigateWithStayBehavior>(11, b => b.TickWait = 60);
        _movie.AddFrameBehavior<MouseDownNavigateWithStayBehavior>(2, b => { b.TickWait = 1; b.FrameOffsetOnClick = 40; });
        _movie.AddSprite(4, 54, 64, 336, 241).AddBehavior<BgScriptBehavior>().SetMember("Game");// BG GAme
        _movie.AddSprite(5, 56, 64, 591, 36, c => { c.Width = 193; c.Height = 35; }).SetMember("TetriGrounds_s"); // LOGO
        _movie.AddSprite(6, 59, 64, 503, 438).SetMember(7, 2); // copyright text
        var sprite = _movie.AddSprite(7, 60, 64, 441, 92).SetMember("T_data"); // level
        
        _movie.AddSprite(9, 60, 64, 519, 343).SetMember("B_Play").AddBehavior<ButtonStartGameBehavior>(); // Button play

        _movie.AddSprite(22, 55, 64, 463, 62).SetMember("bell0039") // Bell anim
            .AddBehavior<AnimationScriptBehavior>(b =>
            {
                b.myStartMembernum = 67;
                b.myEndMembernum = 108;
                b.mySlowDown = 2;
                b.myValue = -1;
                // My Sprite that contains info
                b.myDataSpriteNum = 1;
                // Name Info
                b.myDataName = "1";
                b.myWaitbeforeExecute = 0;
                //b.myFunction = 70;
            });
        _movie.AddSprite(28, 59, 64, 94, 297).SetMember(32,2); // Text personal
        _movie.AddSprite(29, 59, 64, 95, 313).SetMember("T_InternetScoresNamesP");
        _movie.AddSprite(30, 59, 64, 151, 313).SetMember("T_InternetScoresP");
        _movie.AddSprite(35, 61, 64, 323, 238).SetMember("alert");
    }
}
