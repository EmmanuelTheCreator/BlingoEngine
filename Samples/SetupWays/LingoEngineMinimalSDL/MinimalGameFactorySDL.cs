using System;
using System.Threading.Tasks;
using AbstUI.Primitives;
using AbstUI.Texts;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Setup;
using LingoEngine.Texts;

namespace LingoEngineMinimalSDL;

public sealed class MinimalGameFactorySDL : ILingoProjectFactory
{
    public void Setup(ILingoEngineRegistration engineRegistration)
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

    public Task LoadCastLibsAsync(ILingoCastLibsContainer castlibContainer, LingoPlayer lingoPlayer)
    {
        var cast = castlibContainer.AddCast(MinimalGameSDL.CastName);

        var textMember = cast.Add<LingoMemberText>(1, MinimalGameSDL.TextMemberName);
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

    public Task<ILingoMovie?> LoadStartupMovieAsync(ILingoServiceProvider serviceProvider, LingoPlayer lingoPlayer)
    {
        var movie = lingoPlayer.NewMovie(MinimalGameSDL.MovieName);

        var textMember = lingoPlayer.CastLibs.GetMember<LingoMemberText>(MinimalGameSDL.TextMemberName)
            ?? throw new InvalidOperationException($"Text member '{MinimalGameSDL.TextMemberName}' was not created.");

        movie.AddSprite(1, 1, movie.FrameCount, MinimalGameSDL.StageWidth / 2f, MinimalGameSDL.StageHeight / 2f, sprite =>
        {
            sprite.Name = MinimalGameSDL.SpriteName;
            sprite.Member = textMember;
        });

        lingoPlayer.Stage.BackgroundColor = AColors.Black;

        return Task.FromResult<ILingoMovie?>(movie);
    }

    public void Run(ILingoMovie movie, bool autoPlayMovie)
    {
        if (autoPlayMovie)
        {
            movie.Play();
        }
    }
}
