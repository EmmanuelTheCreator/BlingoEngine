using System;
using AbstUI.Components.Graphics;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using AbstUI.LUnity.Primitives;
using AbstUI.Primitives;
using AbstUI.Texts;
using UnityEngine;
using UnityEngine.UI;
using AbstUI.Components;
using AbstUI.Styles;

namespace AbstUI.LUnity.Components.Graphics;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkGfxCanvas"/> that delegates drawing
/// to a <see cref="UnityImagePainter"/> and displays the resulting <see cref="Texture2D"/>.
/// </summary>
internal class AbstUnityGfxCanvas : AbstUnityComponent, IAbstFrameworkGfxCanvas, IFrameworkFor<AbstGfxCanvas>, IDisposable
{
    private readonly RawImage _image;
    private readonly UnityImagePainter _painter;
    private readonly GfxCanvasBehaviour _behaviour;

    public AbstUnityGfxCanvas(UnityImagePainter painter)
        : base(CreateGameObject(out var image, out var behaviour))
    {
        _painter = painter;
        _image = image;
        _behaviour = behaviour;
        _image.texture = _painter.Texture;
        _behaviour.Init(this);
        Width = _painter.Width;
        Height = _painter.Height;
    }

    private static GameObject CreateGameObject(out RawImage image, out GfxCanvasBehaviour behaviour)
    {
        var go = new GameObject("GfxCanvas", typeof(RectTransform));
        image = go.AddComponent<RawImage>();
        behaviour = go.AddComponent<GfxCanvasBehaviour>();
        return go;
    }

    public bool Pixilated
    {
        get => _painter.Pixilated;
        set => _painter.Pixilated = value;
    }

    public new float Width
    {
        get => base.Width;
        set
        {
            if (Math.Abs(base.Width - value) < float.Epsilon)
                return;
            base.Width = value;
            _painter.Resize((int)value, _painter.Height);
            Redraw();
        }
    }

    public new float Height
    {
        get => base.Height;
        set
        {
            if (Math.Abs(base.Height - value) < float.Epsilon)
                return;
            base.Height = value;
            _painter.Resize(_painter.Width, (int)value);
            Redraw();
        }
    }

    internal void Redraw()
    {
        _painter.Render();
        _image.texture = _painter.Texture;
        base.Width = _painter.Width;
        base.Height = _painter.Height;
    }

    public void Clear(AColor color) => _painter.Clear(color);

    public void SetPixel(APoint point, AColor color) => _painter.SetPixel(point, color);

    public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
        => _painter.DrawLine(start, end, color, width);

    public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
        => _painter.DrawRect(rect, color, filled, width);

    public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
        => _painter.DrawCircle(center, radius, color, filled, width);

    public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
        => _painter.DrawArc(center, radius, startDeg, endDeg, segments, color, width);

    public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
        => _painter.DrawPolygon(points, color, filled, width);

    public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12,
        int width = -1, AbstTextAlignment alignment = default)
        => _painter.DrawText(position, text, font, color, fontSize, width, alignment);

    public void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12,
           int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left,
           AbstFontStyle style = AbstFontStyle.Regular) => _painter.DrawSingleLine(position, text, font, color, fontSize, width, height, alignment, style);
    public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
        => _painter.DrawPicture(data, width, height, position, format);

    public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
        => _painter.DrawPicture(texture, width, height, position);

    public IAbstTexture2D GetTexture(string? name = null) => _painter.GetTexture(name);

    float IAbstFrameworkNode.Width
    {
        get => Width;
        set => Width = value;
    }

    float IAbstFrameworkNode.Height
    {
        get => Height;
        set => Height = value;
    }

    public new void Dispose()
    {
        _painter.Dispose();
        base.Dispose();
    }

    private class GfxCanvasBehaviour : MonoBehaviour
    {
        private AbstUnityGfxCanvas? _canvas;
        public void Init(AbstUnityGfxCanvas canvas) => _canvas = canvas;
        private void LateUpdate() => _canvas?.Redraw();
    }
}

