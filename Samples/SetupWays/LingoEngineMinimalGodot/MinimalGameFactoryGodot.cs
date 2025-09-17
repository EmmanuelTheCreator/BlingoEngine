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

namespace LingoEngineMinimalGodot;

public sealed class MinimalGameFactoryGodot : ILingoProjectFactory
{
    public void Setup(ILingoEngineRegistration engineRegistration)
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

    public Task LoadCastLibsAsync(ILingoCastLibsContainer castlibContainer, LingoPlayer lingoPlayer)
    {
        var cast = castlibContainer.AddCast(MinimalGame.CastName);

        var textMember = cast.Add<LingoMemberText>(1, MinimalGame.TextMemberName);
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

    public Task<ILingoMovie?> LoadStartupMovieAsync(ILingoServiceProvider serviceProvider, LingoPlayer lingoPlayer)
    {
        var movie = lingoPlayer.NewMovie(MinimalGame.MovieName);

        var textMember = lingoPlayer.CastLibs.GetMember<LingoMemberText>(MinimalGame.TextMemberName)
            ?? throw new InvalidOperationException($"Text member '{MinimalGame.TextMemberName}' was not created.");

        movie.AddSprite(1, 1, movie.FrameCount, MinimalGame.StageWidth / 2f, MinimalGame.StageHeight / 2f, sprite =>
        {
            sprite.Name = MinimalGame.SpriteName;
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
