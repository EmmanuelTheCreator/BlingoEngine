using System.Collections.Generic;
using System.Linq;
using AbstUI.Primitives;
using AbstUI.Components;

namespace LingoEngine.Director.Core.Bitmaps;

public class LingoBitmapSelection
{
    private readonly HashSet<APoint> _selectedPixels = new();
    private readonly List<APoint> _lassoPoints = new();

    private bool _dragSelecting;
    private ARect? _dragRect;
    private bool _lassoSelecting;

    public IReadOnlyCollection<APoint> SelectedPixels => _selectedPixels;
    public bool IsDragSelecting => _dragSelecting;
    public ARect? DragRect => _dragRect;
    public bool IsLassoSelecting => _lassoSelecting;
    public IReadOnlyList<APoint> LassoPoints => _lassoPoints;

    private APoint _canvasHalf;
    private APoint _offset;

    public void Prepare(AbstUIGfxCanvas canvas, int memberWidth, int memberHeight)
    {
        _canvasHalf = new APoint(canvas.Width / 2f, canvas.Height / 2f);
        var imageHalf = new APoint(memberWidth / 2f, memberHeight / 2f);
        _offset = _canvasHalf - imageHalf;
    }

    public APoint ToCanvas(APoint point, float scale)
        => (_offset + point) * scale + _canvasHalf * (1 - scale);

    public ARect ToCanvas(ARect rect, float scale)
    {
        var pos = ToCanvas(rect.TopLeft, scale);
        return ARect.New(pos.X, pos.Y, rect.Width * scale, rect.Height * scale);
    }

    public void BeginRectSelection()
    {
        _dragSelecting = true;
        _dragRect = null;
    }

    public void UpdateRectSelection(APoint start, APoint end)
    {
        _dragRect = APoint.RectFromPoints(start, end);
    }

    public void EndRectSelection()
    {
        _dragSelecting = false;
        _dragRect = null;
    }

    public void BeginLassoSelection(APoint start)
    {
        _lassoPoints.Clear();
        _lassoPoints.Add(start);
        _lassoSelecting = true;
    }

    public void AddLassoPoint(int x, int y)
    {
        _lassoPoints.Add(new APoint(x, y));
    }

    public IReadOnlyList<APoint> EndLassoSelection()
    {
        _lassoSelecting = false;
        var points = _lassoPoints.ToArray();
        _lassoPoints.Clear();
        return points;
    }

    public void ApplySelection(IEnumerable<APoint> pixels, bool ctrl, bool shift)
    {
        if (shift)
        {
            foreach (var p in pixels)
                _selectedPixels.Remove(p);
        }
        else if (ctrl)
        {
            foreach (var p in pixels)
                _selectedPixels.Add(p);
        }
        else
        {
            _selectedPixels.Clear();
            foreach (var p in pixels)
                _selectedPixels.Add(p);
        }
    }

    public void ApplySelection(ARect rect, bool ctrl, bool shift)
    {
        ApplySelection(PixelsInRect(rect), ctrl, shift);
    }

    private static IEnumerable<APoint> PixelsInRect(ARect rect)
    {
        for (int y = (int)rect.Top; y < (int)(rect.Top + rect.Height); y++)
            for (int x = (int)rect.Left; x < (int)(rect.Left + rect.Width); x++)
                yield return new APoint(x, y);
    }
}
