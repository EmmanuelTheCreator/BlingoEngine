using System;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Director.Core.Styles;
using AbstUI.Components.Containers;
using AbstUI.Components.Graphics;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;

namespace BlingoEngine.Director.Core.Bitmaps;

public class DirectorBitmapEditWindowV2 : DirectorWindow<IDirFrameworkBitmapEditWindowV2>
{
    private readonly AbstZoomBox _zoomBox;
    private readonly AbstPanel _rootPanel;
    private readonly AbstGfxCanvas _background;
    private readonly PaintCanvasV2 _paintCanvas;
    private readonly AbstGfxCanvas _selectionCanvas;
    private readonly RegPointCanvas _regPointCanvas;
    private readonly PicturePainterV2 _painter;
    private readonly BlingoBitmapSelection _selection = new();

    private PainterToolType _selectedTool = PainterToolType.Pencil;
    private AColor _currentColor = AColors.Black;
    private int _brushSize = 1;
    private bool _mouseDown;
    private bool _panning;
    private bool _spaceDown;
    private APoint _selectionStart;
    private APoint _lastMousePos;
    private float _scale = 1f;
    private bool _ctrlDown;
    private bool _shiftDown;

    private IAbstMouseSubscription? _mouseDownSub;
    private IAbstMouseSubscription? _mouseMoveSub;
    private IAbstMouseSubscription? _mouseUpSub;

    public AbstPanel Panel => _rootPanel;
    public AbstGfxCanvas Background => _background;
    public AbstGfxCanvas PaintCanvas => _paintCanvas.Canvas;
    public AbstGfxCanvas Selection => _selectionCanvas;

    public DirectorBitmapEditWindowV2(IServiceProvider serviceProvider) : base(serviceProvider, DirectorMenuCodes.PictureEditWindow)
    {
        Width = 800;
        Height = 500;
        MinimumWidth = 200;
        MinimumHeight = 150;
        X = 20;
        Y = 120;

        _zoomBox = Factory.ComponentFactory.CreateZoomBox("BitmapEditZoomBox");
        _rootPanel = Factory.ComponentFactory.CreatePanel("BitmapEditZoomContent");
        _zoomBox.Content = _rootPanel;
        _background = Factory.CreateGfxCanvas("BackgroundCanvas", 0, 0);
        _paintCanvas = new PaintCanvasV2(Factory);
        _selectionCanvas = Factory.CreateGfxCanvas("SelectionCanvas", 0, 0);
        _regPointCanvas = new RegPointCanvas(Factory);
        Func<int, int, IAbstTexture2D> texFactory = (w, h) =>
        {
            var p = Factory.ComponentFactory.CreateImagePainterToTexture(w, h);
            p.Clear(AColors.Transparent);
            var t = p.GetTexture();
            p.Dispose();
            return t;
        };
        _painter = new PicturePainterV2(1, 1, texFactory);

        _rootPanel.AddItem(_background);
        _rootPanel.AddItem(_paintCanvas.Canvas);
        _rootPanel.AddItem(_selectionCanvas);
        _rootPanel.AddItem(_regPointCanvas.Canvas);

        Content = _zoomBox;

        _mouseDownSub = MouseT.OnMouseDown(OnMouseDown);
        _mouseMoveSub = MouseT.OnMouseMove(OnMouseMove);
        _mouseUpSub = MouseT.OnMouseUp(OnMouseUp);
    }

    protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
    {
        base.OnInit(frameworkWindow);
        Title = "Bitmap Editor";
    }

    public void SelectTool(PainterToolType tool) => _selectedTool = tool;

    public void SetColor(AColor color) => _currentColor = color;

    public void SetBrushSize(int size) => _brushSize = size;

    private void OnMouseDown(AbstMouseEvent e)
    {
        _mouseDown = true;
        if (_spaceDown)
        {
            _panning = true;
            _lastMousePos = new APoint(e.MouseH, e.MouseV);
            return;
        }

        var point = GetPixel(e);
        switch (_selectedTool)
        {
            case PainterToolType.Pencil:
                _painter.PaintPixel(point, _currentColor);
                _paintCanvas.Draw(_painter, _scale);
                DrawSelectionOverlay();
                break;
            case PainterToolType.PaintBrush:
                _painter.PaintBrush(point, _currentColor, _brushSize);
                _paintCanvas.Draw(_painter, _scale);
                DrawSelectionOverlay();
                break;
            case PainterToolType.Eraser:
                _painter.EraseBrush(point, _brushSize);
                _paintCanvas.Draw(_painter, _scale);
                DrawSelectionOverlay();
                break;
            case PainterToolType.SelectRectangle:
                _selection.BeginRectSelection();
                _selectionStart = point;
                _selection.UpdateRectSelection(_selectionStart, point);
                DrawSelectionOverlay();
                break;
            case PainterToolType.SelectLasso:
                _selection.BeginLassoSelection(point);
                DrawSelectionOverlay();
                break;
        }
    }

    private void OnMouseMove(AbstMouseEvent e)
    {
        if (!_mouseDown)
            return;

        if (_panning)
        {
            var pos = new APoint(e.MouseH, e.MouseV);
            var delta = pos - _lastMousePos;
            _zoomBox.OffsetX += delta.X;
            _zoomBox.OffsetY += delta.Y;
            _lastMousePos = pos;
            return;
        }

        var point = GetPixel(e);
        switch (_selectedTool)
        {
            case PainterToolType.Pencil:
                _painter.PaintPixel(point, _currentColor);
                _paintCanvas.Draw(_painter, _scale);
                DrawSelectionOverlay();
                break;
            case PainterToolType.PaintBrush:
                _painter.PaintBrush(point, _currentColor, _brushSize);
                _paintCanvas.Draw(_painter, _scale);
                DrawSelectionOverlay();
                break;
            case PainterToolType.Eraser:
                _painter.EraseBrush(point, _brushSize);
                _paintCanvas.Draw(_painter, _scale);
                DrawSelectionOverlay();
                break;
            case PainterToolType.SelectRectangle:
                _selection.UpdateRectSelection(_selectionStart, point);
                DrawSelectionOverlay();
                break;
            case PainterToolType.SelectLasso:
                _selection.AddLassoPoint((int)point.X, (int)point.Y);
                DrawSelectionOverlay();
                break;
        }
    }

    private void OnMouseUp(AbstMouseEvent e)
    {
        if (!_mouseDown)
            return;

        if (_panning)
        {
            _panning = false;
            _mouseDown = false;
            return;
        }

        var point = GetPixel(e);
        switch (_selectedTool)
        {
            case PainterToolType.SelectRectangle:
                _selection.UpdateRectSelection(_selectionStart, point);
                if (_selection.DragRect.HasValue)
                    _selection.ApplySelection(_selection.DragRect.Value, ctrl: _ctrlDown, shift: _shiftDown);
                _selection.EndRectSelection();
                DrawSelectionOverlay();
                break;
            case PainterToolType.SelectLasso:
                _selection.AddLassoPoint((int)point.X, (int)point.Y);
                var points = _selection.EndLassoSelection();
                _selection.ApplySelection(points, ctrl: _ctrlDown, shift: _shiftDown);
                DrawSelectionOverlay();
                break;
        }
        _mouseDown = false;
    }

    private APoint GetPixel(AbstMouseEvent e)
        => new((int)(e.MouseH / _scale), (int)(e.MouseV / _scale));

    protected override void OnRaiseKeyDown(AbstKeyEvent key)
    {
        _ctrlDown = key.ControlDown;
        _shiftDown = key.ShiftDown;
        if (key.KeyPressed(AbstUIKeyType.SPACE))
            _spaceDown = true;
    }

    protected override void OnRaiseKeyUp(AbstKeyEvent key)
    {
        _ctrlDown = key.ControlDown;
        _shiftDown = key.ShiftDown;
        if (key.KeyPressed(AbstUIKeyType.SPACE))
            _spaceDown = false;
    }

    private void DrawSelectionOverlay()
    {
        _selectionCanvas.Width = _painter.Width * _scale;
        _selectionCanvas.Height = _painter.Height * _scale;
        _selectionCanvas.Clear(AColors.Transparent);
        _selection.Prepare((int)_selectionCanvas.Width, (int)_selectionCanvas.Height, _painter.Width, _painter.Height);

        foreach (var px in _selection.SelectedPixels)
        {
            var pos = _selection.ToCanvas(px, _scale);
            _selectionCanvas.DrawRect(ARect.New(pos.X, pos.Y, _scale, _scale), DirectorColors.BitmapSelectionFill, true);
        }

        if (_selection.IsDragSelecting && _selection.DragRect.HasValue)
        {
            var rect = _selection.ToCanvas(_selection.DragRect.Value, _scale);
            _selectionCanvas.DrawRect(ARect.New(rect.Left, rect.Top, rect.Width, rect.Height), AColors.Cyan, false);
        }
        else if (_selection.IsLassoSelecting && _selection.LassoPoints.Count > 1)
        {
            for (int i = 0; i < _selection.LassoPoints.Count - 1; i++)
            {
                var p1 = _selection.ToCanvas(_selection.LassoPoints[i], _scale);
                var p2 = _selection.ToCanvas(_selection.LassoPoints[i + 1], _scale);
                _selectionCanvas.DrawLine(p1, p2, AColors.Cyan);
            }
        }
    }

    protected override void OnDispose()
    {
        _mouseDownSub?.Release();
        _mouseMoveSub?.Release();
        _mouseUpSub?.Release();
        base.OnDispose();
    }

    protected override void OnResizing(bool firstLoad, int width, int height)
    {
        base.OnResizing(firstLoad, width, height);
        _zoomBox.Width = width;
        _zoomBox.Height = height;
        _rootPanel.Width = width;
        _rootPanel.Height = height;
        _background.Width = width;
        _background.Height = height;
        _paintCanvas.Canvas.Width = width;
        _paintCanvas.Canvas.Height = height;
        _selectionCanvas.Width = width;
        _selectionCanvas.Height = height;
        _regPointCanvas.Canvas.Width = width;
        _regPointCanvas.Canvas.Height = height;
    }
}


