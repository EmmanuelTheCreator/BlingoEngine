using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AbstUI.Bitmaps;
using AbstUI.Primitives;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Stages;
using LingoEngine.Blazor.Movies;
using Microsoft.JSInterop;
using AbstUI.Blazor;
using AbstUI.Blazor.Bitmaps;

namespace LingoEngine.Blazor.Stages;

/// <summary>
/// Blazor stage implementation that drives the engine clock, renders
/// the active movie to an off-screen canvas and supports transition
/// overlays.
/// </summary>
public class LingoBlazorStage : ILingoFrameworkStage, IDisposable
{
    private readonly LingoClock _clock;
    private readonly IJSRuntime _js;
    private readonly AbstUIScriptResolver _scripts;
    private readonly HashSet<LingoBlazorMovie> _movies = new();
    private LingoBlazorMovie? _activeMovie;

    public event Action? Changed;
    private LingoStage _stage = null!;

    private readonly CancellationTokenSource _loopCts = new();
    private readonly Task _loopTask;
    private Action<IAbstTexture2D>? _nextShot;
    private bool _isTransitioning;
    private AbstBlazorTexture2D? _transitionStart;

    public LingoStage LingoStage => _stage;

    public float Scale { get; set; } = 1f;

    public LingoBlazorStage(LingoClock clock, IJSRuntime js, AbstUIScriptResolver scripts)
    {
        _clock = clock;
        _js = js;
        _scripts = scripts;
        _loopTask = Task.Run(RenderLoopAsync);
    }

    internal void Init(LingoStage stage)
    {
        _stage = stage;
    }

    internal void ShowMovie(LingoBlazorMovie movie)
    {
        _movies.Add(movie);
    }

    internal void HideMovie(LingoBlazorMovie movie) => _movies.Remove(movie);

    /// <inheritdoc />
    public void SetActiveMovie(LingoMovie? lingoMovie)
    {
        _activeMovie?.Hide();
        if (lingoMovie == null)
        {
            _activeMovie = null;
            Changed?.Invoke();
            return;
        }
        var movie = lingoMovie.Framework<LingoBlazorMovie>();
        _activeMovie = movie;
        movie.Show();
        Changed?.Invoke();
    }

    /// <inheritdoc />
    public void ApplyPropertyChanges() { }

    public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured)
    {
        _nextShot = onCaptured;
    }

    public IAbstTexture2D GetScreenshot()
    {
        return new NullTexture(_stage.Width, _stage.Height, $"StageShot_{_activeMovie?.CurrentFrame ?? 0}");
    }

    public void ShowTransition(IAbstTexture2D startTexture)
    {
        _transitionStart?.Dispose();
        _transitionStart = (AbstBlazorTexture2D)startTexture.Clone();
        _isTransitioning = true;
        // Transition drawing not implemented in DOM composition yet.
    }

    public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
    {
        if (!_isTransitioning)
            return;
    }

    public void HideTransition()
    {
        _isTransitioning = false;
        _transitionStart?.Dispose();
        _transitionStart = null;
        _activeMovie?.UpdateStage();
        Changed?.Invoke();
    }

    public void Dispose()
    {
        _loopCts.Cancel();
        try { _loopTask.Wait(); } catch { }
        foreach (var m in _movies)
            m.Dispose();
        _movies.Clear();
        _transitionStart?.Dispose();
    }

    private async Task RenderLoopAsync()
    {
        var sw = Stopwatch.StartNew();
        var last = sw.Elapsed;
        while (!_loopCts.IsCancellationRequested)
        {
            var now = sw.Elapsed;
            var delta = (float)(now - last).TotalSeconds;
            last = now;
            _clock.Tick(delta);
            if (!_isTransitioning)
                _activeMovie?.UpdateStage();
            if (_nextShot != null)
            {
                var shot = GetScreenshot();
                var cb = _nextShot;
                _nextShot = null;
                cb(shot);
            }
            try
            {
                await Task.Delay(16, _loopCts.Token);
            }
            catch (TaskCanceledException) { }
        }
    }

    private sealed class NullTexture : AbstBaseTexture2D<object>
    {
        private byte[] _pixels;

        public NullTexture(int width, int height, string name) : base(name)
        {
            Width = width;
            Height = height;
            _pixels = new byte[width * height * 4];
        }

        public override int Width { get; }
        public override int Height { get; }

        protected override void DisposeTexture() { }

        public override byte[] GetPixels() => _pixels;
        public override void SetARGBPixels(byte[] argbPixels) => _pixels = argbPixels;
        public override void SetRGBAPixels(byte[] rgbaPixels) => _pixels = rgbaPixels;

        public override IAbstTexture2D Clone()
        {
            var clone = new NullTexture(Width, Height, Name);
            clone._pixels = (byte[])_pixels.Clone();
            return clone;
        }
    }

    internal LingoBlazorMovie? ActiveMovie => _activeMovie;
}
