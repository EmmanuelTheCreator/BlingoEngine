using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Setup;
using LingoEngine.Sounds;
using LingoEngine.Texts;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.Mime.MediaTypeNames;

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
        lingoPlayer.AddCastLib("Sounds",true,c =>
        {
            c.Add(LingoMemberType.Sound, 0, "S_Click", Path.Combine("Media", "Sounds", "click.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_BtnStart", Path.Combine("Media", "Sounds", "btnstart.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_Gong", Path.Combine("Media", "Sounds", "gong.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_DeleteRow", Path.Combine("Media", "Sounds", "deleterow.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_GO", Path.Combine("Media", "Sounds", "go.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_LevelUp", Path.Combine("Media", "Sounds", "level_up.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_Shhh1", Path.Combine("Media", "Sounds", "shhh1.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_Terminated", Path.Combine("Media", "Sounds", "terminated.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_Died", Path.Combine("Media", "Sounds", "die.mp3"));

            c.Add(LingoMemberType.Sound, 0, "S_BlockFall1", Path.Combine("Media", "Sounds", "blockfall_1.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_BlockFall2", Path.Combine("Media", "Sounds", "blockfall_2.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_BlockFall3", Path.Combine("Media", "Sounds", "blockfall_3.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_BlockFall4", Path.Combine("Media", "Sounds", "blockfall_4.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_BlockFall5", Path.Combine("Media", "Sounds", "blockfall_5.mp3"));

            c.Add(LingoMemberType.Sound, 0, "S_RowsDeleted_2", Path.Combine("Media", "Sounds", "rows_deleted_2.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_RowsDeleted_3", Path.Combine("Media", "Sounds", "rows_deleted_3.mp3"));
            c.Add(LingoMemberType.Sound, 0, "S_RowsDeleted_4", Path.Combine("Media", "Sounds", "rows_deleted_4.mp3"));
        });
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
        text!.Color = textColor;
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
        //TestPuppetSprite();
        //return;

        var castData = _movie.CastLib["Data"];

        castData.Member["T_data"]!.Width = 191;
        castData.Member["T_NewGame"]!.Width = 48;
        castData.Member["T_Score"]!.Width = 99;
        castData.Member["T_OverScreen"]!.Width = 366;
        castData.Member["T_OverScreen2"]!.Width = 366;
        castData.Member["T_InternetScoresNames"]!.Width = 65;
        castData.Member["T_InternetScores"]!.Width = 37;
        castData.Member["T_InternetScoresNamesP"]!.Width = 69;
        castData.Member["T_InternetScoresP"]!.Width = 37;

        var MyBG = _movie.Member["Game"];
        _movie.AddFrameBehavior<GameStopBehavior>(60);
        //_movie.AddFrameBehavior<WaiterFrameScript>(1);
        //_movie.AddFrameBehavior<StayOnFrameFrameScript>(4);
        //_movie.AddFrameBehavior<MouseDownNavigateWithStayBehavior>(11, b => b.TickWait = 60);
        _movie.AddFrameBehavior<MouseDownNavigateWithStayBehavior>(2, b => { b.TickWait = 1; b.FrameOffsetOnClick = 40; });
        
        _movie.AddSprite(4, 54, 64, 336, 241).AddBehavior<BgScriptBehavior>().SetMember("Game");// BG GAme
        _movie.AddSprite(5, 56, 64, 591, 36, c => { c.Width = 193; c.Height = 35; }).SetMember("TetriGrounds_s"); // LOGO
        _movie.AddSprite(6, 59, 64, 503, 438).SetMember(9, 2); // copyright text
        var sprite = _movie.AddSprite(7, 60, 64, 441, 92).SetMember("T_data"); // level
        
        // Button play
        _movie.AddSprite(9, 60, 64, 519, 343).SetMember("B_Play").AddBehavior<ButtonStartGameBehavior>(); // Button play
        var memberT_NewGame = _movie.Member["T_NewGame"];
        _movie.AddSprite(11, 60, 64, 497, 334).SetMember(memberT_NewGame); // Text New Game on Button

        var memberScore = _movie.Member["T_Score"];
        _movie.AddSprite(12, 60, 64, 486, 148).SetMember(memberScore); 


        _movie.AddSprite(22, 55, 64, 463, 62).SetMember("bell0039") // Bell anim
            .AddBehavior<AnimationScriptBehavior>(b =>
            {
                b.myStartMembernum = 100;
                b.myEndMembernum = 140;
                b.myValue =  100;
                b.mySlowDown = 2; 
                // My Sprite that contains info
                b.myDataSpriteNum = 1;
                // Name Info
                b.myDataName = "1";
                b.myWaitbeforeExecute = 0;
                //b.myFunction = 70;
            });
       
        _movie.AddSprite(24, 59, 64, 94, 129).SetMember("T_InternetScoresNames"); 
        _movie.AddSprite(25, 59, 64, 159, 132).SetMember("T_InternetScores"); 
        _movie.AddSprite(26, 59, 64, 91, 50).SetMember(39,2); // Text highscores
        _movie.AddSprite(27, 59, 64, 91, 50).SetMember(39,2); // Text All
        _movie.AddSprite(28, 59, 64, 94, 113).SetMember(41,2); // Text personal
        _movie.AddSprite(29, 59, 64, 95, 313).SetMember("T_InternetScoresNamesP");
        _movie.AddSprite(30, 59, 64, 151, 313).SetMember("T_InternetScoresP");
        _movie.AddSprite(35, 61, 64, 323, 238).SetMember("alert");
    }

    private void TestPuppetSprite()
    {
        _movie!.AddSprite(5, 2, 64, 519, 343).SetMember("B_Play").AddBehavior<ButtonStartGameBehavior>(); // Button play
        _movie.AddFrameBehavior<StayOnFrameFrameScript>(10);
    }
}
