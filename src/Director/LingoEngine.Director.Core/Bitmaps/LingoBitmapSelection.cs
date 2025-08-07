using System.Collections.Generic;
using System.Linq;
using LingoEngine.Primitives;
using LingoEngine.Gfx;

namespace LingoEngine.Director.Core.Bitmaps;

public class LingoBitmapSelection
{
    private readonly HashSet<LingoPoint> _selectedPixels = new();
    private readonly List<LingoPoint> _lassoPoints = new();

    private bool _dragSelecting;
    private LingoRect? _dragRect;
    private bool _lassoSelecting;

    public IReadOnlyCollection<LingoPoint> SelectedPixels => _selectedPixels;
    public bool IsDragSelecting => _dragSelecting;
    public LingoRect? DragRect => _dragRect;
    public bool IsLassoSelecting => _lassoSelecting;
    public IReadOnlyList<LingoPoint> LassoPoints => _lassoPoints;

    private LingoPoint _canvasHalf;
    private LingoPoint _offset;

    public void Prepare(LingoGfxCanvas canvas, int memberWidth, int memberHeight)
    {
        _canvasHalf = new LingoPoint(canvas.Width / 2f, canvas.Height / 2f);
        var imageHalf = new LingoPoint(memberWidth / 2f, memberHeight / 2f);
        _offset = _canvasHalf - imageHalf;
    }

    public LingoPoint ToCanvas(LingoPoint point, float scale)
        => (_offset + point) * scale + _canvasHalf * (1 - scale);

    public LingoRect ToCanvas(LingoRect rect, float scale)
    {
        var pos = ToCanvas(rect.TopLeft, scale);
        return LingoRect.New(pos.X, pos.Y, rect.Width * scale, rect.Height * scale);
    }

    public void BeginRectSelection()
    {
        _dragSelecting = true;
        _dragRect = null;
    }

    public void UpdateRectSelection(LingoPoint start, LingoPoint end)
    {
        _dragRect = LingoPoint.RectFromPoints(start, end);
    }

    public void EndRectSelection()
    {
        _dragSelecting = false;
        _dragRect = null;
    }

    public void BeginLassoSelection(LingoPoint start)
    {
        _lassoPoints.Clear();
        _lassoPoints.Add(start);
        _lassoSelecting = true;
    }

    public void AddLassoPoint(int x, int y)
    {
        _lassoPoints.Add(new LingoPoint(x, y));
    }

    public IReadOnlyList<LingoPoint> EndLassoSelection()
    {
        _lassoSelecting = false;
        var points = _lassoPoints.ToArray();
        _lassoPoints.Clear();
        return points;
    }

    public void ApplySelection(IEnumerable<LingoPoint> pixels, bool ctrl, bool shift)
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

    public void ApplySelection(LingoRect rect, bool ctrl, bool shift)
    {
        ApplySelection(PixelsInRect(rect), ctrl, shift);
    }

    private static IEnumerable<LingoPoint> PixelsInRect(LingoRect rect)
    {
        for (int y = (int)rect.Top; y < (int)(rect.Top + rect.Height); y++)
            for (int x = (int)rect.Left; x < (int)(rect.Left + rect.Width); x++)
                yield return new LingoPoint(x, y);
    }
}
