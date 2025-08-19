using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Inputs;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Core;
using LingoEngine.Director.Core.Tools;
using LingoEngine.ColorPalettes;
using AbstUI.Commands;
using LingoEngine.Director.Core.UI;
using LingoEngine.Sprites;
using LingoEngine.Sounds;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.Windowing;
using AbstUI;


namespace LingoEngine.Director.Core.Scores
{
    public class DirectorScoreWindow : DirectorWindow<IDirFrameworkScoreWindow>
    {
        private readonly IDirSpritesManager _spritesManager;
        private readonly DirScoreManager _scoreManager;
        private readonly IAbstWindowManager _windowManager;
        private readonly ILingoColorPaletteDefinitions _paletteDefinitions;
        private readonly LingoPlayer _player;
        private readonly IAbstMouse<AbstMouseEvent> _globalMouse;
        private readonly DirScoreLabelsBar _labelsBar;
        private readonly DirScoreFrameHeader _frameHeader;
        private readonly DirScoreLeftTopContainer _LeftTopContainer;
        private readonly DirScoreLeftChannelsContainer _LeftChannelContainer;
        private readonly AbstPanel _panelScroll;
        private readonly AbstPanel _panelFix;
        private readonly AbstInputCombobox _spriteShowSelector;
        private DirScoreSprite? _contextSprite;
        private int _contextFrame;
        private LingoMovie? _movie;
        protected IAbstMouseSubscription _mouseSub;
        private IAbstMouseSubscription? _globalMouseUpSub;
        private float _scollY;
        private float _scollX;
        private float _lastPosV;
        private KeyValuePair<string, string>[] _frameLabelsForCombo = [
                   new KeyValuePair<string, string>("Label1","Label1"),
               ];
        private AbstContextMenu? _spriteContextMenu;

        public DirScoreGridTopContainer TopContainer { get; private set; }
        public DirScoreGridSprites2DContainer Sprites2DContainer { get; private set; }
        public DirScoreLabelsBar LabelsBar => _labelsBar;
        public DirScoreFrameHeader FrameHeader => _frameHeader;
        public DirScoreGfxValues GfxValues => _scoreManager.GfxValues;
        // public DirScoreLeftTopContainer LeftTopContainer => _LeftTopContainer;
        //public DirScoreLeftChannelsContainer LeftChannelContainer => _LeftChannelContainer;
        public AbstPanel ScorePanelScroll => _panelScroll;
        public AbstPanel ScorePanelFix => _panelFix;
        public float ScollX { get => _scollX; set => _scollX = value; }
        public float ScollY { get => _scollY; set => _scollY = value; }

#pragma warning disable CS8618
        public DirectorScoreWindow(IServiceProvider serviceProvider, IDirSpritesManager spritesManager, ILingoPlayer player, ILingoFrameworkFactory factory, DirScoreManager scoreManager, IAbstWindowManager windowManager, ILingoColorPaletteDefinitions paletteDefinitions, IAbstCommandManager commandManager, IAbstGlobalMouse globalMouse, IDirectorEventMediator mediator, IAbstComponentFactory componentFactory) : base(serviceProvider, DirectorMenuCodes.ScoreWindow)
#pragma warning restore CS8618
        {
            _spritesManager = spritesManager;
            _scoreManager = scoreManager;
            _windowManager = windowManager;
            _paletteDefinitions = paletteDefinitions;
            _player = (LingoPlayer)player;
            _globalMouse = (IAbstMouse<AbstMouseEvent>)globalMouse;
            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _labelsBar = new DirScoreLabelsBar(GfxValues, factory, commandManager);
            _frameHeader = new DirScoreFrameHeader(GfxValues, factory);
            _LeftTopContainer = new DirScoreLeftTopContainer(GfxValues, factory, new APoint(0, GfxValues.TopStripHeight), mediator);
            _LeftChannelContainer = new DirScoreLeftChannelsContainer(GfxValues, factory, new APoint(0, 0), mediator);

            Width = 800;
            Height = 360;
            MinimumHeight = 200;
            MinimumWidth = 300;

            // Fix top panel
            _panelFix = factory.CreatePanel("ScoreWindowPanelFix");
            _panelFix.AddItem(_LeftTopContainer.Canvas, 0, GfxValues.LabelsBarHeight + 1);
            _panelFix.AddItem(_labelsBar.FixPanel, 0, 0);

            _frameLabelsForCombo = Enum.GetNames<DirScoreSpriteLabelType>().Select(name => new KeyValuePair<string, string>(name, name)).ToArray();
            _spriteShowSelector = _panelFix.SetComboBoxAt(_frameLabelsForCombo, "ScoreSpriteShowSelector", 2, GfxValues.TopStripHeight - 2, GfxValues.ChannelInfoWidth - 6, _frameLabelsForCombo[1].Key, ShowSpriteInfo);
            _spriteShowSelector.Visibility = true;


            // Main Scroll panel
            _panelScroll = factory.CreatePanel("ScoreWindowPanelScroll");
            _panelScroll.AddItem(_LeftChannelContainer.Canvas, 0, 0);

            _labelsBar.HeaderCollapseChanged += OnHeaderCollapseChanged;
        }



        public override void Init(IAbstFrameworkWindow frameworkWindow)
        {
            base.Init(frameworkWindow);
            InitContextMenu();
            var mouse = (AbstMouse<AbstMouseEvent>)Mouse;
            _mouseSub = mouse.OnMouseEvent(HandleMouseEvent);
            _globalMouseUpSub = _globalMouse.OnMouseUp(GlobalHandleMouseEvent);
            TopContainer = new DirScoreGridTopContainer(_scoreManager, _paletteDefinitions, ShowConfirmDialog);
            Sprites2DContainer = new DirScoreGridSprites2DContainer(_scoreManager, ShowConfirmDialog);
        }



        public override void Dispose()
        {
            _spriteContextMenu?.Dispose();
            _mouseSub.Release();
            _globalMouseUpSub?.Release();
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


        private void InitContextMenu()
        {
            _spriteContextMenu = CreateContextMenu();
            _spriteContextMenu
                .AddItemFluent(string.Empty, "Find Member",
                    () => _contextSprite?.Sprite is ILingoSpriteWithMember swm && swm.GetMember() != null,
                    () =>
                    {
                        if (_contextSprite?.Sprite is not ILingoSpriteWithMember swm) return;
                        var member = swm.GetMember();
                        if (member != null)
                            _spritesManager.Mediator.RaiseFindMember(member);
                    })
                .AddItemFluent(string.Empty, "Delete Keyframe",
                    () => _movie != null && _contextSprite != null && !_contextSprite.IsLocked &&
                        _contextSprite.Sprite2D != null && _contextSprite.IsKeyFrame(_contextFrame),
                    () =>
                    {
                        _contextSprite?.DeleteKeyFrame(_contextFrame);
                    })
                .AddItemFluent(string.Empty, "Delete",
                    () => _movie != null && _contextSprite != null && !_contextSprite.IsLocked,
                    () =>
                    {
                        if (_movie == null) return;
                        _spritesManager.DeleteSelected(_movie);
                    })
                .AddItemFluent(string.Empty, "Create FilmLoop",
                    () => _movie != null && _spritesManager.SpritesSelection.Sprites.Any() &&
                        _spritesManager.SpritesSelection.Sprites.All(s => s is LingoSprite2D or LingoSpriteSound),
                    () =>
                    {
                        if (_movie == null) return;
                        _spritesManager.CreateFilmLoop(_movie, "New FilmLoop");
                    });
        }


        private void GlobalHandleMouseEvent(AbstMouseEvent mouseEvent)
        {
            if (DirectorDragDropHolder.IsDragging && DirectorDragDropHolder.Member != null && mouseEvent.Type == AbstMouseEventType.MouseUp)
            {
                var localEvent = mouseEvent.Translate((AbstMouse<AbstMouseEvent>)Mouse);
                if (localEvent.MouseH < 0 || localEvent.MouseH > Width || localEvent.MouseV < 0 || localEvent.MouseV > Height)
                    return;
                HandleMouseEvent(localEvent);
            }
        }


        private void HandleMouseEvent(AbstMouseEvent mouseEvent)
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
            else
            {
                isInFrameHeader = mouseEvent.MouseV <= gfxValues.LabelsBarHeight + TopContainer.Size.Y + gfxValues.ChannelFramesHeight;
                if (isInFrameHeader && !isInsideLeft && !DirectorDragDropHolder.IsDragging)
                {
                    _frameHeader.HandleMouseEvent(mouseEvent, mouseFrame);
                    return;
                }
                // Sprites Container
                var topPosition = gfxValues.LabelsBarHeight + TopContainer.Size.Y + gfxValues.ChannelFramesHeight;
                if (mouseEvent.MouseV >= topPosition && mouseEvent.MouseV <= topPosition + Sprites2DContainer.Size.Y)
                {
                    var yPosition = mouseEvent.MouseV - topPosition;
                    spriteNumWithChannel = Math.Clamp(MathL.RoundToInt((yPosition + 4 + ScollY) / gfxValues.ChannelHeight), 1, 999) + 6;
                }
            }
           // Console.WriteLine($"Mouse Event: Frame {mouseFrame}, Channel {spriteNumWithChannel}, isInsideLeft={isInsideLeft}");
            if (spriteNumWithChannel <= 0)
                return;
            if (isInsideLeft)
            {
                if (mouseEvent.Type == AbstMouseEventType.MouseDown)
                {
                    if (spriteNumWithChannel < 7)
                        _LeftTopContainer.RaiseMouseDown(mouseEvent, spriteNumWithChannel);
                    else
                        _LeftChannelContainer.RaiseMouseDown(mouseEvent, spriteNumWithChannel);
                }
                return;
            }
            var scoreChannel = spriteNumWithChannel; // - 6;
            //Console.WriteLine($"Mouse Event: Frame {mouseFrame}, Channel {scoreChannel}, isInsideLeft={isInsideLeft}");
            _scoreManager.HandleMouse(mouseEvent, scoreChannel, mouseFrame);
            if (mouseEvent.Type == AbstMouseEventType.MouseDown && mouseEvent.Mouse.RightMouseDown)
            {
                var sprite = _scoreManager.GetSpriteAt(scoreChannel, mouseFrame);
                if (sprite != null)
                {
                    _contextSprite = sprite;
                    _contextFrame = mouseFrame;
                    _spriteContextMenu.Popup();
                }
            }
        }
        public void OnCurrentFrameChanged(int currentFrame)
        {

            TopContainer.CurrentFrameChanged(currentFrame);
            Sprites2DContainer.CurrentFrameChanged(currentFrame);
        }

        protected override void OnRaiseKeyDown(AbstKeyEvent lingoKey)
        {
            if (_movie != null && string.Equals(lingoKey.Key, "Delete", StringComparison.OrdinalIgnoreCase))
                _spritesManager.DeleteSelected(_movie);
        }

        protected override void OnRaiseKeyUp(AbstKeyEvent lingoKey) { }

        internal IAbstWindowDialogReference? ShowConfirmDialog(string title, IAbstFrameworkPanel panel)
            => _windowManager.ShowCustomDialog(title, panel);


        protected override void OnResizing(bool firstLoad, int width, int height)
        {
            base.OnResizing(firstLoad,width, height);
            _labelsBar.OnResize(width, height);
        }

        public void ScollVPositionChanged()
        {
            if (_lastPosV != _scollY)
            {
                _lastPosV = _scollY;
                var lastPos = new APoint(0, -_scollY);
                _LeftChannelContainer.UpdatePosition(new APoint(0, -_lastPosV + 1));
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
