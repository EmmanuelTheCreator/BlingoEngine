using LingoEngine.Commands;
using LingoEngine.Movies;
using LingoEngine.Movies.Commands;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Events;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.UI;
using System.Drawing;
using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.Director.Core.Scores;

public class DirScoreLabelsBar : IDisposable
{
    private readonly DirScoreGfxValues _gfxValues;
    private readonly ILingoCommandManager _commandManager;
    private readonly LingoGfxCanvas _canvasFix;
    private readonly LingoGfxCanvas _canvasOpenCollapse;
    private readonly LingoGfxCanvas _canvas;
    private readonly LingoGfxInputText _editField;
    private readonly LingoGfxButton _btnApply;
    private readonly LingoGfxButton _btnCancel;
    private readonly LingoGfxButton _btnDelete;
    private readonly ILingoGfxLayoutNode _btnApplyLayout;
    private readonly ILingoGfxLayoutNode _btnCancelLayout;
    private readonly ILingoGfxLayoutNode _btnDeleteLayout;
    private LingoMovie? _movie;
    //private readonly LingoGfxInputCombobox _labelsCombo;
    private readonly LingoGfxItemList _labelsCombo;
    private readonly LingoGfxPanel _scollingPanel;
    private readonly LingoGfxPanel _fixPanel;
    private FrameLabelData? _activeFrameLabel;
    private bool _dragging;
    private bool _headerCollapsed;
    private Dictionary<int,FrameLabelData> _frameLabels = new();
    private KeyValuePair<string, string>[] _frameLabelsForCombo = [
                    new KeyValuePair<string, string>("Label1","Label1"),
               ];



    public event Action<bool>? HeaderCollapseChanged;
    private int _dragFrame;
    private int _mouseFrameOffset;
    public bool IsEditing { get; private set; }
    public string TextEdit { get; set; } = "New label";
    
    public bool HeaderCollapsed
    {
        get => _headerCollapsed;
        set
        {
            _headerCollapsed = value;
        }
    }

    public float Width => _fixPanel.Width;
    public float Height => _fixPanel.Height;
    public float ScrollingWidth => _scollingPanel.Width;
    public float ScollingHeight => _scollingPanel.Height;
    public LingoGfxPanel ScollingPanel => _scollingPanel;
    public LingoGfxPanel FixPanel => _fixPanel;
    //public LingoGfxInputText EditField => _editField;



    public DirScoreLabelsBar(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, ILingoCommandManager commandManager)
    {
        _gfxValues = gfxValues;
        _commandManager = commandManager;


        // FixLayout panel
        _fixPanel = factory.CreatePanel("ScoreLabelsFixBarPanel");
        _fixPanel.Height = gfxValues.LabelsBarHeight;
        _fixPanel.Width = 500;
        _fixPanel.BackgroundColor = AColors.Transparent;

        _canvasFix = factory.CreateGfxCanvas("ScoreCanvasFix", 500, gfxValues.LabelsBarHeight);
        _canvasOpenCollapse = factory.CreateGfxCanvas("ScoreOpenCollapse", 20, 13);
        DrawCollapseButton();
        _fixPanel.AddItem(_canvasFix, 0, 0);
        _fixPanel.AddItem(_canvasOpenCollapse, 10, 4);
        _labelsCombo = _fixPanel.SetInputListAt(_frameLabelsForCombo, "ScoreLabelsBarListSelect", 2, 20, _gfxValues.ChannelInfoWidth, null,key => MoveHeadTo(key));
        _labelsCombo.Visibility = false;
        _labelsCombo.Height = 100;

        // Scolling panel
        _scollingPanel = factory.CreatePanel("ScoreLabelsScollBarPanel");
        _scollingPanel.Height = gfxValues.LabelsBarHeight;
        _scollingPanel.Width = 500;
        //_canvas = factory.CreateGfxCanvas("ScoreLabelsBar", 500, _gfxValues.LabelsBarHeight);
        //_panel.AddItem(_canvas);

        _canvas = _scollingPanel.SetGfxCanvasAt("ScoreLabelsBar", 0, 0, 500, _gfxValues.LabelsBarHeight);
        _editField = _scollingPanel.SetInputTextAt(this, "ScoreTextEdit", 20, 0, 100, x => x.TextEdit, 50);
        (_btnApply, _btnApplyLayout) = _scollingPanel.SetButtonAt("ScoreBtnApplyLabelEdit", "Apply", 0, 0, () => CommitEdit(true), 50);
        (_btnCancel, _btnCancelLayout) = _scollingPanel.SetButtonAt("ScoreBtnCancelLabelEdit", "X", 40, 0, () => CommitEdit(false), 20);
        (_btnDelete, _btnDeleteLayout) = _scollingPanel.SetButtonAt("ScoreBtnCancelLabelEdit", "Delete", 40, 0, () => { Delete(_activeFrameLabel); CommitEdit(false); }, 20);
        ShowEdit(false);
    }

    

    public void Dispose()
    {
        if (_movie != null)
            _movie.FrameLabels.LabelsChanged -= OnFrameLabelsChanged;
        _canvas.Dispose();
        _editField.Dispose();
        _btnApply.Dispose();
        _canvasFix.Dispose();
        _canvasOpenCollapse.Dispose();
        _labelsCombo.Dispose();
    }

    public void SetMovie(LingoMovie? movie)
    {
        if (_movie != null)
            _movie.FrameLabels.LabelsChanged -= OnFrameLabelsChanged;
        _movie = movie;
        if (_movie != null)
            _movie.FrameLabels.LabelsChanged += OnFrameLabelsChanged;
        var width = (_movie?.FrameCount ?? 300) * _gfxValues.FrameWidth;
        _scollingPanel.Width = width;
        _canvas.Width = width;
        DrawScrollable();
    }
    private void OnFrameLabelsChanged()
    {
        DrawScrollable();
        if (_movie == null) return;
        InitCombo();
    }

    

    public void OnResize(int width, int height)
    {
        _fixPanel.Width = width;
        _canvasFix.Width = width;
        _canvasOpenCollapse.X = width - 20;
        RedrawFix();
    }



    #region Fix Gfx data
    private void InitCombo()
    {
        if (_movie == null) return;
        _frameLabelsForCombo = _movie.FrameLabels.GetScoreLabels()
                    .Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Key))
                    .ToArray();
        _labelsCombo.ClearItems();
        foreach (var item in _frameLabelsForCombo)
            _labelsCombo.AddItem(item.Key, item.Value);
    }
    private void RedrawFix()
    {
        var height = _gfxValues.LabelsBarHeight;
        _canvasFix.Clear(AColors.Transparent);
        // info tight line
        _canvasFix.DrawVLineR(_gfxValues.ChannelInfoWidth, 0, height);
        // Bottom line
        _canvasFix.DrawHLine(0, height, Width);
        // draw combo Label
        var x = 7;
        var y = 5;
        APoint[] pts = [new APoint(x, y+3), new APoint(x + 8, y+3), new APoint(x + 4, y+11)];
        _canvasFix.DrawPolygon(pts, AColors.DarkGray);
        _canvasFix.DrawLine(new APoint(x-1, y-2), new APoint(x + 10, y-2 ), AColors.DarkGray);
        _canvasFix.DrawLine(new APoint(x-1, y), new APoint(x + 10, y ), AColors.DarkGray);

        // draw previous Label
        x = 30;
        y = 5;
        pts = [new APoint(x + 10, y), new APoint(x + 10, y + 8), new APoint(x + 3, y + 4)];
        _canvasFix.DrawPolygon(pts, AColors.DarkGray);
        // Draw next label
        x = 50;
        pts = [new APoint(x + 3, y), new APoint(x + 3, y + 8), new APoint(x + 10, y + 4)];
        _canvasFix.DrawPolygon(pts, AColors.DarkGray);
    }
    private void SelectCombo(int newFrame)
    {
        if (!_frameLabels.TryGetValue(newFrame, out var frameData)) return;
        _labelsCombo.SelectedKey = frameData.Name;
    }
    #endregion


    public void DrawScrollable()
    {
        if (_movie == null) return;
        _canvas.Clear(AColors.White);
        
        var labels = _movie.FrameLabels.GetScoreLabels();
        _frameLabels.Clear();
        foreach (KeyValuePair<string, int> kv in labels)
        {
            if (_dragging && _activeFrameLabel != null && _activeFrameLabel.Name == kv.Key)
            {
                // Skip the label being dragged
                continue;
            }
            var frameData = new FrameLabelData(kv, _gfxValues.FrameWidth);
            _frameLabels.Add(frameData.Frame, frameData);
        }
        foreach (var frameData in _frameLabels.Values.OrderBy(x => x.Frame))
            frameData.DrawOnCanvas(_canvas, _gfxValues.LabelsBarHeight,frameData.X, AColors.DarkGray);

        // Draw dragging pointer
        if (_dragging && _activeFrameLabel != null)
        {
            var x = (_dragFrame - 1) * _gfxValues.FrameWidth;
            _activeFrameLabel.DrawOnCanvas(_canvas, _gfxValues.LabelsBarHeight, x, AColors.Black);
            //Console.WriteLine("DirScoreLabelsBar.Draw():" + _dragFrame + ":" + x+":"+ _activeFrameLabel.Name+":"+ _activeFrameLabel.Frame);
            _canvas.DrawRect(new ARect(x-20, 0, x, _gfxValues.LabelsBarHeight), AColors.White, true);
            _canvas.DrawText(new APoint(x-18, 13), _dragFrame.ToString(),null,AColors.Gray,9);
        }
    }

  
    internal void HandleMouseEvent(LingoMouseEvent mouseEvent, int mouseFrame)
    {
        if (mouseEvent.Type == LingoMouseEventType.MouseDown) MouseDown(mouseEvent, mouseFrame);
        else if (mouseEvent.Type == LingoMouseEventType.MouseUp) MouseUp(mouseEvent);
        else if (mouseEvent.Type == LingoMouseEventType.MouseMove) MouseMove(mouseEvent, mouseFrame);
    }
    private void MouseDown(LingoMouseEvent e, int mouseFrame)
    {
        //Console.WriteLine($"Mouse Down: {e.MouseH} {e.MouseV} Frame: {mouseFrame}");
        if (_movie == null) return;
        if (!e.Mouse.LeftMouseDown) return;
        if (e.MouseH > Width - 20)
        {
            ToggleCollapsed();
            return;
        }
        else if (e.MouseH < _gfxValues.ChannelInfoWidth)
        {
            if (e.MouseH > 50) { MoveHeadNextFrame(); return; }
            if (e.MouseH > 30) { MoveHeadPreviousFrame(); return; }
            _labelsCombo.Visibility = !_labelsCombo.Visibility;
            return;
        }
        if (IsEditing)
            return;

        // first try by the key
        if (!_frameLabels.TryGetValue(mouseFrame, out var frameLabel))
        {
            // then try if click on label
            frameLabel = _frameLabels.Values.FirstOrDefault(x => x.IsFramePointInside(mouseFrame));
            if (frameLabel == null)
            {
                if (e.Mouse.DoubleClick)
                    CreateNewLabel(mouseFrame);
                return;
            }
        }
        //Console.WriteLine("ok=" + frameLabel.Name+":"+ frameLabel.FrameWidth);
        _mouseFrameOffset = mouseFrame - frameLabel.Frame;
        
        if (e.Mouse.DoubleClick)
            StartEditing(frameLabel);
        else
            StartDragging(frameLabel);
    }

  



    #region Dragging
    private void StartDragging(FrameLabelData frameLabel)
    {
        _activeFrameLabel = frameLabel;
        _dragFrame = _activeFrameLabel.Frame;
        _dragging = true;
        IsEditing = false;
        DrawScrollable();
    }
    private void MouseMove(LingoMouseEvent e, int mouseFrame)
    {
        if (_movie == null || !_dragging || _activeFrameLabel == null) return;
        var newFrame = mouseFrame - _mouseFrameOffset;
        if (newFrame != _dragFrame)
        {
            _dragFrame = newFrame;
            //if (e.MouseV < 0)
            //{
            //    Delete(_activeFrameLabel);
            //    StopDragging();
            //}
            //Console.WriteLine("_dragFrame:" + _dragFrame + ":" + _activeFrameLabel.Name);
            DrawScrollable();
        }
    }

  
    private void MouseUp(LingoMouseEvent e)
    {
        if (_dragging && _activeFrameLabel != null)
        {
            _commandManager.Handle(new UpdateFrameLabelCommand(_activeFrameLabel.Frame, _dragFrame, _activeFrameLabel.Name));
            StopDragging();
        }
    }

    private void StopDragging()
    {
        _dragging = false;
        DrawScrollable();
    }

    #endregion




    #region Edit Text
    private void StartEditing(FrameLabelData frameLabel)
    {
        TextEdit = frameLabel.Name;
        _activeFrameLabel = frameLabel;
        _dragging = false;
        _editField.Text = frameLabel.Name;
        ShowEdit(true);
        IsEditing = true;
        float x = _gfxValues.LeftMargin + (_activeFrameLabel.Frame - 1) * _gfxValues.FrameWidth + 12;
        _editField.X = x;
        _editField.Y = 2;
        _btnApplyLayout.X =  x + _editField.Width + 2;
        _btnCancelLayout.X =  x + _editField.Width + 2 + _btnApply.Width + 2;
        _btnDeleteLayout.X =  x + _editField.Width + 2 + _btnApply.Width + 2 + _btnCancel.Width + 2;
    }

    private void CommitEdit(bool saveChanges)
    {
        var newText = TextEdit;
        if (saveChanges && _activeFrameLabel != null && _activeFrameLabel.Name != newText)
            _commandManager.Handle(new UpdateFrameLabelCommand(_activeFrameLabel.Frame, _activeFrameLabel.Frame, newText));
        _activeFrameLabel = null;
        ShowEdit(false);
        DrawScrollable();
        IsEditing = false;
    }
    private void ShowEdit(bool state)
    {
        _btnApply.Visibility = state;
        _editField.Visibility = state;
        _btnCancel.Visibility = state;
        _btnDelete.Visibility = state;
    }

    #endregion



    #region Commands
    private void Delete(FrameLabelData? frameLabel)
    {
        if (frameLabel == null) return;
        _commandManager.Handle(new DeleteFrameLabelCommand(frameLabel.Frame));
    }


    private void CreateNewLabel(int mouseFrame)
    {
        _commandManager.Handle(new AddFrameLabelCommand(mouseFrame, "New"));
    }
    private void MoveHeadPreviousFrame()
    {
        if (_movie == null) return;
        var newFrame = _movie.FrameLabels.GetPreviousLabelFrame(_movie.CurrentFrame);
        if (newFrame < 0) return;
        var offset = newFrame - _movie.CurrentFrame;
        _commandManager.Handle(new StepFrameCommand(offset));
        SelectCombo(newFrame);
    }

   

    private void MoveHeadNextFrame()
    {
        if (_movie == null) return;
        var newFrame = _movie.FrameLabels.GetNextLabelFrame(_movie.CurrentFrame);
        if (newFrame <= _movie.CurrentFrame) return;
        var offset = newFrame - _movie.CurrentFrame;
        _commandManager.Handle(new StepFrameCommand(offset));
        SelectCombo(newFrame);
    }
    private void MoveHeadTo(string? key)
    {
        if (_movie == null) return;
        var frameData = _frameLabels.Values.FirstOrDefault(x => x.Name == key);
        if (frameData == null) return;
        var newFrame = frameData.Frame;
        if (newFrame == _movie.CurrentFrame) return;
        var offset = newFrame - _movie.CurrentFrame;
        _commandManager.Handle(new StepFrameCommand(offset));
        SelectCombo(newFrame);
    }
    #endregion




    #region Collapser
    public void ToggleCollapsed()
    {
        HeaderCollapsed = !HeaderCollapsed;
        HeaderCollapseChanged?.Invoke(HeaderCollapsed);
        DrawCollapseButton();
    }
    private void DrawCollapseButton()
    {
        _canvasOpenCollapse.Clear(AColors.White);
        _canvasOpenCollapse.DrawRect(new ARect(0, 0, 12, 12), AColors.Black, false, 1);
        var x = 1;
        var y = 1;
        APoint[] pts = !HeaderCollapsed
            ? [new APoint(x, 3), new APoint(x + 8, 3), new APoint(x + 4, 11)]
            : [new APoint(10, y), new APoint(10, y + 8), new APoint(3, y + 4)];
        _canvasOpenCollapse.DrawPolygon(pts, AColors.Black);
    }

    #endregion




    private record FrameLabelData
    {
        public string Name { get; }
        public int Frame { get; }
        public float WidthPx { get; }
        public int FrameWidth { get; }
        public float X { get; }

        public FrameLabelData(KeyValuePair<string, int> frameLabel, float frameWidthPx)
        {
            Name = frameLabel.Key;
            Frame = frameLabel.Value;
            WidthPx = EstimateLabelWidth(Name) + 20;
            FrameWidth = MathL.RoundToInt(WidthPx / frameWidthPx);
            X = (Frame - 1) * frameWidthPx;
        }
        
        private float EstimateLabelWidth(string text) => text.Length * 8;

        internal bool IsFramePointInside(int mouseFrame)
        {
            return mouseFrame >= Frame && mouseFrame < Frame + FrameWidth;
        }
        public void DrawOnCanvas(LingoGfxCanvas _canvas, int labelsBarHeight, float x, AColor color)
        {
            var pts = new[]
            {
                new APoint(x,5),
                new APoint(x+10,5),
                new APoint(x+5,15)
            };
            _canvas.DrawRect(new ARect(x, 0, x + WidthPx, labelsBarHeight), AColors.White, true);
            _canvas.DrawPolygon(pts, color);
            _canvas.DrawText(new APoint(x + 12, 12), Name, null, color, 10);
        }
    }

}
