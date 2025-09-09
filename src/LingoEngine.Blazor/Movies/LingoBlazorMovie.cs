using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Blazor;
using AbstUI.Blazor.Primitives;
using AbstUI.Blazor.Bitmaps;
using AbstUI.Primitives;
using LingoEngine.Blazor.Sprites;
using LingoEngine.Blazor.Stages;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace LingoEngine.Blazor.Movies;

/// <summary>
/// Lightweight movie container for the Blazor backend. It keeps track of
/// sprites and exposes basic lifecycle hooks used by the engine.
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
    private ElementReference _canvas;
    private IJSObjectReference? _ctx;

    public LingoBlazorMovie(LingoBlazorStage stage, LingoMovie movie, Action<LingoBlazorMovie> remove, AbstUIScriptResolver scripts, LingoBlazorRootPanel rootPanel)
    {
        _stage = stage;
        _movie = movie;
        _remove = remove;
        _scripts = scripts;
        _rootPanel = rootPanel;
        _width = stage.LingoStage.Width;
        _height = stage.LingoStage.Height;
        _canvas = _scripts.CanvasCreateCanvas(_width, _height).GetAwaiter().GetResult();
        _scripts.CanvasAddToElement(_rootPanel.Root, _canvas).GetAwaiter().GetResult();
        _ctx = _scripts.CanvasGetContext(_canvas, true).GetAwaiter().GetResult();
        _scripts.CanvasSetVisible(_canvas, false).GetAwaiter().GetResult();
    }

    internal void Show()
    {
        _stage.ShowMovie(this);
        _scripts.CanvasSetVisible(_canvas, true).GetAwaiter().GetResult();
    }

    internal void Hide()
    {
        _scripts.CanvasSetVisible(_canvas, false).GetAwaiter().GetResult();
        _stage.HideMovie(this);
    }

    public void UpdateStage()
    {
        if (_ctx == null) return;
        _scripts.CanvasClear(_ctx, _stage.LingoStage.BackgroundColor.ToCss(), _width, _height).GetAwaiter().GetResult();
        foreach (var s in _drawnSprites.OrderBy(s => s.ZIndex))
        {
            if (s.Texture is not AbstBlazorTexture2D tex) continue;
            var drawW = s.DesiredWidth > 0 ? s.DesiredWidth : s.Width;
            var drawH = s.DesiredHeight > 0 ? s.DesiredHeight : s.Height;
            var x = s.X - s.RegPoint.X;
            var y = s.Y - s.RegPoint.Y;
            _ctx.InvokeVoidAsync("drawImage", tex.Canvas, x, y, drawW, drawH).GetAwaiter().GetResult();
            s.Update();
        }
    }

    internal void CreateSprite<T>(T lingoSprite) where T : LingoSprite2D
    {
        var sprite = new LingoBlazorSprite2D(lingoSprite,
            s => _drawnSprites.Add(s),
            s => _drawnSprites.Remove(s),
            s => { _drawnSprites.Remove(s); _allSprites.Remove(s); });
        _allSprites.Add(sprite);
    }

    public void RemoveMe()
    {
        _remove(this);
    }

    public int CurrentFrame => _movie.CurrentFrame;

    internal IJSObjectReference? Context => _ctx;

    public APoint GetGlobalMousePosition() => (0, 0);

    public void Dispose()
    {
        Hide();
        _scripts.CanvasDisposeCanvas(_canvas).GetAwaiter().GetResult();
        RemoveMe();
    }
}
