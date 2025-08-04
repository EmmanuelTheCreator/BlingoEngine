using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Events;
using LingoEngine.Gfx;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Primitives;



namespace LingoEngine.Director.Core.Scores
{
    public interface IDirScoreFrameworkGridContainer
    {
        float CurrentFrameX { get; set; }
        void RequireRedrawChannels();
        void UpdateSize();
    }
    public abstract class DirScoreGridContainer : IDisposable
    {
        protected IDirScoreFrameworkGridContainer _framework;
        protected readonly IDirSpritesManager _spritesManager;
        protected readonly IDirScoreManager _scoreManager;
        protected readonly DirScoreGfxValues _gfxValues;
        protected readonly ILingoMouse _mouse;
        protected readonly DirScoreGridPainter _gridCanvas;
        protected readonly LingoGfxCanvas _canvasCurrentFrame;
        protected LingoMovie? _movie;
        protected ILingoMouseSubscription _mouseSub;
        protected int _currentFrame;
        protected Dictionary<int,DirScoreChannel> _channelsDic = new();
        protected DirScoreChannel[] _channels = [];

        public LingoPoint Position { get; set; }
        public LingoPoint Size { get; set; }
       
        public LingoGfxCanvas CanvasGridLines => _gridCanvas.Canvas;
        public LingoGfxCanvas CanvasCurrentFrame => _canvasCurrentFrame;
        public DirScoreChannel[] Channels => _channels;



#pragma warning disable CS8618 
        public DirScoreGridContainer(IDirScoreManager scoreManager, ILingoMouse mouse, int channelCount)
#pragma warning restore CS8618 
        {
            _spritesManager = scoreManager.SpritesManager;
            _scoreManager = scoreManager;
            _gfxValues = _spritesManager.GfxValues;
            _mouse = mouse;
            _gridCanvas = new DirScoreGridPainter(_spritesManager.Factory, _spritesManager.GfxValues);
            _mouseSub = _mouse.OnMouseEvent(HandleMouseEvent);
            _canvasCurrentFrame = _spritesManager.Factory.CreateGfxCanvas("CurrentTimeLineTop" , 2, _spritesManager.GfxValues.ChannelHeight* channelCount);
            _canvasCurrentFrame.DrawLine(new LingoPoint(0, 0), new LingoPoint(0, _canvasCurrentFrame.Height), LingoColorList.Red, 2);
        }

        public void Init(IDirScoreFrameworkGridContainer framework)
        { 
            _framework = framework;
        }

        public void Dispose()
        {
            if (_movie != null)
                _movie.CurrentFrameChanged -= CurrentFrameChanged;
            _mouseSub.Release();
            foreach (var channel in _channels)
                channel.Dispose();
            _canvasCurrentFrame.Dispose();
        }

        protected void SetChannels(DirScoreChannel[] channels)
        {
            _channels = channels;
            _channelsDic = _channels.ToDictionary(x => x.SpriteNum);
        }

        protected void UpdateChannelsPosition()
        {
            float y = 0;
            for (int i = 0; i < _channels.Length; i++)
            {
                var ch = _channels[i];
                if (!ch.Visible) continue;
                ch.Position = new LingoPoint(0, y);
                y += _gfxValues.ChannelHeight;
            }
        }

        protected virtual DirScoreChannel? GetChannelByDisplayIndex(int index)
        {
            if (index < 0 || index >= _channels.Length) return null;
            return _channelsDic[index+1];
        }

        public void HandleMouseEvent(LingoMouseEvent mouseEvent)
        {
            
            if (_movie == null) return;
            float frameF = (mouseEvent.MouseH + _gfxValues.ChannelInfoWidth - 3) / _gfxValues.FrameWidth;
            var mouseFrame = Math.Clamp(MathL.RoundToInt(frameF) + 1, 1, _movie.FrameCount);
            var displayIndex = Math.Clamp(MathL.RoundToInt((mouseEvent.MouseV - Position.Y + 4 - _gfxValues.ChannelHeight) / _gfxValues.ChannelHeight), 1, 999) -1;
            var ch = GetChannelByDisplayIndex(displayIndex);
            if (ch != null)
                _spritesManager.ScoreManager.HandleMouse(mouseEvent, ch.SpriteNum, mouseFrame);
        }

        public virtual void SetMovie(LingoMovie? movie)
        {
            _movie = movie;
            if (_channels == null || _channels.Length == 0)
                return;
            
            foreach (var ch in _channels)
                ch.SetMovie(movie);
            UpdateSize();
            UpdateChannelsPosition();
            if (_currentFrame <=0)
                CurrentFrameChanged(1);
        }

        public void CurrentFrameChanged(int currentFrame)
        {
            _currentFrame = currentFrame;
            int cur = currentFrame - 1;
            if (cur < 0) cur = 0;
            _framework.CurrentFrameX = _gfxValues.LeftMargin + cur * _gfxValues.FrameWidth + _gfxValues.FrameWidth / 2f -1;
        }

        protected void UpdateSize()
        {
            if (_movie == null)
                return;
            float width = _gfxValues.LeftMargin + _movie.FrameCount * _gfxValues.FrameWidth;
            int visibleCount = _channels.Count(ch => ch.Visible);
            float height = visibleCount * _gfxValues.ChannelHeight;
            Size = new LingoPoint(width, height);
            _gridCanvas.FrameCount = _movie.FrameCount;
            _gridCanvas.ChannelCount = visibleCount;
            _gridCanvas.Draw();
            foreach (var ch in _channels)
                ch.UpdateSize();
            UpdateChannelsPosition();
            _framework.UpdateSize();
            _framework.RequireRedrawChannels();
        }
    }

}
