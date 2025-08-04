using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Director.Core.Tools;


namespace LingoEngine.Director.Core.Scores
{
    public class DirectorScoreWindow : DirectorWindow<IDirFrameworkScoreWindow> 
    {
        private readonly IDirSpritesManager _spritesManager;
        private readonly DirScoreManager _scoreManager;
        private readonly LingoPlayer _player;
        private LingoMovie? _movie;
        protected ILingoMouseSubscription _mouseSub;
        public DirScoreGridTopContainer TopContainer { get; private set; }
        public DirScoreGridSprites2DContainer Sprites2DContainer { get; private set; }
        public DirScoreGfxValues GfxValues  => _scoreManager.GfxValues;
        public float ScollX { get; set; }

#pragma warning disable CS8618
        public DirectorScoreWindow(IDirSpritesManager spritesManager, ILingoPlayer player, ILingoFrameworkFactory factory, DirScoreManager scoreManager) : base(factory)
#pragma warning restore CS8618 
        {
            _spritesManager = spritesManager;
            _scoreManager = scoreManager;
            _player = (LingoPlayer)player;
            _player.ActiveMovieChanged += OnActiveMovieChanged;
            
        }
        public override void Init(IDirFrameworkWindow frameworkWindow)
        {
            base.Init(frameworkWindow);
            _mouseSub = Mouse.OnMouseEvent(HandleMouseEvent);
            TopContainer = new DirScoreGridTopContainer(_scoreManager);
            Sprites2DContainer = new DirScoreGridSprites2DContainer(_scoreManager);
        }
        public override void Dispose()
        {
            _mouseSub.Release();
            if (_movie != null)
                _movie.CurrentFrameChanged -= OnCurrentFrameChanged;
            
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            base.Dispose();
        }


        private void OnActiveMovieChanged(ILingoMovie? movie)
        {
            if (_movie != null)
            {
                _movie.CurrentFrameChanged -= OnCurrentFrameChanged;
            }
            _movie = movie as LingoMovie;
            if (_movie != null)
            {
                _movie.CurrentFrameChanged += OnCurrentFrameChanged;
            }
            TopContainer.SetMovie(_movie);
            Sprites2DContainer.SetMovie(_movie);
        }
        

        private void HandleMouseEvent(LingoMouseEvent mouseEvent)
        {
            if (_movie == null) return;
            var gfxValues = _scoreManager.GfxValues;
            if (mouseEvent.MouseH < gfxValues.ChannelInfoWidth) return;
            if (mouseEvent.MouseV < gfxValues.LabelsBarHeight) return;

            float frameF = (mouseEvent.MouseH - gfxValues.ChannelInfoWidth - 3 + ScollX) / gfxValues.FrameWidth;
            var mouseFrame = Math.Clamp(MathL.RoundToInt(frameF) + 1, 1, _movie.FrameCount);
            
            var channel = 0;

            if (mouseEvent.MouseV <= gfxValues.LabelsBarHeight + TopContainer.Size.Y)
            {
                // Top channel
                var yPosition = mouseEvent.MouseV - gfxValues.LabelsBarHeight;
                channel = Math.Clamp(MathL.RoundToInt((yPosition + 4) / gfxValues.ChannelHeight), 1, 999);
            }
            else {
                // Sprites Container
                var topPosition = gfxValues.LabelsBarHeight + TopContainer.Size.Y + gfxValues.ChannelFramesHeight;
                if (mouseEvent.MouseV >= topPosition && mouseEvent.MouseV <= topPosition + Sprites2DContainer.Size.Y )
                {
                    var yPosition = mouseEvent.MouseV - topPosition;
                    channel = Math.Clamp(MathL.RoundToInt((yPosition + 4) / gfxValues.ChannelHeight), 1, 999) + 6;
                }
            }
            if (channel <= 0) return;
            //Console.WriteLine($"Mouse Event: Frame {mouseFrame}, Channel {channel}");
            _scoreManager.HandleMouse(mouseEvent, channel, mouseFrame);
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

       

    }
}
