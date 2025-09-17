using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Blazor;
using AbstUI.Blazor.Components.Containers;
using AbstUI.Primitives;
using BlingoEngine.Blazor.Sprites;
using BlingoEngine.Blazor.Stages;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using AbstUI.Components;
using AbstUI.Components.Containers;

namespace BlingoEngine.Blazor.Movies;

/// <summary>
/// Lightweight movie container for the Blazor backend. It keeps track of
/// sprites and exposes basic lifecycle hooks used by the engine. Instead of
/// rendering to a single canvas the movie now composes sprite canvases in the
/// DOM.
/// </summary>
public class BlingoBlazorMovie : IBlingoFrameworkMovie, IDisposable
{
    private readonly BlingoBlazorStage _stage;
    private readonly BlingoMovie _movie;
    private readonly Action<BlingoBlazorMovie> _remove;
    private readonly HashSet<BlingoBlazorSprite2D> _drawnSprites = new();
    private readonly HashSet<BlingoBlazorSprite2D> _allSprites = new();
    private readonly AbstUIScriptResolver _scripts;
    private readonly AbstBlazorPanelComponent _panel;
    private float _layoutWidth;
    private float _layoutHeight;
    private AMargin _margin = AMargin.Zero;
    private int _zIndex;

    public event Action? Changed;

    public BlingoBlazorMovie(BlingoBlazorStage stage, BlingoMovie movie, Action<BlingoBlazorMovie> remove, AbstUIScriptResolver scripts, BlingoBlazorRootPanel rootPanel, IAbstComponentFactory factory)
    {
        _stage = stage;
        _movie = movie;
        _remove = remove;
        _scripts = scripts;
        _ = rootPanel;
        float stageWidth = stage.BlingoStage.Width;
        float stageHeight = stage.BlingoStage.Height;
        _panel = factory.CreatePanel($"Movie_{movie.Name}").Framework<AbstBlazorPanelComponent>();
        _panel.Width = stageWidth;
        _panel.Height = stageHeight;
        _panel.Name = movie.Name;
        _panel.Visibility = true;
        _panel.Changed += OnPanelChanged;
        _layoutWidth = stageWidth;
        _layoutHeight = stageHeight;
    }

    internal void Show()
    {
        _panel.Visibility = true;
        _stage.ShowMovie(this);
        Changed?.Invoke();
    }

    internal void Hide()
    {
        _panel.Visibility = false;
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

    internal void CreateSprite<T>(T blingoSprite) where T : BlingoSprite2D
    {
        var sprite = new BlingoBlazorSprite2D(blingoSprite,
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

    internal IEnumerable<BlingoBlazorSprite2D> VisibleSprites => _drawnSprites.OrderBy(s => s.ZIndex);

    public int WidthPx => (int)_layoutWidth;
    public int HeightPx => (int)_layoutHeight;

    public APoint GetGlobalMousePosition() => (0, 0);

    public void Dispose()
    {
        Hide();
        RemoveMe();
        _panel.Changed -= OnPanelChanged;
        _panel.Dispose();
    }

    internal IAbstFrameworkPanel Panel => _panel;

    internal void UpdateDimensions(float width, float height)
    {
        if (Math.Abs(_layoutWidth - width) <= float.Epsilon && Math.Abs(_layoutHeight - height) <= float.Epsilon)
            return;
        _layoutWidth = width;
        _layoutHeight = height;
        _panel.Width = width;
        _panel.Height = height;
        Changed?.Invoke();
    }

    private void OnPanelChanged() => Changed?.Invoke();

    string IAbstFrameworkNode.Name
    {
        get => _panel.Name;
        set
        {
            if (_panel.Name == value)
                return;
            _panel.Name = value;
            Changed?.Invoke();
        }
    }

    bool IAbstFrameworkNode.Visibility
    {
        get => _panel.Visibility;
        set
        {
            if (_panel.Visibility == value)
                return;
            _panel.Visibility = value;
            Changed?.Invoke();
        }
    }

    float IAbstFrameworkNode.Width
    {
        get => _layoutWidth;
        set => UpdateDimensions(value, _layoutHeight);
    }

    float IAbstFrameworkNode.Height
    {
        get => _layoutHeight;
        set => UpdateDimensions(_layoutWidth, value);
    }

    AMargin IAbstFrameworkNode.Margin
    {
        get => _margin;
        set
        {
            if (_margin.Equals(value))
                return;
            _margin = value;
            _panel.Margin = value;
            Changed?.Invoke();
        }
    }

    int IAbstFrameworkNode.ZIndex
    {
        get => _zIndex;
        set
        {
            if (_zIndex == value)
                return;
            _zIndex = value;
            _panel.ZIndex = value;
            Changed?.Invoke();
        }
    }

    object IAbstFrameworkNode.FrameworkNode => _panel;
}

