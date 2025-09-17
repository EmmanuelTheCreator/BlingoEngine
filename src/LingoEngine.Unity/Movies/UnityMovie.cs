using System;
using System.Collections.Generic;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using UnityEngine;
using LingoEngine.Unity.Stages;
using LingoEngine.Unity.Sprites;
using AbstUI.Primitives;
using AbstUI.Components;

namespace LingoEngine.Unity.Movies;

/// <summary>
/// Basic Unity movie container. Responsible for holding sprites and
/// forwarding updates to them.
/// </summary>
public class UnityMovie : ILingoFrameworkMovie, IDisposable
{
    private readonly UnityStage _stage;
    private readonly Action<UnityMovie> _remove;
    private readonly HashSet<LingoUnitySprite2D> _drawnSprites = new();
    private readonly HashSet<LingoUnitySprite2D> _allSprites = new();
    private readonly GameObject _root;
    private readonly LingoMovie _movie;
    private float _width;
    private float _height;
    private AMargin _margin = AMargin.Zero;
    private int _zIndex;

    public int CurrentFrame => _movie.CurrentFrame;
    public UnityMovie(UnityStage stage, LingoMovie movie, Action<UnityMovie> remove)
    {
        _stage = stage;
        _remove = remove;
        _root = new GameObject("MovieRoot");
        _root.transform.parent = stage.transform;
        stage.ShowMovie(this);
        _movie = movie;
        _width = movie.Width;
        _height = movie.Height;
    }

    internal void Show()
    {
        _root.SetActive(true);
        _stage.ShowMovie(this);
    }

    internal void Hide()
    {
        _root.SetActive(false);
        _stage.HideMovie(this);
    }

    public void UpdateStage()
    {
        foreach (var s in _drawnSprites)
            s.Update();
    }

    internal void CreateSprite<T>(T lingoSprite) where T : LingoSprite2D
    {
        var sprite = new LingoUnitySprite2D(lingoSprite, _root.transform,
            s => _drawnSprites.Add(s),
            s => _drawnSprites.Remove(s),
            s => { _drawnSprites.Remove(s); _allSprites.Remove(s); });
        _allSprites.Add(sprite);
    }

    public void RemoveMe()
    {
        _remove(this);
        GameObject.Destroy(_root);
    }

    APoint ILingoFrameworkMovie.GetGlobalMousePosition()
    {
        var pos = Input.mousePosition;
        return (pos.x, pos.y);
    }

    public void Dispose()
    {
        Hide();
        RemoveMe();
    }

    string IAbstFrameworkNode.Name
    {
        get => _root.name;
        set => _root.name = value;
    }

    bool IAbstFrameworkNode.Visibility
    {
        get => _root.activeSelf;
        set => _root.SetActive(value);
    }

    float IAbstFrameworkNode.Width
    {
        get => _width;
        set => _width = value;
    }

    float IAbstFrameworkNode.Height
    {
        get => _height;
        set => _height = value;
    }

    AMargin IAbstFrameworkNode.Margin
    {
        get => _margin;
        set => _margin = value;
    }

    int IAbstFrameworkNode.ZIndex
    {
        get => _zIndex;
        set => _zIndex = value;
    }

    object IAbstFrameworkNode.FrameworkNode => _root;
}

