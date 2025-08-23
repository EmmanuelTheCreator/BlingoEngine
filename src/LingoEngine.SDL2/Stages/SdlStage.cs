using System;
using AbstUI.Primitives;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.SDL2.Movies;
using LingoEngine.Stages;

namespace LingoEngine.SDL2.Stages;

public class SdlStage : ILingoFrameworkStage, IDisposable
{
    private readonly LingoClock _clock;
    private readonly LingoSdlRootContext _rootContext;
    private LingoStage _stage = null!;
    private readonly HashSet<SdlMovie> _movies = new();
    private SdlMovie? _activeMovie;
    public float Scale { get; set; }

    public SdlStage(LingoSdlRootContext rootContext, LingoClock clock)
    {
        _rootContext = rootContext;
        _clock = clock;

    }

    internal LingoSdlRootContext RootContext => _rootContext;

    public LingoStage LingoStage => _stage;

    internal void Init(LingoStage stage)
    {
        _stage = stage;

    }

    internal void ShowMovie(SdlMovie movie)
    {
        _movies.Add(movie);
    }
    internal void HideMovie(SdlMovie movie)
    {
        _movies.Remove(movie);
    }

    public void SetActiveMovie(LingoMovie? lingoMovie)
    {
        _activeMovie?.Hide();
        if (lingoMovie == null) { _activeMovie = null; return; }
        var movie = lingoMovie.Framework<SdlMovie>();
        _activeMovie = movie;
        movie.Show();
    }

    public void Dispose() { _movies.Clear(); }

    public void ApplyPropertyChanges()
    {

    }

    public IAbstTexture2D GetScreenshot()
        => throw new NotImplementedException();

    public void ShowTransition(IAbstTexture2D startTexture)
    {
    }

    public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
    {
    }

    public void HideTransition()
    {
    }
}
