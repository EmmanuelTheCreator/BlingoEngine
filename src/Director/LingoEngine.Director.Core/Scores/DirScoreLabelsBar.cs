using LingoEngine.Commands;
using LingoEngine.Movies;
using LingoEngine.Movies.Commands;
using LingoEngine.Primitives;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Gfx;
using LingoEngine.Events;

namespace LingoEngine.Director.Core.Scores;

public class DirScoreLabelsBar : IDisposable
{
    private readonly DirScoreGfxValues _gfxValues;
    private readonly ILingoCommandManager _commandManager;
    private readonly ILingoMouse _mouse;
    private readonly LingoGfxCanvas _canvas;
    private readonly LingoGfxInputText _editField;
    private readonly ILingoMouseSubscription _mouseDownSub;
    private readonly ILingoMouseSubscription _mouseUpSub;
    private readonly ILingoMouseSubscription _mouseMoveSub;
    private LingoMovie? _movie;
    private LingoPoint _position;
    private string? _activeLabel;
    private int _activeFrame;
    private int _startFrame;
    private bool _dragging;
    private bool _headerCollapsed;

    public event Action<bool>? HeaderCollapseChanged;
    public Action? RequestRedraw;

    public DirScoreLabelsBar(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, ILingoMouse mouse, ILingoCommandManager commandManager, LingoPoint position)
    {
        _gfxValues = gfxValues;
        _commandManager = commandManager;
        _mouse = mouse;
        _position = position;
        _canvas = factory.CreateGfxCanvas("ScoreLabelsBar", 0, _gfxValues.LabelsBarHeight);
        _editField = factory.CreateInputText("ScoreLabelsEdit", onChange: _ => CommitEdit());
        _editField.Visibility = false;
        _editField.Width = 120;
        _editField.Height = 16;
        _mouseDownSub = _mouse.OnMouseDown(OnMouseDown);
        _mouseUpSub = _mouse.OnMouseUp(OnMouseUp);
        _mouseMoveSub = _mouse.OnMouseMove(OnMouseMove);
    }

    public bool HeaderCollapsed
    {
        get => _headerCollapsed;
        set
        {
            _headerCollapsed = value;
            RequestRedraw?.Invoke();
        }
    }

    public float Width => _canvas.Width;
    public float Height => _canvas.Height;
    public LingoGfxCanvas Canvas => _canvas;
    public LingoGfxInputText EditField => _editField;

    public void Dispose()
    {
        _mouseDownSub.Release();
        _mouseUpSub.Release();
        _mouseMoveSub.Release();
        _canvas.Dispose();
        _editField.Dispose();
    }

    public void ToggleCollapsed()
    {
        HeaderCollapsed = !HeaderCollapsed;
        HeaderCollapseChanged?.Invoke(HeaderCollapsed);
    }

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        _canvas.Width = _gfxValues.LeftMargin + (_movie?.FrameCount ?? 0) * _gfxValues.FrameWidth;
        _canvas.Height = _gfxValues.LabelsBarHeight;
        _editField.Visibility = false;
        RequestRedraw?.Invoke();
    }

    public void UpdatePosition(LingoPoint position) => _position = position;

    public void Draw()
    {
        if (_movie == null) return;
        _canvas.Clear(LingoColorList.White);
        foreach (var kv in _movie.GetScoreLabels())
        {
            float x = _gfxValues.LeftMargin + (kv.Value - 1) * _gfxValues.FrameWidth;
            var pts = new[]
            {
                new LingoPoint(x,5),
                new LingoPoint(x+10,5),
                new LingoPoint(x+5,15)
            };
            _canvas.DrawPolygon(pts, LingoColorList.Black);
            _canvas.DrawText(new LingoPoint(x+12,10), kv.Key, null, LingoColorList.Black, 10);
        }
    }

    private bool InBar(float mouseH, float mouseV)
    {
        return mouseH >= _position.X && mouseH <= _position.X + Width &&
               mouseV >= _position.Y && mouseV <= _position.Y + Height;
    }

    private void OnMouseDown(LingoMouseEvent e)
    {
        if (_movie == null) return;
        if (!InBar(e.MouseH, e.MouseV)) return;
        if (!e.Mouse.LeftMouseDown) return;
        float localX = e.MouseH - _position.X;
        if (localX > Width - 20)
        {
            ToggleCollapsed();
            return;
        }
        foreach (var kv in _movie.GetScoreLabels())
        {
            float x = _gfxValues.LeftMargin + (kv.Value - 1) * _gfxValues.FrameWidth;
            float width = EstimateLabelWidth(kv.Key) + 20;
            if (localX >= x && localX <= x + width)
            {
                _activeLabel = kv.Key;
                _activeFrame = kv.Value;
                _startFrame = kv.Value;
                _editField.Text = kv.Key;
                if (e.Mouse.DoubleClick)
                {
                    _dragging = false;
                    UpdateEditFieldPosition();
                    _editField.Visibility = true;
                }
                else
                {
                    _dragging = true;
                }
                break;
            }
        }
    }

    private void OnMouseUp(LingoMouseEvent e)
    {
        if (_dragging)
        {
            CommitEdit();
            _dragging = false;
        }
    }

    private void OnMouseMove(LingoMouseEvent e)
    {
        if (_movie == null || !_dragging) return;
        float localX = e.MouseH - _position.X;
        float frameF = (localX - _gfxValues.LeftMargin) / _gfxValues.FrameWidth;
        int frame = LingoMath.Clamp(LingoMath.RoundToInt(frameF) + 1, 1, _movie.FrameCount);
        if (frame != _activeFrame)
        {
            _activeFrame = frame;
            UpdateEditFieldPosition();
            RequestRedraw?.Invoke();
        }
    }

    private void UpdateEditFieldPosition()
    {
        float x = _gfxValues.LeftMargin + (_activeFrame - 1) * _gfxValues.FrameWidth + 12;
        _editField.X = x;
        _editField.Y = 2;
    }

    private void CommitEdit()
    {
        if (_activeLabel == null || _movie == null) return;
        if (_activeFrame != _startFrame || _editField.Text != _activeLabel)
            _commandManager.Handle(new UpdateFrameLabelCommand(_startFrame, _activeFrame, _editField.Text));
        _activeLabel = null;
        _editField.Visibility = false;
        RequestRedraw?.Invoke();
    }

    private float EstimateLabelWidth(string text) => text.Length * 8;
}
