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

namespace BlingoEngineMinimalSDL;

public sealed class MinimalGameFactorySDL : IBlingoProjectFactory
{
    public void Setup(IBlingoEngineRegistration engineRegistration)
    {
        engineRegistration.WithProjectSettings(settings =>
        {
            settings.ProjectName = MinimalGameSDL.ProjectName;
            settings.ProjectFolder = ".";
            settings.CodeFolder = ".";
            settings.StageWidth = MinimalGameSDL.StageWidth;
            settings.StageHeight = MinimalGameSDL.StageHeight;
        });
    }

    public Task LoadCastLibsAsync(IBlingoCastLibsContainer castlibContainer, BlingoPlayer blingoPlayer)
    {
        var cast = castlibContainer.AddCast(MinimalGameSDL.CastName);

        var textMember = cast.Add<BlingoMemberText>(1, MinimalGameSDL.TextMemberName);
        textMember.Text = MinimalGameSDL.SampleText;
        textMember.Color = AColors.White;
        textMember.FontSize = 20;
        textMember.WordWrap = true;
        textMember.Alignment = AbstTextAlignment.Center;
        textMember.Width = MinimalGameSDL.StageWidth;
        textMember.Height = 80;
        textMember.RegPoint = new APoint(textMember.Width / 2f, textMember.Height / 2f);

        return Task.CompletedTask;
    }

    public Task<IBlingoMovie?> LoadStartupMovieAsync(IBlingoServiceProvider serviceProvider, BlingoPlayer blingoPlayer)
    {
        var movie = blingoPlayer.NewMovie(MinimalGameSDL.MovieName);

        var textMember = blingoPlayer.CastLibs.GetMember<BlingoMemberText>(MinimalGameSDL.TextMemberName)
            ?? throw new InvalidOperationException($"Text member '{MinimalGameSDL.TextMemberName}' was not created.");

        movie.AddSprite(1, 1, movie.FrameCount, MinimalGameSDL.StageWidth / 2f, MinimalGameSDL.StageHeight / 2f, sprite =>
        {
            sprite.Name = MinimalGameSDL.SpriteName;
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

