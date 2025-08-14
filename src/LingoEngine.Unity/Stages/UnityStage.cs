using System;
using System.Collections.Generic;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Stages;
using LingoEngine.Unity.Movies;
using UnityEngine;

namespace LingoEngine.Unity.Stages;

/// <summary>
/// Unity implementation of <see cref="ILingoFrameworkStage"/>.
/// Wraps a <see cref="GameObject"/> and drives the engine clock
/// via Unity's update loop.
/// </summary>
public class UnityStage : MonoBehaviour, ILingoFrameworkStage, IDisposable
{
    private LingoStage _stage = null!;
    private LingoClock _clock = null!;
    private readonly HashSet<UnityMovie> _movies = new();
    private UnityMovie? _activeMovie;

    public LingoStage LingoStage => _stage;

    public float Scale
    {
        get => transform.localScale.x;
        set => transform.localScale = new Vector3(value, value, value);
    }

    internal void Configure(LingoClock clock) => _clock = clock;

    internal void Init(LingoStage stage)
    {
        _stage = stage;
    }

    private void Update()
    {
        _clock?.Tick(Time.deltaTime);
        _activeMovie?.UpdateStage();
    }

    internal void ShowMovie(UnityMovie movie) => _movies.Add(movie);

    internal void HideMovie(UnityMovie movie) => _movies.Remove(movie);

    public void SetActiveMovie(LingoMovie? lingoMovie)
    {
        _activeMovie?.Hide();
        if (lingoMovie == null)
        {
            _activeMovie = null;
            return;
        }
        var movie = lingoMovie.Framework<UnityMovie>();
        _activeMovie = movie;
        movie.Show();
    }

    public void ApplyPropertyChanges()
    {
    }

    public void Dispose()
    {
        foreach (var m in _movies)
            m.Dispose();
        _movies.Clear();
    }
}

