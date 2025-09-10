using System;
using LingoEngine.Primitives;
using LingoEngine.Shapes;
using LingoEngine.Sprites;
using LingoEngine.Members;
using AbstUI.Primitives;
using AbstUI.LUnity.Bitmaps;
using UnityEngine;
using System.Linq;

namespace LingoEngine.Unity.Shapes;

/// <summary>
/// Unity implementation for shape members that renders primitives into a
/// <see cref="Texture2D"/> similar to the Godot version.
/// </summary>
public class UnityMemberShape : ILingoFrameworkMemberShape, IDisposable
{
    private LingoList<APoint> _vertexList = new();
    private LingoShapeType _shapeType = LingoShapeType.Rectangle;
    private AColor _fillColor = AColor.FromRGB(255, 255, 255);
    private int _strokeWidth = 1;
    private UnityTexture2D? _texture;

    internal void Init(LingoMemberShape member) { }

    /// <inheritdoc/>
    public bool IsLoaded { get; private set; }

    /// <inheritdoc/>
    public bool IsDirty { get; private set; } = true;

    /// <inheritdoc/>
    public LingoList<APoint> VertexList
    {
        get => _vertexList;
        set { _vertexList = value; IsDirty = true; IsLoaded = false; }
    }

    /// <inheritdoc/>
    public LingoShapeType ShapeType
    {
        get => _shapeType;
        set { _shapeType = value; IsDirty = true; IsLoaded = false; }
    }

    /// <inheritdoc/>
    public AColor FillColor
    {
        get => _fillColor;
        set { _fillColor = value; IsDirty = true; IsLoaded = false; }
    }

    /// <inheritdoc/>
    public AColor EndColor { get; set; } = AColor.FromRGB(255, 255, 255);

    /// <inheritdoc/>
    public AColor StrokeColor { get; set; } = AColor.FromRGB(0, 0, 0);

    /// <inheritdoc/>
    public int StrokeWidth
    {
        get => _strokeWidth;
        set => _strokeWidth = value;
    }

    /// <inheritdoc/>
    public bool Closed { get; set; } = true;

    /// <inheritdoc/>
    public bool AntiAlias { get; set; } = true;

    /// <inheritdoc/>
    public float Width { get; set; }

    /// <inheritdoc/>
    public float Height { get; set; }

    /// <inheritdoc/>
    public (int TL, int TR, int BR, int BL) CornerRadius { get; set; } = (5, 5, 5, 5);

    /// <inheritdoc/>
    public bool Filled { get; set; } = true;

    /// <inheritdoc/>
    public IAbstTexture2D? TextureLingo => _texture;

    /// <inheritdoc/>
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    /// <inheritdoc/>
    public void CopyToClipboard() { }

    /// <inheritdoc/>
    public void Erase() => VertexList.Clear();

    /// <inheritdoc/>
    public void ImportFileInto() { }

    /// <inheritdoc/>
    public void PasteClipboardInto() { }

    /// <inheritdoc/>
    public void Preload()
    {
        if (IsLoaded && !IsDirty) return;
        int w = Math.Max(1, (int)Width);
        int h = Math.Max(1, (int)Height);
        var pixels = new Color32[w * h];

        var fill = new Color32(FillColor.R, FillColor.G, FillColor.B, FillColor.A);
        var stroke = new Color32(StrokeColor.R, StrokeColor.G, StrokeColor.B, StrokeColor.A);

        switch (ShapeType)
        {
            case LingoShapeType.Rectangle:
                pixels.DrawRectangle(w, h, fill, stroke, Filled, StrokeWidth);
                break;
            case LingoShapeType.Oval:
                pixels.DrawOval(w, h, fill, stroke, Filled, StrokeWidth);
                break;
            case LingoShapeType.Line:
                if (VertexList.Count >= 2)
                {
                    var p0 = VertexList[0];
                    var p1 = VertexList[1];
                    pixels.DrawLine(w, h, (int)p0.X, (int)p0.Y, (int)p1.X, (int)p1.Y, stroke);
                }
                break;
            case LingoShapeType.PolyLine:
                if (VertexList.Count >= 2)
                    pixels.DrawPolyLine(w, h, VertexList.ToList(), Closed, stroke);
                break;
            case LingoShapeType.RoundRect:
                pixels.DrawRoundRect(w, h, fill, stroke, Filled, StrokeWidth);
                break;
        }

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels32(pixels);
        tex.Apply();
        _texture?.Dispose();
        _texture = new UnityTexture2D(tex, "Shape");
        IsLoaded = true;
        IsDirty = false;
    }

    /// <inheritdoc/>
    public void Unload()
    {
        _texture?.Dispose();
        _texture = null;
        IsLoaded = false;
        IsDirty = true;
    }

    /// <inheritdoc/>
    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
    {
        if (_texture == null || !IsLoaded || IsDirty)
            Preload();
        return _texture;
    }

    /// <inheritdoc/>
    public void Dispose() => Unload();

    public bool IsPixelTransparent(int x, int y) => false;
}

