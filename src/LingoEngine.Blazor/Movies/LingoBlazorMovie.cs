using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Blazor;
using AbstUI.Primitives;
using LingoEngine.Blazor.Sprites;
using LingoEngine.Blazor.Stages;
using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Blazor.Movies;

/// <summary>
/// Lightweight movie container for the Blazor backend. It keeps track of
/// sprites and exposes basic lifecycle hooks used by the engine. Instead of
/// rendering to a single canvas the movie now composes sprite canvases in the
/// DOM.
/// </summary>
public class LingoBlazorMovie : ILingoFrameworkMovie, IDisposable
{
    private readonly LingoBlazorStage _stage;
    private readonly LingoMovie _movie;
    private readonly Action<LingoBlazorMovie> _remove;
    private readonly HashSet<LingoBlazorSprite2D> _drawnSprites = new();
    private readonly HashSet<LingoBlazorSprite2D> _allSprites = new();
    private readonly AbstUIScriptResolver _scripts;
    private readonly int _width;
    private readonly int _height;
    private readonly LingoBlazorRootPanel _rootPanel;

    public event Action? Changed;

    public LingoBlazorMovie(LingoBlazorStage stage, LingoMovie movie, Action<LingoBlazorMovie> remove, AbstUIScriptResolver scripts, LingoBlazorRootPanel rootPanel)
    {
        _stage = stage;
        _movie = movie;
        _remove = remove;
        _scripts = scripts;
        _rootPanel = rootPanel;
        _width = stage.LingoStage.Width;
        _height = stage.LingoStage.Height;
    }

    internal void Show()
    {
        _stage.ShowMovie(this);
        Changed?.Invoke();
    }

    internal void Hide()
    {
        _stage.HideMovie(this);
        Changed?.Invoke();
    }

    public void UpdateStage()
    {
        foreach (var s in _drawnSprites)
        {
            s.Update();
        }
        Changed?.Invoke();
    }

    internal void CreateSprite<T>(T lingoSprite) where T : LingoSprite2D
    {
        var sprite = new LingoBlazorSprite2D(lingoSprite,
            s => { _drawnSprites.Add(s); Changed?.Invoke(); },
            s => { _drawnSprites.Remove(s); Changed?.Invoke(); },
            s => { _drawnSprites.Remove(s); _allSprites.Remove(s); Changed?.Invoke(); },
            _scripts);
        sprite.Changed += () => Changed?.Invoke();
        _allSprites.Add(sprite);
    }

    public void RemoveMe()
    {
        _remove(this);
    }

    public int CurrentFrame => _movie.CurrentFrame;

    internal IEnumerable<LingoBlazorSprite2D> VisibleSprites => _drawnSprites.OrderBy(s => s.ZIndex);

    public int WidthPx => _width;
    public int HeightPx => _height;

    public APoint GetGlobalMousePosition() => (0, 0);

    public void Dispose()
    {
        Hide();
        RemoveMe();
    }
}
