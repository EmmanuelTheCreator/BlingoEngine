using System;
using System.Collections.Generic;
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
/// Minimal Blazor stage implementation. It tracks movies and delegates
/// activation to the engine without providing a concrete rendering surface.
/// </summary>
public class LingoBlazorStage : ILingoFrameworkStage, IDisposable
{
    private readonly LingoClock _clock;
    private readonly IJSRuntime _js;
    private readonly AbstUIScriptResolver _scripts;
    private readonly HashSet<LingoBlazorMovie> _movies = new();
    private LingoBlazorMovie? _activeMovie;
    private LingoStage _stage = null!;

    public LingoStage LingoStage => _stage;

    public float Scale { get; set; } = 1f;

    public LingoBlazorStage(LingoClock clock, IJSRuntime js, AbstUIScriptResolver scripts)
    {
        _clock = clock;
        _js = js;
        _scripts = scripts;
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

    public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured)
    {
        onCaptured(GetScreenshot());
    }

    public IAbstTexture2D GetScreenshot()
    {
        if (_activeMovie?.Context is not IJSObjectReference ctx)
            return new NullTexture(_stage.Width, _stage.Height, $"StageShot_{_activeMovie?.CurrentFrame ?? 0}");

        var data = _scripts.CanvasGetImageData(ctx, _stage.Width, _stage.Height).GetAwaiter().GetResult();
        return AbstBlazorTexture2D
            .CreateFromPixelDataAsync(_js, _scripts, data, _stage.Width, _stage.Height,
                $"StageShot_{_activeMovie.CurrentFrame}")
            .GetAwaiter().GetResult();
    }

    public void ShowTransition(IAbstTexture2D startTexture) { }

    public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect) { }

    public void HideTransition() { }

    public void Dispose()
    {
        foreach (var m in _movies)
            m.Dispose();
        _movies.Clear();
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
}
