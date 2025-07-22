using Godot;
using LingoEngine.Movies;
using LingoEngine.LGodot.Stages;
using LingoEngine.Core;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Stages;
using System.Linq;
using System.Collections.Generic;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Texts;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.Core.Stages.Commands;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Sprites;
using LingoEngine.Movies.Commands;
using LingoEngine.Stages;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.UI;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Inputs;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Styles;

namespace LingoEngine.Director.LGodot.Movies;

internal partial class DirGodotStageWindow : BaseGodotWindow, IHasSpriteSelectedEvent, IDirFrameworkStageWindow
{
    private const int IconBarHeight = 12;
    private readonly LingoGodotStageContainer _stageContainer;
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoPlayer _player;
    private readonly ILingoCommandManager _commandManager;
    private readonly IHistoryManager _historyManager;
    private readonly HBoxContainer _iconBar = new HBoxContainer();
    private readonly HSlider _zoomSlider = new HSlider();
    private readonly OptionButton _zoomDropdown = new OptionButton();
    private readonly Button _rewindButton = new Button();
    private readonly Button _playButton = new Button();
    private readonly Button _prevFrameButton = new Button();
    private readonly Button _nextFrameButton = new Button();
    private readonly Button _recordButton = new Button();
    private readonly ColorRect _stageBgRect = new ColorRect();
    private readonly ColorRect _colorDisplay = new ColorRect();
    private readonly ColorPickerButton _colorPicker = new ColorPickerButton();
    private readonly ScrollContainer _scrollContainer = new ScrollContainer();
    private readonly SelectionBox _selectionBox = new SelectionBox();
    private readonly StageBoundingBoxesOverlay _boundingBoxes;
    private readonly StageSpriteSummaryOverlay _spriteSummary;
    private readonly IDirectorEventSubscription _stageChangedSubscription;

    private LingoMovie? _movie;
    private ILingoFrameworkStage? _stage;
    private readonly List<LingoSprite> _selectedSprites = new();
    private readonly DirectorStageWindow _directorStageWindow;
    private LingoSprite? _primarySelectedSprite;
    private Vector2? _dragStart;
    private Dictionary<LingoSprite, Primitives.LingoPoint>? _initialPositions;
    private Dictionary<LingoSprite, float>? _initialRotations;
    private bool _rotating;
    private bool _spaceHeld;
    private bool _panning;
    private float _scale = 1f;

    public DirGodotStageWindow(ILingoFrameworkStageContainer stageContainer, IDirectorEventMediator directorEventMediator, ILingoCommandManager commandManager, IHistoryManager historyManager, ILingoPlayer player, DirectorStageWindow directorStageWindow, IDirGodotWindowManager windowManager, IDirectorIconManager iconManager)
        : base(DirectorMenuCodes.StageWindow, "Stage", windowManager)
    {
        _stageContainer = (LingoGodotStageContainer)stageContainer;
        _mediator = directorEventMediator;
        _player = player;
        _player.ActiveMovieChanged += OnActiveMovieChanged;
        _commandManager = commandManager;
        _historyManager = historyManager;
        directorStageWindow.Init(this);
        _directorStageWindow = directorStageWindow;
        BackgroundColor = DirectorColors.BG_WhiteMenus;
        _mediator.Subscribe(this);
        _stageChangedSubscription = _mediator.Subscribe(DirectorEventType.StagePropertiesChanged, StagePropertyChanged);
        var lp = _player as LingoPlayer;
        if (lp != null)
        {
            _spriteSummary = new StageSpriteSummaryOverlay(lp.Factory, _mediator, iconManager);
            _boundingBoxes = new StageBoundingBoxesOverlay(lp.Factory, _mediator);
        }
        else
        {
            var p = (LingoPlayer)_player!;
            _spriteSummary = new StageSpriteSummaryOverlay(p.Factory, _mediator, iconManager);
            _boundingBoxes = new StageBoundingBoxesOverlay(p.Factory, _mediator);
        }



        Size = new Vector2(640 + 10, 480 + 5 + IconBarHeight + TitleBarHeight);
        CustomMinimumSize = Size;
        // Give all nodes clear names for easier debugging
        Name = "DirGodotStageWindow";
        _scrollContainer.Name = "StageScrollContainer";
        _stageBgRect.Name = "StageBackgroundRect";
        _stageContainer.Container.Name = "StageContainer";
        _iconBar.Name = "IconBar";
        _zoomSlider.Name = "ZoomSlider";
        _zoomDropdown.Name = "ZoomDropdown";
        _rewindButton.Name = "RewindButton";
        _playButton.Name = "PlayButton";
        _prevFrameButton.Name = "PrevFrameButton";
        _nextFrameButton.Name = "NextFrameButton";
        _recordButton.Name = "RecordButton";
        _colorDisplay.Name = "ColorDisplay";
        _colorPicker.Name = "ColorPicker";
        // Set anchors to stretch fully
        _scrollContainer.AnchorLeft = 0;
        _scrollContainer.AnchorTop = 0;
        _scrollContainer.AnchorRight = 1;
        _scrollContainer.AnchorBottom = 1;

        // Set offsets to 0
        _scrollContainer.OffsetLeft = 0;
        _scrollContainer.OffsetTop = 0;
        _scrollContainer.OffsetRight = -10;
        _scrollContainer.OffsetBottom = -IconBarHeight - 5-50;


        _scrollContainer.Position = new Vector2(0, 20);
        _stageBgRect.Color = Colors.Black;
        _stageBgRect.CustomMinimumSize = new Vector2(640, 480);
        _stageBgRect.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _stageBgRect.SizeFlagsVertical = SizeFlags.ExpandFill;
        _scrollContainer.AddChild(_stageBgRect);
        _scrollContainer.AddChild(_stageContainer.Container);
        _stageContainer.Container.AddChild(_boundingBoxes.Canvas.Framework<LingoGodotGfxCanvas>());
        _stageContainer.Container.AddChild(_selectionBox);
        _stageContainer.Container.AddChild(_spriteSummary.Canvas.Framework<LingoGodotGfxCanvas>());
        //_boundingBoxes.Canvas.Framework<LingoGodotGfxCanvas>().ZIndex = 500;
        //_spriteSummary.Canvas.Framework<LingoGodotGfxCanvas>().ZIndex = 750;
        //_boundingBoxes.MouseFilter = MouseFilterEnum.Ignore; // ensure mouse clicks pass through
        _selectionBox.Visible = false;
        //_selectionBox.ZIndex = 1000;
        AddChild(_scrollContainer);

        // bottom icon bar
        AddChild(_iconBar);
        CreateBottomIconBar();

        UpdatePlayButton();
    }

    private void CreateBottomIconBar()
    {
        _iconBar.AnchorLeft = 0;
        _iconBar.AnchorRight = 1;
        _iconBar.AnchorTop = 1;
        _iconBar.AnchorBottom = 1;
        _iconBar.OffsetLeft = 0;
        _iconBar.OffsetRight = 0;
        _iconBar.OffsetTop = 0;// -IconBarHeight;
        _iconBar.OffsetBottom = 0;
       

        _rewindButton.Text = "|<";
        _rewindButton.CustomMinimumSize = new Vector2(20, IconBarHeight);
        _rewindButton.Pressed += () => _commandManager.Handle(new RewindMovieCommand());
        _iconBar.AddChild(_rewindButton);

        _playButton.CustomMinimumSize = new Vector2(60, IconBarHeight);
        _playButton.AddThemeFontSizeOverride("font_size", 8);
        _playButton.Pressed += () => _commandManager.Handle(new PlayMovieCommand());
        _iconBar.AddChild(_playButton);

        _prevFrameButton.Text = "<";
        _prevFrameButton.CustomMinimumSize = new Vector2(20, IconBarHeight);
        _prevFrameButton.Pressed += () => _commandManager.Handle(new StepFrameCommand(-1));
        _iconBar.AddChild(_prevFrameButton);

        _nextFrameButton.Text = ">";
        _nextFrameButton.CustomMinimumSize = new Vector2(20, IconBarHeight);
        _nextFrameButton.Pressed += () => _commandManager.Handle(new StepFrameCommand(1));
        _iconBar.AddChild(_nextFrameButton);

        _zoomSlider.MinValue = 0.5f;
        _zoomSlider.MaxValue = 1.5f;
        _zoomSlider.Step = 0.1f;
        _zoomSlider.Value = 1f;
        _zoomSlider.CustomMinimumSize = new Vector2(150, IconBarHeight);
        _zoomSlider.ValueChanged += value =>
        {
            float scale = (float)value;
            _scale = scale;
            UpdateScaleDropdown(scale);
            _stageContainer.SetScale(scale);
        };
        _iconBar.AddChild(_zoomSlider);

        for (int i = 50; i <= 150; i += 10)
            _zoomDropdown.AddItem($"{i}%");
        _zoomDropdown.Select(5); // 100%
        _zoomDropdown.CustomMinimumSize = new Vector2(60, IconBarHeight);
        _zoomDropdown.ItemSelected += id =>
        {
            float scale = (50 + id * 10) / 100f;
            _zoomSlider.Value = scale;
            _scale = scale;
            _stageContainer.SetScale(scale);
        };
        _iconBar.AddChild(_zoomDropdown);

        _colorDisplay.Color = Colors.Black;
        _colorDisplay.CustomMinimumSize = new Vector2(IconBarHeight, IconBarHeight);
        _iconBar.AddChild(_colorDisplay);

        _colorPicker.CustomMinimumSize = new Vector2(IconBarHeight, IconBarHeight);
        _colorPicker.Color = Colors.Black;
        _colorPicker.ColorChanged += c => OnColorChanged(c);
        _iconBar.AddChild(_colorPicker);

        _recordButton.Text = "●";
        _recordButton.ToggleMode = true;
        _recordButton.CustomMinimumSize = new Vector2(IconBarHeight, IconBarHeight);
        _recordButton.AddThemeColorOverride("font_color", Colors.Red);
        _recordButton.Toggled += pressed =>
        {
            if (_player is LingoPlayer lp)
                lp.Stage.RecordKeyframes = pressed;
        };
        _iconBar.AddChild(_recordButton);
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
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
            _movie.SpriteListChanged -= UpdateBoundingBoxes;
        }

        _stage?.SetActiveMovie(movie);
        _movie = movie;
        _selectedSprites.Clear();
        _primarySelectedSprite = null;
        _selectionBox.Visible = false;

        if (_movie != null)
        {
            _movie.PlayStateChanged += OnPlayStateChanged;
            _movie.SpriteListChanged += UpdateBoundingBoxes;
            var env = _movie.GetEnvironment();
            _boundingBoxes.SetInput(env.Mouse, env.Key);
        }

        UpdatePlayButton();
        UpdateBoundingBoxes();
    }

    private void OnActiveMovieChanged(ILingoMovie? movie)
    {
        SetActiveMovie(movie as LingoMovie);
    }

    private void OnPlayStateChanged(bool isPlaying)
    {
        UpdatePlayButton();
        UpdateBoundingBoxes();
        if (isPlaying)
        {
            _selectionBox.Visible = false;
        }
        else if (_selectedSprites.Count > 0)
            UpdateSelectionBox();
    }

    private void UpdatePlayButton()
    {
        _playButton.Text = _movie != null && _movie.IsPlaying ? "stop !" : "Play >";
    }


    private void OnColorChanged(Color color)
    {
        _stageBgRect.Color = color;
        _colorDisplay.Color = color;
        _player.Stage.BackgroundColor = color.ToLingoColor();
    }
    private bool StagePropertyChanged()
    {
        _stageBgRect.Color = _player.Stage.BackgroundColor.ToGodotColor();
        _stageBgRect.CustomMinimumSize = new Vector2(_player.Stage.Width, _player.Stage.Height);
        return true;
    }

    private void UpdateScaleDropdown(float value)
    {
        int percent = (int)Mathf.Round(value * 100);
        int index = (percent - 50) / 10;
        if (index >= 0 && index < _zoomDropdown.ItemCount)
            _zoomDropdown.Select(index);
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

    public void SpriteSelected(ILingoSprite sprite)
    {
        _selectedSprites.Clear();
        if (!(sprite is LingoSprite ls))
            return;
        _selectedSprites.Add(ls);
        _primarySelectedSprite = ls;
        if (_movie != null && !_movie.IsPlaying && ls != null)
            UpdateSelectionBox();
    }

    public void UpdateSelectionBox()
    {
        if (_selectedSprites.Count == 0)
        {
            _selectionBox.Visible = false;
            return;
        }
        float left = _selectedSprites.Min(s => s.Rect.Left);
        float top = _selectedSprites.Min(s => s.Rect.Top);
        float right = _selectedSprites.Max(s => s.Rect.Right);
        float bottom = _selectedSprites.Max(s => s.Rect.Bottom);
        var rect = new Rect2(left, top, right - left, bottom - top);
        _selectionBox.UpdateRect(rect);
        _selectionBox.Visible = true;
    }

    public void UpdateBoundingBoxes()
    {
        if (_movie == null || _movie.IsPlaying)
        {
            _boundingBoxes.Visible = false;
            _spriteSummary.Visible = false;
            return;
        }

        if (_selectedSprites.Count > 0)
        {
            _boundingBoxes.SetSprites(_selectedSprites);
            _boundingBoxes.Visible = true;
            _spriteSummary.Visible = true;
        }
        else
        {
            _boundingBoxes.Visible = false;
            _spriteSummary.Visible = false;
        }
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
                float newScale = Mathf.Clamp(_scale + delta, (float)_zoomSlider.MinValue, (float)_zoomSlider.MaxValue);
                _zoomSlider.Value = newScale;
                _scale = newScale;
                UpdateScaleDropdown(newScale);
                _stageContainer.SetScale(newScale);
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

        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Z && key.CtrlPressed)
        {
            _historyManager.Undo();
            return;
        }
        if (@event is InputEventKey key2 && key2.Pressed && key2.Keycode == Key.Y && key2.CtrlPressed)
        {
            _historyManager.Redo();
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
            var sprite = _movie.GetSpriteAtPoint(localPos.X, localPos.Y, skipLockedSprites: true) as LingoSprite;
            if (mb.Pressed)
            {
                if (sprite != null)
                {
                    if (Input.IsKeyPressed(Key.Ctrl))
                    {
                        if (_selectedSprites.Contains(sprite))
                            _selectedSprites.Remove(sprite);
                        else
                            _selectedSprites.Add(sprite);
                        UpdateSelectionBox();
                        UpdateBoundingBoxes();
                    }
                    else
                    {
                        _selectedSprites.Clear();
                        _selectedSprites.Add(sprite);
                        _mediator.RaiseSpriteSelected(sprite);
                        UpdateSelectionBox();
                        UpdateBoundingBoxes();
                    }
                }
            }
        }
    }

    private void HandleMoveInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                if (_selectedSprites.Count > 0)
                {
                    _dragStart = mb.Position;
                    _initialPositions = _selectedSprites.ToDictionary(s => s, s => new Primitives.LingoPoint(s.LocH, s.LocV));
                    if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                        foreach (var s in _selectedSprites)
                            lp.Stage.AddKeyFrame(s);
                }
            }
            else if (_dragStart.HasValue && _initialPositions != null)
            {
                var end = _selectedSprites.ToDictionary(s => s, s => new Primitives.LingoPoint(s.LocH, s.LocV));
                _commandManager.Handle(new MoveSpritesCommand(_initialPositions, end));
                _dragStart = null;
                _initialPositions = null;
                if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                    foreach (var s in _selectedSprites)
                        lp.Stage.UpdateKeyFrame(s);
            }
        }
        else if (@event is InputEventMouseMotion motion && _dragStart.HasValue && _initialPositions != null)
        {
            Vector2 delta = motion.Position - _dragStart.Value;
            foreach (var s in _selectedSprites)
            {
                var start = _initialPositions[s];
                s.LocH = start.X + delta.X;
                s.LocV = start.Y + delta.Y;
                if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                    lp.Stage.UpdateKeyFrame(s);
            }
            UpdateSelectionBox();
            UpdateBoundingBoxes();
        }
    }

    private void HandleRotateInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                if (_selectedSprites.Count > 0)
                {
                    _dragStart = mb.Position;
                    _initialRotations = _selectedSprites.ToDictionary(s => s, s => s.Rotation);
                    _rotating = true;
                    if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                        foreach (var s in _selectedSprites)
                            lp.Stage.AddKeyFrame(s);
                }
            }
            else if (_rotating && _initialRotations != null)
            {
                var end = _selectedSprites.ToDictionary(s => s, s => s.Rotation);
                _commandManager.Handle(new RotateSpritesCommand(_initialRotations, end));
                _rotating = false;
                _dragStart = null;
                _initialRotations = null;
                if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                    foreach (var s in _selectedSprites)
                        lp.Stage.UpdateKeyFrame(s);
            }
        }
        else if (@event is InputEventMouseMotion motion && _rotating && _initialRotations != null && _dragStart.HasValue)
        {
            Vector2 center = ComputeSelectionCenter();
            float startAngle = (_dragStart.Value - center).Angle();
            float currentAngle = (motion.Position - center).Angle();
            float delta = Mathf.RadToDeg(currentAngle - startAngle);
            foreach (var s in _selectedSprites)
            {
                s.Rotation = _initialRotations[s] + delta;
                if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                    lp.Stage.UpdateKeyFrame(s);
            }
            UpdateSelectionBox();
            UpdateBoundingBoxes();
        }
    }

    private Vector2 ComputeSelectionCenter()
    {
        if (_selectedSprites.Count == 0) return Vector2.Zero;
        float left = _selectedSprites.Min(s => s.Rect.Left);
        float top = _selectedSprites.Min(s => s.Rect.Top);
        float right = _selectedSprites.Max(s => s.Rect.Right);
        float bottom = _selectedSprites.Max(s => s.Rect.Bottom);
        return new Vector2((left + right) / 2f, (top + bottom) / 2f);
    }

  

    protected override void Dispose(bool disposing)
    {
        if (_movie != null)
        {
            _movie.PlayStateChanged -= OnPlayStateChanged;
            _movie.SpriteListChanged -= UpdateBoundingBoxes;
        }
        _stageChangedSubscription.Release();
        _player.ActiveMovieChanged -= OnActiveMovieChanged;
        _mediator.Unsubscribe(this);
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
