using LingoEngine.Commands;
using LingoEngine.Movies;
using LingoEngine.Movies.Commands;
using LingoEngine.Primitives;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Events;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.UI;
using System.Numerics;
using System.Reflection.Emit;

namespace LingoEngine.Director.Core.Scores;

public class DirScoreLabelsBar : IDisposable
{
    private readonly DirScoreGfxValues _gfxValues;
    private readonly ILingoCommandManager _commandManager;
    private readonly LingoGfxCanvas _canvasOpenCollapse;
    private readonly LingoGfxCanvas _canvas;
    private readonly LingoGfxInputText _editField;
    private readonly LingoGfxButton _btnApply;
    private readonly LingoGfxButton _btnCancel;
    private readonly ILingoGfxLayoutNode _btnApplyLayout;
    private readonly ILingoGfxLayoutNode _btnCancelLayout;
    private LingoMovie? _movie;
    private readonly LingoGfxPanel _scollingPanel;
    private readonly LingoGfxPanel _fixPanel;
    private FrameLabelData? _activeFrameLabel;
    private bool _dragging;
    private bool _headerCollapsed;
    private Dictionary<int,FrameLabelData> _frameLabels = new();

    public event Action<bool>? HeaderCollapseChanged;
    public Action? RequestRedraw;
    private int _dragFrame;
    private int _mouseFrameOffset;
    public bool IsEditing { get; private set; }
    public string TextEdit { get; set; } = "New label";
    public int TestInt { get; set; } = 20;
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
    //public LingoGfxCanvas Canvas => _canvas;
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
        _fixPanel.BackgroundColor = LingoColorList.Transparent;

        _canvasOpenCollapse = factory.CreateGfxCanvas("ScoreOpenCollapse", 20, gfxValues.LabelsBarHeight-2);
        DrawCollapseButton();
        _fixPanel.AddItem(_canvasOpenCollapse,10,4);



        // Scolling panel
        _scollingPanel = factory.CreatePanel("ScoreLabelsScollBarPanel");
        _scollingPanel.Height = gfxValues.LabelsBarHeight;
        _scollingPanel.Width = 500;
        //_canvas = factory.CreateGfxCanvas("ScoreLabelsBar", 500, _gfxValues.LabelsBarHeight);
        //_panel.AddItem(_canvas);

        _canvas = _scollingPanel.SetGfxCanvasAt("ScoreLabelsBar",0,0, 500, _gfxValues.LabelsBarHeight);
        _editField = _scollingPanel.SetInputTextAt(this,"ScoreTextEdit", 20,0, 100,x => x.TextEdit,50);
        (_btnApply,_btnApplyLayout) =_scollingPanel.SetButtonAt("ScoreBtnApplyLabelEdit", "Apply",0,0, () => CommitEdit(true), 50);
        (_btnCancel, _btnCancelLayout) =_scollingPanel.SetButtonAt("ScoreBtnCancelLabelEdit", "X",40,0, () => CommitEdit(false), 20);
        _btnApply.Visibility = false;
        _editField.Visibility = false;
        _btnCancel.Visibility = false;
    }

    
    public void Dispose()
    {
        if (_movie != null)
            _movie.FrameLabels.LabelsChanged -= OnFrameLabelsChanged;
        _canvas.Dispose();
        _editField.Dispose();
        _btnApply.Dispose();
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


        RequestRedrawIt();
        RequestRedraw?.Invoke();
    }

    public void OnResize(int width, int height)
    {
        _fixPanel.Width = width;
        _canvasOpenCollapse.X = width - 20;
    }

    public void ToggleCollapsed()
    {
        HeaderCollapsed = !HeaderCollapsed;
        HeaderCollapseChanged?.Invoke(HeaderCollapsed);
        DrawCollapseButton();
    }
    private void DrawCollapseButton()
    {
        _canvasOpenCollapse.Clear(LingoColorList.White);
        _canvasOpenCollapse.DrawRect(new LingoRect(0, 0, 12, 12), LingoColorList.Black, false, 1);
        var x = 1;
        var y = 1;
        LingoPoint[] pts = !HeaderCollapsed
            ? [new LingoPoint(x, 3), new LingoPoint(x + 8, 3), new LingoPoint(x + 4, 11)]
            : [new LingoPoint(10, y), new LingoPoint(10, y + 8), new LingoPoint(3, y + 4)];
        _canvasOpenCollapse.DrawPolygon(pts, LingoColorList.Black);
    }
   

    private void OnFrameLabelsChanged()
    {
        RequestRedrawIt();
    }

    private void RequestRedrawIt()
    {
        Draw();
        RequestRedraw?.Invoke();
    }


    public void Draw()
    {
        if (_movie == null) return;
        _canvas.Clear(LingoColorList.White);
        _canvas.DrawText(new LingoPoint(200, 0), "oooo");
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
            frameData.DrawOnCanvas(_canvas, _gfxValues.LabelsBarHeight,frameData.X, LingoColorList.DarkGray);

        // Draw dragging pointer
        if (_dragging && _activeFrameLabel != null)
        {
            var x = (_dragFrame - 1) * _gfxValues.FrameWidth;
            _activeFrameLabel.DrawOnCanvas(_canvas, _gfxValues.LabelsBarHeight, x, LingoColorList.Black);
        }
        Console.WriteLine("_dragFrame:" + _dragFrame + ":" + _activeFrameLabel?.Name);

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
        if (IsEditing)
            return;

        // first try by the key
        if (!_frameLabels.TryGetValue(mouseFrame, out var frameLabel))
        {
            // then try if click on label
            frameLabel = _frameLabels.Values.FirstOrDefault(x => x.IsFramePointInside(mouseFrame));
            if (frameLabel == null) return;
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
        RequestRedrawIt();
    }
    private void MouseMove(LingoMouseEvent e, int mouseFrame)
    {
        if (_movie == null || !_dragging || _activeFrameLabel == null) return;
        var newFrame = mouseFrame - _mouseFrameOffset;
        if (newFrame != _dragFrame)
        {
            _dragFrame = newFrame;
            //Console.WriteLine("_dragFrame:" + _dragFrame + ":" + _activeFrameLabel.Name);
            RequestRedrawIt();
        }
    }
    private void MouseUp(LingoMouseEvent e)
    {
        if (_dragging && _activeFrameLabel != null)
        {
            _commandManager.Handle(new UpdateFrameLabelCommand(_activeFrameLabel.Frame, _dragFrame, _activeFrameLabel.Name));
            _dragging = false;
            RequestRedrawIt();
        }
    }



    #endregion




    #region Edit Text
    private void StartEditing(FrameLabelData frameLabel)
    {
        TextEdit = frameLabel.Name;
        _activeFrameLabel = frameLabel;
        _dragging = false;
        _editField.Text = frameLabel.Name;
        _editField.Visibility = true;
        _btnApply.Visibility = true;
        _btnCancel.Visibility = true;
        IsEditing = true;
        float x = _gfxValues.LeftMargin + (_activeFrameLabel.Frame - 1) * _gfxValues.FrameWidth + 12;
        _editField.X = x;
        _editField.Y = 2;
        _btnApplyLayout.X =  x + _editField.Width + 2;
        _btnCancelLayout.X =  x + _editField.Width + 2 + _btnApply.Width + 2;
    }

    private void CommitEdit(bool saveChanges)
    {
        var newText = TextEdit;
        if (saveChanges && _activeFrameLabel != null && _activeFrameLabel.Name != newText)
            _commandManager.Handle(new UpdateFrameLabelCommand(_activeFrameLabel.Frame, _activeFrameLabel.Frame, newText));
        _activeFrameLabel = null;
        _editField.Visibility = false;
        _btnApply.Visibility = false;
        _btnCancel.Visibility = false;
        RequestRedrawIt();
        IsEditing = false;
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
        public void DrawOnCanvas(LingoGfxCanvas _canvas, int labelsBarHeight, float x, LingoColor color)
        {
            var pts = new[]
            {
                new LingoPoint(X,5),
                new LingoPoint(X+10,5),
                new LingoPoint(X+5,15)
            };
            _canvas.DrawRect(new LingoRect(X, 0, X + WidthPx, labelsBarHeight), LingoColorList.White, true);
            _canvas.DrawPolygon(pts, color);
            _canvas.DrawText(new LingoPoint(X + 12, 12), Name, null, color, 10);
        }
    }

}
