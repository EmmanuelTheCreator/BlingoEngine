using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using System.Collections.Generic;

namespace LingoEngine.Director.Core.Stages;

/// <summary>
/// Draws user defined guides and a configurable grid on the stage canvas.
/// </summary>
public class DirectorStageGuides
{
    private readonly LingoGfxCanvas _canvas;

    public IList<float> VerticalGuides { get; } = new List<float>();
    public IList<float> HorizontalGuides { get; } = new List<float>();

    private LingoColor _guidesColor = LingoColorList.Blue;
    public LingoColor GuidesColor { get => _guidesColor; set { _guidesColor = value; Draw(); } }
    private bool _guidesVisible = true;
    public bool GuidesVisible { get => _guidesVisible; set { _guidesVisible = value; Draw(); } }
    private bool _guidesSnap;
    public bool GuidesSnap { get => _guidesSnap; set { _guidesSnap = value; } }
    private bool _guidesLocked;
    public bool GuidesLocked { get => _guidesLocked; set { _guidesLocked = value; } }

    private LingoColor _gridColor = LingoColorList.Gray;
    public LingoColor GridColor { get => _gridColor; set { _gridColor = value; Draw(); } }
    private bool _gridVisible;
    public bool GridVisible { get => _gridVisible; set { _gridVisible = value; Draw(); } }
    private bool _gridSnap;
    public bool GridSnap { get => _gridSnap; set { _gridSnap = value; } }
    private float _gridWidth = 32;
    public float GridWidth { get => _gridWidth; set { _gridWidth = value; Draw(); } }
    private float _gridHeight = 32;
    public float GridHeight { get => _gridHeight; set { _gridHeight = value; Draw(); } }

    public LingoGfxCanvas Canvas => _canvas;

    public DirectorStageGuides(ILingoFrameworkFactory factory)
    {
        _canvas = factory.CreateGfxCanvas("StageGuidesCanvas", 3000, 2000);
        _canvas.Visibility = false;
    }

    public void AddVertical(float x) { if (!GuidesLocked) { VerticalGuides.Add(x); Draw(); } }
    public void AddHorizontal(float y) { if (!GuidesLocked) { HorizontalGuides.Add(y); Draw(); } }
    public void RemoveAll() { if (!GuidesLocked) { VerticalGuides.Clear(); HorizontalGuides.Clear(); Draw(); } }

    public void Draw()
    {
        _canvas.Clear(LingoColorList.Transparent);

        if (GridVisible && GridWidth > 0 && GridHeight > 0)
        {
            for (float x = 0; x <= _canvas.Width; x += GridWidth)
                _canvas.DrawLine(new LingoPoint(x, 0), new LingoPoint(x, _canvas.Height), GridColor);
            for (float y = 0; y <= _canvas.Height; y += GridHeight)
                _canvas.DrawLine(new LingoPoint(0, y), new LingoPoint(_canvas.Width, y), GridColor);
        }

        if (GuidesVisible)
        {
            foreach (var x in VerticalGuides)
                _canvas.DrawLine(new LingoPoint(x, 0), new LingoPoint(x, _canvas.Height), GuidesColor);
            foreach (var y in HorizontalGuides)
                _canvas.DrawLine(new LingoPoint(0, y), new LingoPoint(_canvas.Width, y), GuidesColor);
        }

        _canvas.Visibility = GuidesVisible || GridVisible;
    }
}
