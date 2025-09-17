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

namespace LingoEngineWithDirectorInDebugGodot;

public sealed class MinimalDirectorGameFactoryGodot : ILingoProjectFactory
{
    public void Setup(ILingoEngineRegistration engineRegistration)
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

    public Task LoadCastLibsAsync(ILingoCastLibsContainer castlibContainer, LingoPlayer lingoPlayer)
    {
        var cast = castlibContainer.AddCast(MinimalDirectorGame.CastName);

        var textMember = cast.Add<LingoMemberText>(1, MinimalDirectorGame.TextMemberName);
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

    public Task<ILingoMovie?> LoadStartupMovieAsync(ILingoServiceProvider serviceProvider, LingoPlayer lingoPlayer)
    {
        var movie = lingoPlayer.NewMovie(MinimalDirectorGame.MovieName);

        var textMember = lingoPlayer.CastLibs.GetMember<LingoMemberText>(MinimalDirectorGame.TextMemberName)
            ?? throw new InvalidOperationException($"Text member '{MinimalDirectorGame.TextMemberName}' was not created.");

        movie.AddSprite(1, 1, movie.FrameCount, MinimalDirectorGame.StageWidth / 2f, MinimalDirectorGame.StageHeight / 2f, sprite =>
        {
            sprite.Name = MinimalDirectorGame.SpriteName;
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
