using Godot;
using LingoEngine.Movies;
using LingoEngine.Core;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Commands;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Director.LGodot.Styles;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Timeline overlay showing the Score channels and frames.
/// </summary>
public partial class DirGodotScoreWindow : BaseGodotWindow, IDirFrameworkScoreWindow
{
   
    
    private int _footerMargin= 10;

    private bool wasToggleKey;
    private LingoMovie? _movie;
    private readonly ScrollContainer _leftChannelsScollClipper = new ScrollContainer{ Name = "ScoreLeftChannelsScollClipper" };
    private readonly ScrollContainer _masterScroller = new ScrollContainer{Name="ScoreMasterScroller"};
    private readonly Control _topStripContent = new Control{Name="ScoreTopStripContent"};
    private readonly Control _scrollContent = new Control{Name= "ScoreScrollContent" };
    private ColorRect _topHClipper;
    private readonly TopChannelsContainer _TopContainer;
    private readonly Sprites2DChannelsContainer _Sprites2DContainer;
    private LingoPlayer _player;
    private readonly DirScoreGfxValues _gfxValues ;
    private readonly DirGodotFrameHeader _framesHeader;
    private bool _topCollapsed = true;

    private readonly DirectorScoreWindow _directorScoreWindow;

    public DirGodotScoreWindow(IDirectorEventMediator directorMediator, ILingoCommandManager commandManager, DirectorScoreWindow directorScoreWindow, ILingoPlayer player, IDirGodotWindowManager windowManager, IDirSpritesManager spritesManager,DirectorGodotStyle godotStyle)
        : base(DirectorMenuCodes.ScoreWindow, "Score", windowManager)
    {
        //DontUseInputInsteadOfGuiInput();
        var _marginContainer = new Control();
        _marginContainer.Position = new Vector2(0, TitleBarHeight);
        _marginContainer.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_marginContainer);
        _directorScoreWindow = directorScoreWindow;
        directorScoreWindow.Init(this);
        _TopContainer = new TopChannelsContainer(directorScoreWindow.TopContainer);
        _Sprites2DContainer = new Sprites2DChannelsContainer(directorScoreWindow.Sprites2DContainer);
        _player = (LingoPlayer) player;
        _player.ActiveMovieChanged += OnActiveMovieChanged;
        
        _gfxValues = _directorScoreWindow.GfxValues;

        var height = 400;
        var width = 800;

        Size = new Vector2(width, height);
        CustomMinimumSize = Size;
        _framesHeader = new DirGodotFrameHeader(_gfxValues);
        _directorScoreWindow.HeaderCollapseChanged += OnHeaderCollapseChanged;

        

        // The grid inside master scoller
        _masterScroller.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _masterScroller.VerticalScrollMode= ScrollContainer.ScrollMode.ShowAlways;
        _masterScroller.Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, Size.Y - _gfxValues.TopStripHeight- _footerMargin);
        _masterScroller.AddChild(_scrollContent);
        _marginContainer.AddChild(_masterScroller);

        _scrollContent.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _scrollContent.SizeFlagsVertical = SizeFlags.ExpandFill;
        _scrollContent.MouseFilter = MouseFilterEnum.Ignore;
        _scrollContent.AddChild(_Sprites2DContainer);


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
        _topHClipper.AddChild(_TopContainer);
        _marginContainer.AddChild(_topHClipper);
        _topStripContent.AddChild(_framesHeader);
        _topStripContent.AddChild((Node)_directorScoreWindow.LabelsBar.ScollingPanel.FrameworkObj.FrameworkNode);




        // the vertical channel sprite numbers with visibility
        _leftChannelsScollClipper.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _leftChannelsScollClipper.VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever;
        _leftChannelsScollClipper.Size = new Vector2(_gfxValues.ChannelInfoWidth, Size.Y - _gfxValues.TopStripHeight - _footerMargin);
        _leftChannelsScollClipper.ClipContents = true;
        _leftChannelsScollClipper.MouseFilter = MouseFilterEnum.Ignore;
        _leftChannelsScollClipper.AddChild((Node)directorScoreWindow.ScorePanelScroll.FrameworkObj.FrameworkNode);
        _marginContainer.AddChild(_leftChannelsScollClipper); 
        _marginContainer.AddChild((Node)directorScoreWindow.ScorePanelFix.FrameworkObj.FrameworkNode); 

        _TopContainer.Position = new Vector2(0, _gfxValues.LabelsBarHeight + 1);
        OnHeaderCollapseChanged(false);
        RepositionBars();
    }

    private void OnHeaderCollapseChanged(bool collapsed)
    {
        _topCollapsed = collapsed;
        _TopContainer.SetCollapsed(collapsed);
        RepositionBars();
    }

    private float _topHeight = 0;
    private void RepositionBars()
    {
        float barsHeight = _topCollapsed ? _gfxValues.ChannelHeight : _gfxValues.ChannelHeight * 6;
        _framesHeader.Position = new Vector2(0, barsHeight + _gfxValues.LabelsBarHeight+2);
        float topHeight = barsHeight + _gfxValues.ChannelFramesHeight + 22;

        _masterScroller.Position = new Vector2(_gfxValues.ChannelInfoWidth, topHeight);
        _leftChannelsScollClipper.Position = new Vector2(0, topHeight);
        _lastPosV = -1;
        UpdateScrollSize();
        _directorScoreWindow.OnResize((int)Size.X,(int)Size.Y-TitleBarHeight);
    }

    public override void _Process(double delta)
    {
        // Needed for horizontal scroll;
        base._Process(delta);
        if (!Visible || !IsActiveWindow) return;
        UpdatePositioScollPositionChanged();
    }
    //public override void _UnhandledInput(InputEvent @event)
    //{
    //    if (!IsActiveWindow) return;
    //    if (@event is InputEventMouseButton mb && !mb.IsPressed())
    //    {
    //        if (mb.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
    //        {
    //            var mousePos = GetGlobalMousePosition();
    //            Rect2 bounds = new Rect2(new Vector2(_masterScroller.GlobalPosition.X- _gfxValues.ChannelInfoWidth, _masterScroller.GlobalPosition.Y), _masterScroller.Size);
    //            if (bounds.HasPoint(mousePos))
    //            {
    //                if (mb.ButtonIndex == MouseButton.WheelUp)
    //                    _masterScroller.ScrollVertical -= 20;
    //                else
    //                    _masterScroller.ScrollVertical += 20;
    //                _lastPosV = -1;
    //                UpdatePositioScollPositionChanged();
    //                GetViewport().SetInputAsHandled();
    //            }
    //        }
    //    }
    //}


    private float _lastPosV = -1;
    private int _lastPosH;

    private void UpdatePositioScollPositionChanged()
    {
        var changed = false;
        if (_lastPosV != _masterScroller.ScrollVertical)
        {
            _lastPosV = _masterScroller.ScrollVertical;
            var lastPos = new Vector2(0, -_masterScroller.ScrollVertical);
            _leftChannelsScollClipper.ScrollVertical = _masterScroller.ScrollVertical;
            _directorScoreWindow.ScollY = _masterScroller.ScrollVertical;
            _directorScoreWindow.ScollVPositionChanged();
            changed = true;
        }
        if (_lastPosH != _masterScroller.ScrollHorizontal)
        {
            _lastPosH = _masterScroller.ScrollHorizontal;
            _directorScoreWindow.ScollX = _masterScroller.ScrollHorizontal;
            if (_TopContainer.ScrollX != _masterScroller.ScrollHorizontal)
                _TopContainer.ScrollX = _masterScroller.ScrollHorizontal;
            changed = true;
        }
        if (changed)
            _topStripContent.Position = new Vector2(-_masterScroller.ScrollHorizontal, _topStripContent.Position.Y);
        
        
    }

    private void RefreshGrid()
    {
        //_grid.MarkSpriteDirty();
    }

    private void UpdateScrollSize()
    {
        if (_movie == null) return;

        float gridWidth = _gfxValues.ChannelInfoWidth + _movie.FrameCount * _gfxValues.FrameWidth + _gfxValues.ExtraMargin;
        float gridHeight = _movie.MaxSpriteChannelCount * _gfxValues.ChannelHeight + _gfxValues.ExtraMargin;

        //float topHeight = _gfxValues.ChannelFramesHeight;
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
        _framesHeader.SetMovie(movie);
        //_LeftChannelsContainer.SetMovie(movie);
        RepositionBars();
    }

    

    protected override void Dispose(bool disposing)
    {
        _player.ActiveMovieChanged -= OnActiveMovieChanged;
        //_labelBar.Dispose();
        //_LeftTopContainer.Dispose();
        _TopContainer.Dispose();
        _masterScroller.Dispose();
        //_hScroller.Dispose();
        //_LeftChannelsContainer.Dispose();
        base.Dispose(disposing);
    }



   


   
    private partial class TopChannelsContainer : ChannelsContainer<DirScoreGridTopContainer>
    {
        public TopChannelsContainer(DirScoreGridTopContainer topContainer) : base(topContainer)
        {
        }

        internal void SetCollapsed(bool collapsed)
        {
            _DirScoreContainer.Collapsed = collapsed;
        }
    }
    private partial class Sprites2DChannelsContainer : ChannelsContainer<DirScoreGridSprites2DContainer>
    {
        public Sprites2DChannelsContainer(DirScoreGridSprites2DContainer topContainer) : base(topContainer)
        {
        }
    }
    private abstract partial class ChannelsContainer<TDirScoreGridContainer> : Control, IDirScoreFrameworkGridContainer
        where TDirScoreGridContainer : DirScoreGridContainer
    {
        protected TDirScoreGridContainer _DirScoreContainer;
        private float _scrollX;
        private readonly LingoGodotGfxCanvas _gridLines;
        private readonly LingoGodotGfxCanvas _currentFrame;
        private List<ContainerChannel> _channels = new List<ContainerChannel>();
        public float CurrentFrameX { get => _currentFrame.Position.X; set => _currentFrame.Position = new Vector2(value, _currentFrame.Position.Y); }

        public ChannelsContainer(TDirScoreGridContainer topContainer)
        {
            _DirScoreContainer = topContainer;
            _DirScoreContainer.Init(this);
            _gridLines = _DirScoreContainer.CanvasGridLines.Framework<LingoGodotGfxCanvas>();
            _currentFrame = _DirScoreContainer.CanvasCurrentFrame.Framework<LingoGodotGfxCanvas>();
            Position = _DirScoreContainer.Position.ToVector2();
            Size = _DirScoreContainer.Size.ToVector2();
            CustomMinimumSize = Size;
            MouseFilter = MouseFilterEnum.Ignore;

            AddChild(_gridLines);
            CreateChannels();
            
        }

        public void RequireRecreateChannels() => CreateChannels();
        public void CreateChannels()
        {
            if (_channels.Count > 0)
            {
                foreach (var channel in _channels)
                    channel.Dispose();
                _channels.Clear();
            }
            foreach (var ch in _DirScoreContainer.Channels)
            {
                var channel = new ContainerChannel(ch);
                _channels.Add(channel);
                AddChild(channel);
            }
            if (_currentFrame.GetParent() != null)
                RemoveChild(_currentFrame);
            AddChild(_currentFrame);
        }
        protected override void Dispose(bool disposing)
        {
            _gridLines.Dispose();
            _currentFrame.Dispose();
            base.Dispose(disposing);

        }
       
        public void RequireRedrawChannels() 
        {
            foreach (var ch in _channels)
                ch.RequireSetPosAndSize();
            CurrentFrameX = _DirScoreContainer.CurrentFrameX;
        }

       

        public void UpdateSize()
        {
            if (_DirScoreContainer.Size.X == 0) return;
            var size = _DirScoreContainer.Size.ToVector2();
            var width = size.X;
            var height = size.Y;
            CustomMinimumSize = size;
            Size = size;
            CurrentFrameX = _DirScoreContainer.CurrentFrameX;
        }

        public float ScrollX
        {
            get => _scrollX;
            set
            {
                _scrollX = value;
                Position = new Vector2(-value, Position.Y);
            }
        }
    }

    private partial class ContainerChannel : Control, IDirScoreChannelFramework
    {
        private readonly DirScoreChannel _directorChannel;

        public ContainerChannel(DirScoreChannel directorChannel)
        {
            _directorChannel = directorChannel;
            _directorChannel.Init(this);
            AddChild(_directorChannel.Framework<LingoGodotGfxCanvas>());
            Size = new Vector2(directorChannel.Size.X, directorChannel.Size.Y);
            CustomMinimumSize = Size;
            Position = new Vector2(directorChannel.Position.X, directorChannel.Position.Y);
            MouseFilter = MouseFilterEnum.Ignore;
        }

        public void RequireSetPosAndSize()
        {
            Size = new Vector2(_directorChannel.Size.X, _directorChannel.Size.Y);
            CustomMinimumSize = Size;
            Position = new Vector2(_directorChannel.Position.X, _directorChannel.Position.Y);
            Visible = _directorChannel.Visible;
            RequireRedraw();
        }
        public void RequireRedraw()
        {
            QueueRedraw();
        }
        public override void _Draw()
        {
            base._Draw();
            _directorChannel.Draw();
        }
    }


  
}
