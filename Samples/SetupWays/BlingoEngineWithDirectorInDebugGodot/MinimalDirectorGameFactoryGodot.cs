using System;
using System.Threading.Tasks;
using AbstUI.Primitives;
using AbstUI.Texts;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Projects;
using BlingoEngine.Setup;
using BlingoEngine.Texts;

namespace BlingoEngineWithDirectorInDebugGodot;

public sealed class MinimalDirectorGameFactoryGodot : IBlingoProjectFactory
{
    public void Setup(IBlingoEngineRegistration engineRegistration)
    {
        engineRegistration.WithProjectSettings(settings =>
        {
            settings.ProjectName = MinimalDirectorGame.ProjectName;
            settings.ProjectFolder = ".";
            settings.CodeFolder = ".";
            settings.StageWidth = MinimalDirectorGame.StageWidth;
            settings.StageHeight = MinimalDirectorGame.StageHeight;
        });
    }

    public Task LoadCastLibsAsync(IBlingoCastLibsContainer castlibContainer, BlingoPlayer blingoPlayer)
    {
        var cast = castlibContainer.AddCast(MinimalDirectorGame.CastName);

        var textMember = cast.Add<BlingoMemberText>(1, MinimalDirectorGame.TextMemberName);
        textMember.Text = MinimalDirectorGame.SampleText;
        textMember.Color = AColors.White;
        textMember.FontSize = 20;
        textMember.WordWrap = true;
        textMember.Alignment = AbstTextAlignment.Center;
        textMember.Width = MinimalDirectorGame.StageWidth;
        textMember.Height = 80;
        textMember.RegPoint = new APoint(textMember.Width / 2f, textMember.Height / 2f);

        return Task.CompletedTask;
    }

    public Task<IBlingoMovie?> LoadStartupMovieAsync(IBlingoServiceProvider serviceProvider, BlingoPlayer blingoPlayer)
    {
        var movie = blingoPlayer.NewMovie(MinimalDirectorGame.MovieName);

        var textMember = blingoPlayer.CastLibs.GetMember<BlingoMemberText>(MinimalDirectorGame.TextMemberName)
            ?? throw new InvalidOperationException($"Text member '{MinimalDirectorGame.TextMemberName}' was not created.");

        movie.AddSprite(1, 1, movie.FrameCount, MinimalDirectorGame.StageWidth / 2f, MinimalDirectorGame.StageHeight / 2f, sprite =>
        {
            sprite.Name = MinimalDirectorGame.SpriteName;
            sprite.Member = textMember;
        });

        blingoPlayer.Stage.BackgroundColor = AColors.Black;

        return Task.FromResult<IBlingoMovie?>(movie);
    }

    public void Run(IBlingoMovie movie, bool autoPlayMovie)
    {
        if (autoPlayMovie)
        {
            movie.Play();
        }
    }
}

