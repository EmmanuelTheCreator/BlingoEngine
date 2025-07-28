using Godot;
using LingoEngine.Movies;
using LingoEngine.Core;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Director.Core.Stages.Commands;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.UI;
using System.Net.Http.Headers;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Simple timeline overlay showing the Score channels and frames.
/// Toggled with F2.
/// </summary>
public partial class DirGodotScoreWindow : BaseGodotWindow, IDirFrameworkScoreWindow,
    ICommandHandler<ChangeSpriteRangeCommand>,
    ICommandHandler<AddSpriteCommand>,
    ICommandHandler<RemoveSpriteCommand>
{
   
    
    private int _footerMargin= 10;

    private bool wasToggleKey;
    private LingoMovie? _movie;
    private readonly ScrollContainer _leftChannelsScollClipper = new();
    private readonly ScrollContainer _masterScroller = new ScrollContainer();
    private readonly Control _topStripContent = new Control();
    private readonly Control _scrollContent = new Control();
    private ColorRect _topHClipper;
    private LingoPlayer _player;
    private readonly DirScoreGfxValues _gfxValues = new();
    private readonly DirGodotScoreGrid _grid;
    internal LingoSprite2D? SelectedSprite => _grid.SelectedSprite;
    private readonly DirGodotFrameHeader _framesHeader;
    private readonly DirGodotFrameScriptsBar _frameScripts;
    private readonly DirGodotTopHeaderContainer _topHeaders;
    private readonly DirGodotTopGridContainer _topGrids;
    private readonly DirGodotScoreLabelsBar _labelBar;
    private readonly DirGodotScoreLeftChannelHeaders _leftChannelsHeaders;
    private readonly CollapseButton _collapseButton;
    private readonly DirGodotScoreLeftForLabels _leftChannelForLabels;
    private readonly DirGodotScoreLeftForLabels _leftHeaderForFrames;
    private bool _topCollapsed = true;

    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoCommandManager _commandManager;
    private readonly IHistoryManager _historyManager;


    public DirGodotScoreWindow(IDirectorEventMediator directorMediator, ILingoCommandManager commandManager, IHistoryManager historyManager, DirectorScoreWindow directorScoreWindow, ILingoPlayer player, IDirGodotWindowManager windowManager)
        : base(DirectorMenuCodes.ScoreWindow, "Score", windowManager)
    {
        var _marginContainer = new Control();
        _marginContainer.Position = new Vector2(0, TitleBarHeight);
        _marginContainer.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_marginContainer);
        _mediator = directorMediator;
        _commandManager = commandManager;
        _historyManager = historyManager;
        directorScoreWindow.Init(this);
        _player = (LingoPlayer) player;
        _player.ActiveMovieChanged += OnActiveMovieChanged;
        

        var height = 400;
        var width = 800;

        _leftChannelForLabels = new DirGodotScoreLeftForLabels(_gfxValues, "Labels", _gfxValues.LabelsBarHeight);
        _leftHeaderForFrames = new DirGodotScoreLeftForLabels(_gfxValues, "Member", _gfxValues.ChannelFramesHeight);
        _marginContainer.AddChild(_leftChannelForLabels);
        _marginContainer.AddChild(_leftHeaderForFrames);
        

        Size = new Vector2(width, height);
        CustomMinimumSize = Size;
        _topHeaders = new DirGodotTopHeaderContainer(_gfxValues, _player.Factory, Mouse,new Vector2(0, _gfxValues.ChannelHeight + 5));
        _topGrids = new DirGodotTopGridContainer(_gfxValues, _player.Factory, _mediator, commandManager);
        _topGrids.Visible = false;
        _leftChannelsHeaders = new DirGodotScoreLeftChannelHeaders(_gfxValues,_player.Factory,Mouse, new Vector2(0, _gfxValues.TopStripHeight - _footerMargin));
        _grid = new DirGodotScoreGrid(directorMediator, _gfxValues, commandManager, historyManager, _player.Factory);
        _mediator.Subscribe(_grid);
        _framesHeader = new DirGodotFrameHeader(_gfxValues);
        _frameScripts = new DirGodotFrameScriptsBar(_gfxValues, _player.Factory);
        _labelBar = new DirGodotScoreLabelsBar(_gfxValues, commandManager);
        _labelBar.HeaderCollapseChanged += OnHeaderCollapseChanged;

        _collapseButton = new CollapseButton(_labelBar);

        

        // The grid inside master scoller
        _masterScroller.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _masterScroller.VerticalScrollMode= ScrollContainer.ScrollMode.ShowAlways;
        _masterScroller.Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, Size.Y - _gfxValues.TopStripHeight- _footerMargin);
        
        _masterScroller.AddChild(_scrollContent);
        _marginContainer.AddChild(_masterScroller);

        _scrollContent.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _scrollContent.SizeFlagsVertical = SizeFlags.ExpandFill;
        _scrollContent.MouseFilter = MouseFilterEnum.Ignore;
        _scrollContent.AddChild(_grid);

        _grid.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _grid.SizeFlagsVertical = SizeFlags.ExpandFill;
        _grid.Resized += UpdateScrollSize;

        // The top strip with clipper
        _topHClipper = new ColorRect
        {
            Color = new Color(0, 0, 0, 0),
            Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, _gfxValues.TopStripHeight),
            Position = new Vector2(_gfxValues.ChannelInfoWidth, 0),
            ClipContents = true
        };
        _topStripContent.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        _topStripContent.SizeFlagsVertical = Control.SizeFlags.Fill;
        _topHClipper.AddChild(_topStripContent);
        _topHClipper.AddChild(_collapseButton);
        _topHClipper.AddChild(_topGrids);
        _marginContainer.AddChild(_topHClipper);
        _marginContainer.AddChild(_topHeaders);
        _topStripContent.AddChild(_labelBar);
        _topStripContent.AddChild(_frameScripts);
        _topStripContent.AddChild(_framesHeader);




        // the vertical channel sprite numbers with visibility
        _leftChannelsScollClipper.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _leftChannelsScollClipper.VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever;
        _leftChannelsScollClipper.Size = new Vector2(_gfxValues.ChannelInfoWidth, Size.Y - _gfxValues.TopStripHeight - _footerMargin);
        _leftChannelsScollClipper.ClipContents = true;
        _leftChannelsScollClipper.MouseFilter = MouseFilterEnum.Ignore;
        _leftChannelsScollClipper.AddChild(_leftChannelsHeaders);
        _marginContainer.AddChild(_leftChannelsScollClipper); 

       _topGrids.Position = new Vector2(0, _gfxValues.ChannelHeight + 5);
        OnHeaderCollapseChanged(false);
        RepositionBars();
    }

    private void OnHeaderCollapseChanged(bool collapsed)
    {
        _topCollapsed = collapsed;
        _topGrids.Collapsed = collapsed;
        _topHeaders.Collapsed = collapsed;
        RepositionBars();
    }

    private float _topHeight = 0;
    private void RepositionBars()
    {
        int ch = _gfxValues.ChannelHeight;
        float barsHeight = _topCollapsed ? 0 : ch * 5;
        _frameScripts.Position = new Vector2(0, 20 + barsHeight);
        _framesHeader.Position = new Vector2(0, _frameScripts.Position.Y + 20);
        _topHeight = _frameScripts.Position.Y +20;

        float topHeight = _framesHeader.Position.Y + 20;
        _masterScroller.Position = new Vector2(_gfxValues.ChannelInfoWidth, topHeight);
        _leftChannelsScollClipper.Position = new Vector2(0, topHeight);
        _collapseButton.Position = new Vector2(_topHClipper.Size.X - 16, 4);
        _leftHeaderForFrames.Position = new Vector2(0, _framesHeader.Position.Y);
        _lastPosV = -1;
        UpdateScrollSize();
    }

    private float _lastPosV = -1;
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!Visible) return;
        UpdatePositioScollPositionChanged();
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsActiveWindow) return;
        if (@event is InputEventMouseButton mb && !mb.IsPressed())
        {
            if (mb.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
            {
                var mousePos = GetGlobalMousePosition();
                Rect2 bounds = new Rect2(new Vector2(_masterScroller.GlobalPosition.X- _gfxValues.ChannelInfoWidth, _masterScroller.GlobalPosition.Y), _masterScroller.Size);
                if (bounds.HasPoint(mousePos))
                {
                    if (mb.ButtonIndex == MouseButton.WheelUp)
                        _masterScroller.ScrollVertical -= 20;
                    else
                        _masterScroller.ScrollVertical += 20;
                    _lastPosV = -1;
                    UpdatePositioScollPositionChanged();
                    GetViewport().SetInputAsHandled();
                }
            }
        }
    }
    private void UpdatePositioScollPositionChanged()
    {
        if (_lastPosV != _masterScroller.ScrollVertical)
        {
            _lastPosV = _masterScroller.ScrollVertical;
            var lastPos = new Vector2(0, -_masterScroller.ScrollVertical);
            _leftChannelsHeaders.UpdatePosition(lastPos, _topHeight + _gfxValues.ChannelHeight);
            _leftChannelsScollClipper.ScrollVertical = _masterScroller.ScrollVertical;
        }
        _topStripContent.Position = new Vector2(-_masterScroller.ScrollHorizontal, _topStripContent.Position.Y);
        if (_topGrids.ScrollX != _masterScroller.ScrollHorizontal)
            _topGrids.ScrollX = _masterScroller.ScrollHorizontal;
    }

    private void RefreshGrid()
    {
        _grid.MarkSpriteDirty();
    }

    private void UpdateScrollSize()
    {
        if (_movie == null) return;

        float gridWidth = _gfxValues.ChannelInfoWidth + _movie.FrameCount * _gfxValues.FrameWidth + _gfxValues.ExtraMargin;
        float gridHeight = _movie.MaxSpriteChannelCount * _gfxValues.ChannelHeight + _gfxValues.ExtraMargin;

        float topHeight = _framesHeader.Position.Y + 20;

        //_channelBar.CustomMinimumSize = new Vector2(_gfxValues.ChannelInfoWidth, gridHeight - _footerMargin);
        _scrollContent.CustomMinimumSize = new Vector2(gridWidth, gridHeight - _footerMargin);
        _topStripContent.CustomMinimumSize = new Vector2(gridWidth, topHeight + 20);

        _leftChannelsScollClipper.Size = new Vector2(_gfxValues.ChannelInfoWidth, Size.Y - topHeight - 20 - _footerMargin);
        _topHClipper.Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, topHeight + 20);
        _masterScroller.Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, Size.Y - topHeight - 20 - _footerMargin);
    }


    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        RepositionBars();
    }

    //public override void _Input(InputEvent @event)
    //{
    //    base._Input(@event);
    //}

    private void OnActiveMovieChanged(ILingoMovie? movie)
    {
        SetActiveMovie(movie as LingoMovie);
    }
    public void SetActiveMovie(LingoMovie? movie)
    {
        _movie = movie;
        _grid.SetMovie(movie);
        _framesHeader.SetMovie(movie);
        _frameScripts.SetMovie(movie);
        _topHeaders.SetMovie(movie);
        _topGrids.SetMovie(movie);
        _leftChannelsHeaders.SetMovie(movie);
        _labelBar.SetMovie(movie);
        _labelBar.HeaderCollapsed = _topCollapsed;
        RepositionBars();
    }

    

    protected override void Dispose(bool disposing)
    {
        _player.ActiveMovieChanged -= OnActiveMovieChanged;
        _grid.Dispose();
        _labelBar.Dispose();
        _frameScripts.Dispose();
        _topHeaders.Dispose();
        _topGrids.Dispose();
        _masterScroller.Dispose();
        //_hScroller.Dispose();
        _leftChannelsHeaders.Dispose();
        _mediator.Unsubscribe(_grid);
        base.Dispose(disposing);
    }


    #region Commands

    public bool CanExecute(ChangeSpriteRangeCommand command) => true;
    public bool Handle(ChangeSpriteRangeCommand command)
    {
        if (command.EndChannel != command.Sprite.SpriteNum - 1)
            command.Movie.ChangeSpriteChannel(command.Sprite, command.EndChannel);
        command.Sprite.BeginFrame = command.EndBegin;
        command.Sprite.EndFrame = command.EndEnd;
        _historyManager.Push(command.ToUndo(RefreshGrid), command.ToRedo(RefreshGrid));
        RefreshGrid();
        return true;
    }

    public bool CanExecute(AddSpriteCommand command) => true;
    public bool Handle(AddSpriteCommand command)
    {
        var sprite = command.Movie.AddSprite(command.Channel, command.BeginFrame, command.EndFrame, 0, 0,
            s => s.SetMember(command.Member));
        _historyManager.Push(command.ToUndo(sprite, RefreshGrid), command.ToRedo(RefreshGrid));
        RefreshGrid();
        return true;
    }

    public bool CanExecute(RemoveSpriteCommand command) => true;
    public bool Handle(RemoveSpriteCommand command)
    {
        var movie = command.Movie;
        var sprite = command.Sprite;

        int channel = sprite.SpriteNum;
        int begin = sprite.BeginFrame;
        int end = sprite.EndFrame;
        var member = sprite.Member;
        string name = sprite.Name;
        float x = sprite.LocH;
        float y = sprite.LocV;

        sprite.RemoveMe();

        LingoSprite2D current = sprite;
        void refresh() => RefreshGrid();

        Action undo = () =>
        {
            current = movie.AddSprite(channel, begin, end, x, y, s =>
            {
                s.Name = name;
                if (member != null)
                    s.SetMember(member);
            });
            refresh();
        };

        Action redo = () =>
        {
            current.RemoveMe();
            refresh();
        };

        _historyManager.Push(undo, redo);
        RefreshGrid();
        return true;
    }


    #endregion



    internal partial class DirGodotScoreLeftForLabels : Control
    {
        private float _height;
        private readonly DirScoreGfxValues _gfxValues;
        private readonly string _text;

        public DirGodotScoreLeftForLabels(DirScoreGfxValues gfxValues, string text,float height)
        {
            _height = height;
            _gfxValues = gfxValues;
            _text = text;
            Size = new Vector2(gfxValues.ChannelLabelWidth + gfxValues.ChannelHeight, _height);
        }

        public override void _Draw()
        {
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), new Color("#f0f0f0"));
            DrawTextWithLine(0, _height, _text, false);
        }

        private void DrawTextWithLine(int top, float height, string text, bool withTopLines = true)
        {
            var font = ThemeDB.FallbackFont;
            if (withTopLines)
                DrawLines(top);
            DrawString(font, new Vector2(5, top + font.GetAscent() - 3), text, HorizontalAlignment.Left, -1, 10, new Color("#666666"));
            DrawLine(new Vector2(0, top + height), new Vector2(Size.X, top + height), _gfxValues.ColLineDark.ToGodotColor());
            DrawLine(new Vector2(0, top + height + 1), new Vector2(Size.X, top + height + 1), _gfxValues.ColLineLight.ToGodotColor());
        }

        private void DrawLines(int top)
        {
            DrawLine(new Vector2(0, top), new Vector2(Size.X, top), _gfxValues.ColLineDark.ToGodotColor());
            DrawLine(new Vector2(0, top + 1), new Vector2(Size.X, top + 1), _gfxValues.ColLineLight.ToGodotColor());
        }
    }

   

    private partial class CollapseButton : Control
    {
        private readonly DirGodotScoreLabelsBar _labels;
        public CollapseButton(DirGodotScoreLabelsBar labels)
        {
            _labels = labels;
            Size = new Vector2(12, 12);
            MouseFilter = MouseFilterEnum.Stop;
            _labels.HeaderCollapseChanged += _ => QueueRedraw();
        }
        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                _labels.ToggleCollapsed();
            }
        }
        public override void _Draw()
        {
            var font = ThemeDB.FallbackFont;
            DrawRect(new Rect2(0, 0, 12, 12), Colors.Black, false, 1);
            var x = 1;
            var y = 1;
            Vector2[] pts = !_labels.HeaderCollapsed
                ?[ new Vector2(x, 3), new Vector2(x + 8, 3), new Vector2(x + 4, 11) ]
                :[ new Vector2(10, y), new Vector2(10, y + 8), new Vector2(3, y + 4) ];
            DrawPolygon(pts, new[] { Colors.Black });
            //DrawString(font, new Vector2(1, font.GetAscent() - 7), (_labels.HeaderCollapsed ? "▶" : "▼"),HorizontalAlignment.Left,-1,11);
        }
    }
}
