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

namespace BlingoEngineMinimalGodot;

public sealed class MinimalGameFactoryGodot : IBlingoProjectFactory
{
    public void Setup(IBlingoEngineRegistration engineRegistration)
    {
        engineRegistration.WithProjectSettings(settings =>
        {
            settings.ProjectName = MinimalGame.ProjectName;
            settings.ProjectFolder = ".";
            settings.CodeFolder = ".";
            settings.StageWidth = MinimalGame.StageWidth;
            settings.StageHeight = MinimalGame.StageHeight;
        });
    }

    public Task LoadCastLibsAsync(IBlingoCastLibsContainer castlibContainer, BlingoPlayer blingoPlayer)
    {
        var cast = castlibContainer.AddCast(MinimalGame.CastName);

        var textMember = cast.Add<BlingoMemberText>(1, MinimalGame.TextMemberName);
        textMember.Text = MinimalGame.SampleText;
        textMember.Color = AColors.White;
        textMember.FontSize = 20;
        textMember.WordWrap = true;
        textMember.Alignment = AbstTextAlignment.Center;
        textMember.Width = MinimalGame.StageWidth;
        textMember.Height = 80;
        textMember.RegPoint = new APoint(textMember.Width / 2f, textMember.Height / 2f);

        return Task.CompletedTask;
    }

    public Task<IBlingoMovie?> LoadStartupMovieAsync(IBlingoServiceProvider serviceProvider, BlingoPlayer blingoPlayer)
    {
        var movie = blingoPlayer.NewMovie(MinimalGame.MovieName);

        var textMember = blingoPlayer.CastLibs.GetMember<BlingoMemberText>(MinimalGame.TextMemberName)
            ?? throw new InvalidOperationException($"Text member '{MinimalGame.TextMemberName}' was not created.");

        movie.AddSprite(1, 1, movie.FrameCount, MinimalGame.StageWidth / 2f, MinimalGame.StageHeight / 2f, sprite =>
        {
            sprite.Name = MinimalGame.SpriteName;
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

