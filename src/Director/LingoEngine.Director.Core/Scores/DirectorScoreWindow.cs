using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Gfx;
using LingoEngine.ColorPalettes;
using LingoEngine.Primitives;
using LingoEngine.Commands;
using LingoEngine.Director.Core.UI;


namespace LingoEngine.Director.Core.Scores
{
    public class DirectorScoreWindow : DirectorWindow<IDirFrameworkScoreWindow> 
    {
        private readonly IDirSpritesManager _spritesManager;
        private readonly DirScoreManager _scoreManager;
        private readonly IDirectorWindowManager _windowManager;
        private readonly ILingoColorPaletteDefinitions _paletteDefinitions;
        private readonly LingoPlayer _player;
        private readonly DirScoreLabelsBar _labelsBar;
        private readonly DirScoreFrameHeader _frameHeader;
        private readonly DirScoreLeftTopContainer _LeftTopContainer;
        private readonly DirScoreLeftChannelsContainer _LeftChannelContainer;
        private readonly LingoGfxPanel _panelScroll;
        private readonly LingoGfxPanel _panelFix;
        private readonly LingoGfxInputCombobox _spriteShowSelector;
        private LingoMovie? _movie;
        protected ILingoMouseSubscription _mouseSub;
        private float _scollY;
        private float _scollX;
        private float _lastPosV;
        private KeyValuePair<string, string>[] _frameLabelsForCombo = [
                   new KeyValuePair<string, string>("Label1","Label1"),
               ];


        public DirScoreGridTopContainer TopContainer { get; private set; }
        public DirScoreGridSprites2DContainer Sprites2DContainer { get; private set; }
        public DirScoreLabelsBar LabelsBar => _labelsBar;
        public DirScoreFrameHeader FrameHeader => _frameHeader;
        public DirScoreGfxValues GfxValues  => _scoreManager.GfxValues;
       // public DirScoreLeftTopContainer LeftTopContainer => _LeftTopContainer;
        //public DirScoreLeftChannelsContainer LeftChannelContainer => _LeftChannelContainer;
        public LingoGfxPanel ScorePanelScroll => _panelScroll;
        public LingoGfxPanel ScorePanelFix => _panelFix;
        public float ScollX { get => _scollX; set => _scollX = value; }
        public float ScollY { get => _scollY; set => _scollY = value; }

#pragma warning disable CS8618
        public DirectorScoreWindow(IDirSpritesManager spritesManager, ILingoPlayer player, ILingoFrameworkFactory factory, DirScoreManager scoreManager, IDirectorWindowManager windowManager, ILingoColorPaletteDefinitions paletteDefinitions, ILingoCommandManager commandManager, IDirectorEventMediator mediator) : base(factory)
#pragma warning restore CS8618 
        {
            _spritesManager = spritesManager;
            _scoreManager = scoreManager;
            _windowManager = windowManager;
            _paletteDefinitions = paletteDefinitions;
            _player = (LingoPlayer)player;
            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _labelsBar = new DirScoreLabelsBar(GfxValues, factory, commandManager);
            _frameHeader = new DirScoreFrameHeader(GfxValues, factory);
            _LeftTopContainer = new DirScoreLeftTopContainer(GfxValues, factory, new LingoPoint(0, GfxValues.TopStripHeight), mediator);
            _LeftChannelContainer = new DirScoreLeftChannelsContainer(GfxValues, factory, new LingoPoint(0, 0), mediator);

            // Fix top panel
            _panelFix = factory.CreatePanel("ScoreWindowPanelFix");
            _panelFix.AddItem(_LeftTopContainer.Canvas,0, GfxValues.LabelsBarHeight+1);
            _panelFix.AddItem(_labelsBar.FixPanel,0, 0);

            _frameLabelsForCombo = Enum.GetNames<DirScoreSpriteLabelType>().Select(name => new KeyValuePair<string, string>(name, name)).ToArray();
            _spriteShowSelector = _panelFix.SetComboBoxAt(_frameLabelsForCombo, "ScoreSpriteShowSelector", 2, GfxValues.TopStripHeight - 2, GfxValues.ChannelInfoWidth - 6, _frameLabelsForCombo[1].Key, ShowSpriteInfo);
            _spriteShowSelector.Visibility = true;


            // Main Scroll panel
            _panelScroll = factory.CreatePanel("ScoreWindowPanelScroll");
            _panelScroll.AddItem(_LeftChannelContainer.Canvas,0, 0);

            _labelsBar.HeaderCollapseChanged += OnHeaderCollapseChanged;
        }

       

        public override void Init(IDirFrameworkWindow frameworkWindow)
        {
            base.Init(frameworkWindow);
            _mouseSub = Mouse.OnMouseEvent(HandleMouseEvent);
            TopContainer = new DirScoreGridTopContainer(_scoreManager, _paletteDefinitions, ShowConfirmDialog);
            Sprites2DContainer = new DirScoreGridSprites2DContainer(_scoreManager, ShowConfirmDialog);
        }
        public override void Dispose()
        {
            _mouseSub.Release();
            if (_movie != null)
                _movie.CurrentFrameChanged -= OnCurrentFrameChanged;
            
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            _labelsBar.HeaderCollapseChanged -= OnHeaderCollapseChanged;
            _labelsBar.Dispose();
            _frameHeader.Dispose();
            TopContainer.Dispose();
            Sprites2DContainer.Dispose();
            _LeftTopContainer.Dispose();
            _LeftChannelContainer.Dispose();
            base.Dispose();
        }


        private void OnActiveMovieChanged(ILingoMovie? movie)
        {
            if (_movie != null)
                _movie.CurrentFrameChanged -= OnCurrentFrameChanged;
            
            _movie = movie as LingoMovie;
            if (_movie != null)
                _movie.CurrentFrameChanged += OnCurrentFrameChanged;
            _scoreManager.CurrentMovieChanged(_movie);
            TopContainer.SetMovie(_movie);
            Sprites2DContainer.SetMovie(_movie);
            _labelsBar.SetMovie(_movie);
            _frameHeader.SetMovie(_movie);
            _LeftTopContainer.SetMovie(_movie);
            _LeftChannelContainer.SetMovie(_movie);
        }
        

        private void HandleMouseEvent(LingoMouseEvent mouseEvent)
        {
            if (_movie == null) return;
            var gfxValues = _scoreManager.GfxValues;
            
            
            float frameF = (mouseEvent.MouseH - gfxValues.ChannelInfoWidth - 3 + ScollX) / gfxValues.FrameWidth;
            var mouseFrame = Math.Clamp(MathL.RoundToInt(frameF) + 1, 1, _movie.FrameCount);

            if (mouseEvent.MouseV < gfxValues.LabelsBarHeight)
            {
                // Inside labels bar
                _labelsBar.HandleMouseEvent(mouseEvent, mouseFrame);
                return;
            }
            var isInsideLeft = mouseEvent.MouseH < gfxValues.ChannelInfoWidth;
            var isInFrameHeader = false;
            var spriteNumWithChannel = 0;
            if (mouseEvent.MouseV <= gfxValues.LabelsBarHeight + TopContainer.Size.Y)
            {
                // Top channel
                var yPosition = mouseEvent.MouseV - gfxValues.LabelsBarHeight;
                spriteNumWithChannel = Math.Clamp(MathL.RoundToInt((yPosition + 4) / gfxValues.ChannelHeight), 1, 999);
            }
            else {
                isInFrameHeader = mouseEvent.MouseV <= gfxValues.LabelsBarHeight + TopContainer.Size.Y + gfxValues.ChannelFramesHeight;
                if (isInFrameHeader && !isInsideLeft)
                {
                    _frameHeader.HandleMouseEvent(mouseEvent, mouseFrame);
                    return;
                }
                // Sprites Container
                var topPosition = gfxValues.LabelsBarHeight + TopContainer.Size.Y + gfxValues.ChannelFramesHeight;
                if (mouseEvent.MouseV >= topPosition && mouseEvent.MouseV <= topPosition + Sprites2DContainer.Size.Y )
                {
                    var yPosition = mouseEvent.MouseV - topPosition;
                    spriteNumWithChannel = Math.Clamp(MathL.RoundToInt((yPosition + 4 + ScollY) / gfxValues.ChannelHeight), 1, 999) + 6;
                }
            }
            //Console.WriteLine($"Mouse Event: Frame {mouseFrame}, Channel {channel}, isInsideLeft={isInsideLeft}");
            if (spriteNumWithChannel <= 0)
                return;
            if (isInsideLeft )
            {
                if (mouseEvent.Type == LingoMouseEventType.MouseDown)
                {
                    if (spriteNumWithChannel < 7)
                        _LeftTopContainer.RaiseMouseDown(mouseEvent, spriteNumWithChannel);
                    else
                        _LeftChannelContainer.RaiseMouseDown(mouseEvent, spriteNumWithChannel);
                }
                return;
            }
            _scoreManager.HandleMouse(mouseEvent, HeaderCollapsed? spriteNumWithChannel+5 : spriteNumWithChannel, mouseFrame);
        }
        public void OnCurrentFrameChanged(int currentFrame)
        {
            
            TopContainer.CurrentFrameChanged(currentFrame);
            Sprites2DContainer.CurrentFrameChanged(currentFrame);
        }

        protected override void OnRaiseKeyDown(LingoKey lingoKey)
        {
            if (_movie != null && string.Equals(lingoKey.Key, "Delete", StringComparison.OrdinalIgnoreCase))
                _spritesManager.DeleteSelected(_movie);
        }

        protected override void OnRaiseKeyUp(LingoKey lingoKey) { }

        internal IDirectorWindowDialogReference? ShowConfirmDialog(string title, ILingoFrameworkGfxPanel panel)
            => _windowManager.ShowCustomDialog(title, panel);


        public void OnResize(int width, int height)
        {
            _labelsBar.OnResize(width, height);
        }

        public void ScollVPositionChanged()
        {
            if (_lastPosV != _scollY)
            {
                _lastPosV = _scollY;
                var lastPos = new LingoPoint(0, -_scollY);
                _LeftChannelContainer.UpdatePosition(new LingoPoint(0, - _lastPosV+1));
                //_leftChannelsScollClipper.ScrollVertical = _masterScroller.ScrollVertical;
            }
        }


        #region Top collapser
        public void ToggleCollapsed() => _labelsBar.ToggleCollapsed();

        public bool HeaderCollapsed
        {
            get => _labelsBar.HeaderCollapsed;
            set => _labelsBar.HeaderCollapsed = value;
        }

        public event Action<bool>? HeaderCollapseChanged
        {
            add => _labelsBar.HeaderCollapseChanged += value;
            remove => _labelsBar.HeaderCollapseChanged -= value;
        }
        private void OnHeaderCollapseChanged(bool state)
        {
            _LeftTopContainer.Collapsed = state;
            _spriteShowSelector.Y = GfxValues.LabelsBarHeight + 2 + _LeftTopContainer.Height;
        }
        #endregion


        private void ShowSpriteInfo(string? key)
        {
            if (key == null) return;
            var type = Enum.Parse<DirScoreSpriteLabelType>(key);
            _scoreManager.ShowSpriteInfo(type);
        }
    }
}
