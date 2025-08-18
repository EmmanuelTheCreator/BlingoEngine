using System;
using System.Collections.Generic;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Stages;
using LingoEngine.Blazor.Movies;

namespace LingoEngine.Blazor.Stages;

/// <summary>
/// Minimal Blazor stage implementation. It tracks movies and delegates
/// activation to the engine without providing a concrete rendering surface.
/// </summary>
public class LingoBlazorStage : ILingoFrameworkStage, IDisposable
{
    private readonly LingoClock _clock;
    private readonly HashSet<LingoBlazorMovie> _movies = new();
    private LingoBlazorMovie? _activeMovie;
    private LingoStage _stage = null!;

    public LingoStage LingoStage => _stage;

    public float Scale { get; set; } = 1f;

    public LingoBlazorStage(LingoClock clock)
    {
        _clock = clock;
    }

    internal void Init(LingoStage stage)
    {
        _stage = stage;
    }

    internal void ShowMovie(LingoBlazorMovie movie)
    {
        _movies.Add(movie);
    }

    internal void HideMovie(LingoBlazorMovie movie)
    {
        _movies.Remove(movie);
    }

    /// <inheritdoc />
    public void SetActiveMovie(LingoMovie? lingoMovie)
    {
        _activeMovie?.Hide();
        if (lingoMovie == null)
        {
            _activeMovie = null;
            return;
        }
        var movie = lingoMovie.Framework<LingoBlazorMovie>();
        _activeMovie = movie;
        movie.Show();
    }

    /// <inheritdoc />
    public void ApplyPropertyChanges() { }

    public void Dispose()
    {
        foreach (var m in _movies)
            m.Dispose();
        _movies.Clear();
    }
}
