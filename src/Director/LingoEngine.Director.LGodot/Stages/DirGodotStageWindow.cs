using Godot;
using LingoEngine.Movies;
using LingoEngine.LGodot.Stages;
using LingoEngine.Core;
using LingoEngine.Director.Core.Stages;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Director.Core.UI;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Inputs;
using LingoEngine.Director.Core.Icons;
using AbstUI.Primitives;

namespace LingoEngine.Director.LGodot.Movies;

internal partial class DirGodotStageWindow : BaseGodotWindow, IDirFrameworkStageWindow
{
    private const int IconBarHeight = 12;
    private readonly LingoGodotStageContainer _stageContainer;
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoPlayer _player;
    private readonly ColorRect _stageBgRect = new ColorRect();
    private readonly ColorRect _stageWindowBgRect = new ColorRect();
    private readonly ScrollContainer _scrollContainer = new ScrollContainer();
    private readonly SelectionBox _selectionBox = new SelectionBox();
    private readonly StageBoundingBoxesOverlay _boundingBoxes;
    private readonly StageSpriteSummaryOverlay _spriteSummary;
    private readonly StageMotionPathOverlay _motionPath;
    private readonly DirectorStageGuides _guides;
    private readonly IDirectorEventSubscription _stageChangedSubscription;

    private LingoMovie? _movie;
    private ILingoFrameworkStage? _stage;
    private readonly DirStageManager _stageManager;
    private readonly DirectorStageWindow _directorStageWindow;
    private readonly StageIconBar _iconBar;
    private readonly Node2D _stageLayer;
    private bool _spaceHeld;
    private bool _panning;
    private float _scale = 1f;
    private LingoGodotGfxCanvas _spriteSummaryCanvas;
    private LingoGodotGfxCanvas _boundingBoxesCanvas;
    private LingoGodotGfxCanvas _motionPathCanvas;
    private LingoGodotGfxCanvas _guidesCanvas;

    public DirGodotStageWindow(ILingoFrameworkStageContainer stageContainer, IDirectorEventMediator directorEventMediator, IHistoryManager historyManager, ILingoPlayer player, DirectorStageWindow directorStageWindow, DirectorStageGuides guides, DirStageManager stageManager, IDirGodotWindowManager windowManager, IDirectorIconManager iconManager)
        : base(DirectorMenuCodes.StageWindow, "Stage", windowManager, historyManager)
    {
        // TempFix
        //base.DontUseInputInsteadOfGuiInput();


        _stageContainer = (LingoGodotStageContainer)stageContainer;
        _mediator = directorEventMediator;
        _player = player;
        _player.ActiveMovieChanged += OnActiveMovieChanged;
        directorStageWindow.Init(this);
        _directorStageWindow = directorStageWindow;
        _iconBar = directorStageWindow.IconBar;
        _iconBar.ZoomChanged += SetScale;
        _iconBar.ColorChanged += OnColorChanged;
        BackgroundColor = AColors.Transparent;
        _stageManager = stageManager;
        _stageManager.SelectionChanged += OnStageSelectionChanged;
        _stageManager.SpritesTransformed += OnStageSpritesTransformed;
        _stageChangedSubscription = _mediator.Subscribe(DirectorEventType.StagePropertiesChanged, StagePropertyChanged);

        var lp = (LingoPlayer)_player;
        ((LingoStageMouse)lp.Mouse).ReplaceFrameworkObj(_MouseFrameworkObj);


        _spriteSummary = new StageSpriteSummaryOverlay(lp.Factory, _mediator, iconManager);
        _boundingBoxes = new StageBoundingBoxesOverlay(lp.Factory, _mediator);
        _motionPath = new StageMotionPathOverlay(lp.Factory);
        _guides = guides;
        _guides.Draw();

        Size = new Vector2(640 + 10, 480 + 5 + IconBarHeight + TitleBarHeight);
        CustomMinimumSize = Size;
        // Give all nodes clear names for easier debugging
        Name = "DirGodotStageWindow";
        _scrollContainer.Name = "StageScrollContainer";
        _stageBgRect.Name = "StageBackgroundRect";
        _stageWindowBgRect.Name = "StageWindowBackgroundRect";
        _stageContainer.Container.Name = "StageContainer";

        const int zIndexStageStart = DirGodotWindowManager.ZIndexInactiveWindow + 1000;

        // Bg color for stage
        _stageWindowBgRect.Color = Colors.DarkGray;
        _stageWindowBgRect.CustomMinimumSize = Size;
        _stageWindowBgRect.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _stageWindowBgRect.SizeFlagsVertical = SizeFlags.ExpandFill;
        _stageWindowBgRect.ZIndex = zIndexStageStart - 2; // Ensure it is behind everything else
        
        AddChild(_stageWindowBgRect);

        // Bg solid color for stage
        _stageBgRect.Color = Colors.Black;
        _stageBgRect.CustomMinimumSize = new Vector2(640, 480);
        //_stageBgRect.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        //_stageBgRect.SizeFlagsVertical = SizeFlags.ExpandFill;
        _stageBgRect.ZIndex = zIndexStageStart - 1; // Ensure it is behind everything else

        _spriteSummaryCanvas = _spriteSummary.Canvas.Framework<LingoGodotGfxCanvas>();
        _boundingBoxesCanvas = _boundingBoxes.Canvas.Framework<LingoGodotGfxCanvas>();
        _motionPathCanvas = _motionPath.Canvas.Framework<LingoGodotGfxCanvas>();
        _guidesCanvas = _guides.Canvas.Framework<LingoGodotGfxCanvas>();
        _spriteSummaryCanvas.ZIndex = 1000;
        _boundingBoxesCanvas.ZIndex = 1001;
        _motionPathCanvas.ZIndex = 1000;
        _guidesCanvas.ZIndex = 999;
        _selectionBox.ZIndex = 1002;
        _selectionBox.Visible = false;
        _stageContainer.Container.ZIndex = zIndexStageStart;

        // canvas layer to keep all childs behind
        _stageLayer = new Node2D();
        _stageLayer.Name = "StageLayer";
        _stageLayer.AddChild(_stageBgRect);
        _stageLayer.AddChild(_stageContainer.Container);
        _stageLayer.AddChild(_guidesCanvas);
        _stageLayer.AddChild(_motionPathCanvas);
        _stageLayer.AddChild(_spriteSummaryCanvas);
        _stageLayer.AddChild(_selectionBox);


        var wrapper = new Control();
        wrapper.Name = "StageWrapper";
        wrapper.CustomMinimumSize = new Vector2(3000, 2000);
        wrapper.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        wrapper.SizeFlagsVertical = SizeFlags.ShrinkBegin;
        wrapper.AddChild(_stageLayer);
        _scrollContainer.AddChild(wrapper);
        //_boundingBoxes.MouseFilter = MouseFilterEnum.Ignore; // ensure mouse clicks pass through


        CreateScrollContainer();
        AddChild(_scrollContainer);

        // bottom icon bar
        var iconBarPanel = _iconBar.Panel.Framework<LingoGodotPanel>();
        AddChild(iconBarPanel);
        CreateBottomIconBar(iconBarPanel);



    }



    private void CreateScrollContainer()
    {
        // Set anchors to stretch fully
        _scrollContainer.Position = new Vector2(0, 20);
        _scrollContainer.AnchorLeft = 0;
        _scrollContainer.AnchorTop = 0;
        _scrollContainer.AnchorRight = 1;
        _scrollContainer.AnchorBottom = 1;

        // Set offsets to 0
        _scrollContainer.OffsetLeft = 0;
        _scrollContainer.OffsetTop = TitleBarHeight;
        _scrollContainer.OffsetRight = 0;
        _scrollContainer.OffsetBottom = -IconBarHeight - 10;
        _scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _scrollContainer.VerticalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        //_scrollContainer.ZIndex = 500;
    }

    private void CreateBottomIconBar(LingoGodotPanel iconBarPanel)
    {
        iconBarPanel.AnchorLeft = 0;
        iconBarPanel.AnchorRight = 1;
        iconBarPanel.AnchorTop = 1;
        iconBarPanel.AnchorBottom = 1;
        iconBarPanel.OffsetLeft = 0;
        iconBarPanel.OffsetRight = 0;
        iconBarPanel.OffsetTop = -TitleBarHeight;
        iconBarPanel.OffsetBottom = 0;
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _stageWindowBgRect.CustomMinimumSize = size;
    }
    public override void SetSize(int width, int height)
    {
        base.SetSize(width, height);
        _stageWindowBgRect.CustomMinimumSize = new Vector2(width,height);
        StagePropertyChanged();
    }
    public void SetStage(ILingoFrameworkStage stage)
    {
        _stage = stage;
        if (stage is Node node)
        {

            if (node.GetParent() != this)
            {
                node.GetParent()?.RemoveChild(node);
                AddChild(node);
            }
            if (node is Node2D node2D)
                node2D.Position = new Vector2(0, TitleBarHeight);
        }
    }

    public void SetActiveMovie(LingoMovie? movie)
    {
        if (_movie != null)
        {
            _movie.PlayStateChanged -= OnPlayStateChanged;
            _movie.Sprite2DListChanged -= SpriteListChanged;
        }

        _stage?.SetActiveMovie(movie);
        _movie = movie;
        _stageManager.ClearSelection();
        _selectionBox.Visible = false;

        if (_movie != null)
        {
            _movie.PlayStateChanged += OnPlayStateChanged;
            _movie.Sprite2DListChanged += SpriteListChanged;
            var env = _movie.GetEnvironment();
            _boundingBoxes.SetInput(env.Mouse, env.Key);
        }

        _iconBar.SetActiveMovie(movie);
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    private void OnActiveMovieChanged(ILingoMovie? movie)
    {
        SetActiveMovie(movie as LingoMovie);
        UpdateStagePosition();
    }

    private void OnPlayStateChanged(bool isPlaying)
    {
        UpdateBoundingBoxes();
        UpdateMotionPath();
        if (isPlaying)
        {
            _selectionBox.Visible = false;
        }
        else if (_stageManager.SelectedSprites.Count > 0)
            UpdateSelectionBox();
    }

    private void OnStageSelectionChanged()
    {
        if (_movie != null && !_movie.IsPlaying && _stageManager.SelectedSprites.Count > 0)
            UpdateSelectionBox();
        else
            _selectionBox.Visible = false;
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    private void OnStageSpritesTransformed()
    {
        UpdateSelectionBox();
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }



    private void OnColorChanged(AColor color)
    {
        _stageManager.ChangeBackgroundColor(color);
    }
    private bool StagePropertyChanged()
    {
        var color = _player.Stage.BackgroundColor;
        _stageBgRect.Color = color.ToGodotColor();
        _stageBgRect.CustomMinimumSize = new Vector2(_player.Stage.Width, _player.Stage.Height);
        var innerPos = new Vector2(40, 30);
        _stageContainer.Container.Position = innerPos; //, TitleBarHeight+10);
        _guidesCanvas.Position = innerPos;
        _motionPathCanvas.Position = innerPos;
        _spriteSummaryCanvas.Position = innerPos;
        _selectionBox.Position = innerPos;
        UpdateStagePosition();
        return true;
    }
    private void UpdateStagePosition()
    {
        
        _stageLayer.Position = new Vector2((3000 - _player.Stage.Width) / 2f, (2000 - _player.Stage.Height) / 2f);

        _scrollContainer.ScrollHorizontal = 3000 / 2 - (int)_scrollContainer.Size.X / 2;
        _scrollContainer.ScrollVertical = 2000 / 2 - (int)_scrollContainer.Size.Y / 2;
        SetScale(1);
        //var min = _scrollContainer.GetVScrollBar().MinValue;
        //var max = _scrollContainer.GetVScrollBar().MaxValue;
        //var range = max - min;
    }
    private void SetScale(float scale)
    {
        _iconBar.SetZoom(scale);
        _scale = scale;
        _stageLayer.SetScale(new Vector2(scale, scale));
    }

    private void OnToolChanged(StageTool tool)
    {
        switch (tool)
        {
            case StageTool.Pointer:
                Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
                break;
            case StageTool.Move:
                Input.SetDefaultCursorShape(Input.CursorShape.Move);
                break;
            case StageTool.Rotate:
                Input.SetDefaultCursorShape(Input.CursorShape.Cross);
                break;
        }
    }

    public void UpdateSelectionBox()
    {
        if (_stageManager.SelectedSprites.Count == 0)
        {
            _selectionBox.Visible = false;
            return;
        }
        var rect = _stageManager.ComputeSelectionRect().ToRect2();
        _selectionBox.UpdateRect(rect);
        _selectionBox.Visible = true;
    }

    public void SpriteListChanged(int spriteNumWithChannelNum)
    {
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }
    public void UpdateBoundingBoxes()
    {
        if (_movie == null || _movie.IsPlaying)
        {
            _boundingBoxes.Visible = false;
            _spriteSummary.Visible = false;
            return;
        }

        if (_stageManager.SelectedSprites.Count > 0)
        {
            _boundingBoxes.SetSprites(_stageManager.SelectedSprites);
            _boundingBoxes.Visible = true;
            _spriteSummary.Visible = true;
        }
        else
        {
            _boundingBoxes.Visible = false;
            _spriteSummary.Visible = false;
        }
    }

    public void UpdateMotionPath()
    {
        if (_movie == null || _movie.IsPlaying)
        {
            _motionPath.Draw(null);
            return;
        }

        var sprite = _stageManager.PrimarySelectedSprite;
        var path = sprite != null ? _stageManager.GetMotionPath(sprite) : null;
        _motionPath.Draw(path);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (!Visible || _movie == null || _movie.IsPlaying || !IsActiveWindow) return;

        if (@event is InputEventKey spaceKey && spaceKey.Keycode == Key.Space)
        {
            _spaceHeld = spaceKey.Pressed;
            if (!spaceKey.Pressed)
                _panning = false;
            return;
        }
        else if (@event is InputEventMouseButton mb)
        {
            Vector2 mousePos = GetGlobalMousePosition();
            Rect2 bounds = new Rect2(_scrollContainer.GlobalPosition, _scrollContainer.Size);
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed && _spaceHeld && bounds.HasPoint(mousePos))
                {
                    _panning = true;
                    GetViewport().SetInputAsHandled();
                    return;
                }
                else if (!mb.Pressed && _panning)
                {
                    _panning = false;
                    GetViewport().SetInputAsHandled();
                    return;
                }
            }
            else if (!mb.Pressed && (mb.ButtonIndex == MouseButton.WheelUp || mb.ButtonIndex == MouseButton.WheelDown) && bounds.HasPoint(mousePos))
            {
                float delta = mb.ButtonIndex == MouseButton.WheelUp ? 0.1f : -0.1f;
                float newScale = Mathf.Clamp(_scale + delta, _iconBar.MinZoom, _iconBar.MaxZoom);
                SetScale(newScale);
                //_stageContainer.SetScale(newScale);
                GetViewport().SetInputAsHandled();
                return;
            }
        }
        else if (@event is InputEventMouseMotion motion && _panning)
        {
            _scrollContainer.ScrollHorizontal -= (int)motion.Relative.X;
            _scrollContainer.ScrollVertical -= (int)motion.Relative.Y;
            GetViewport().SetInputAsHandled();
            return;
        }

        switch (_directorStageWindow.SelectedTool)
        {
            case StageTool.Pointer:
                HandlePointerInput(@event);
                break;
            case StageTool.Move:
                HandleMoveInput(@event);
                break;
            case StageTool.Rotate:
                HandleRotateInput(@event);
                break;
        }
    }

    private void HandlePointerInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            Vector2 localPos = _stageContainer.Container.ToLocal(mb.Position);
            if (_movie == null) return;
            var sprite = _movie.GetSpriteAtPoint(localPos.X, localPos.Y, skipLockedSprites: true) as LingoSprite2D;
            if (mb.Pressed && sprite != null)
            {
                _stageManager.HandlePointerClick(sprite, Input.IsKeyPressed(Key.Ctrl));
            }
        }
    }

    private void HandleMoveInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            var p = new APoint(mb.Position.X, mb.Position.Y);
            if (mb.Pressed)
                _stageManager.BeginMove(p);
            else
                _stageManager.EndMove(p);
        }
        else if (@event is InputEventMouseMotion motion)
        {
            var p = new APoint(motion.Position.X, motion.Position.Y);
            _stageManager.UpdateMove(p);
        }
    }

    private void HandleRotateInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            var p = new APoint(mb.Position.X, mb.Position.Y);
            if (mb.Pressed)
                _stageManager.BeginRotate(p);
            else
                _stageManager.EndRotate(p);
        }
        else if (@event is InputEventMouseMotion motion)
        {
            var p = new APoint(motion.Position.X, motion.Position.Y);
            _stageManager.UpdateRotate(p);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_movie != null)
        {
            _movie.PlayStateChanged -= OnPlayStateChanged;
            _movie.Sprite2DListChanged -= SpriteListChanged;
        }
        _stageChangedSubscription.Release();
        _player.ActiveMovieChanged -= OnActiveMovieChanged;
        _stageManager.SelectionChanged -= OnStageSelectionChanged;
        _stageManager.SpritesTransformed -= OnStageSpritesTransformed;
        _spriteSummary.Dispose();
        base.Dispose(disposing);
    }

    private partial class SelectionBox : Node2D
    {
        public SelectionBox()
        {
            Name = "SelectionBox";
        }
        private Rect2 _rect;
        public void UpdateRect(Rect2 rect)
        {
            _rect = rect;
            QueueRedraw();
        }

        public override void _Draw()
        {
            DrawRect(_rect, Colors.Yellow, false, 1);
        }
    }


}
