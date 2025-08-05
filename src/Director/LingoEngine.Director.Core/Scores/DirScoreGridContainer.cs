using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Windowing;
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
        void RequireRecreateChannels();
        void UpdateSize();
    }
    public abstract class DirScoreGridContainer : IDisposable
    {
        protected IDirScoreFrameworkGridContainer? _framework;
        protected readonly IDirScoreManager _scoreManager;
        private readonly Func<string, ILingoFrameworkGfxPanel, IDirectorWindowDialogReference?> _showConfirmDialog;
        protected readonly DirScoreGfxValues _gfxValues;
        protected readonly DirScoreGridPainter _gridCanvas;
        protected readonly LingoGfxCanvas _canvasCurrentFrame;
        protected LingoMovie? _movie;
        
        protected int _currentFrame = -1;
        protected Dictionary<int,DirScoreChannel> _channelsDic = new();
        protected DirScoreChannel[] _channels = [];

        public float CurrentFrameX { get; set; }
        public LingoPoint Position { get; set; }
        public LingoPoint Size { get; set; }
       
        public LingoGfxCanvas CanvasGridLines => _gridCanvas.Canvas;
        public LingoGfxCanvas CanvasCurrentFrame => _canvasCurrentFrame;
        public DirScoreChannel[] Channels => _channels;



        public DirScoreGridContainer(IDirScoreManager scoreManager,int channelCount, Func<string, ILingoFrameworkGfxPanel, Windowing.IDirectorWindowDialogReference?> showConfirmDialog)
        {
            _scoreManager = scoreManager;
            _showConfirmDialog = showConfirmDialog;
            _gfxValues = _scoreManager.GfxValues;
            _gridCanvas = new DirScoreGridPainter(scoreManager.Factory, _gfxValues);
            _canvasCurrentFrame = scoreManager.Factory.CreateGfxCanvas("CurrentTimeLineTop" , 2, _gfxValues.ChannelHeight* channelCount);
            RedrawCurrentFrameVLine(channelCount);
        }

        public void Init(IDirScoreFrameworkGridContainer framework)
        { 
            _framework = framework;
        }

        public virtual void Dispose()
        {
            if (_movie != null)
                _movie.CurrentFrameChanged -= CurrentFrameChanged;
           
            foreach (var channel in _channels)
                channel.Dispose();
            _canvasCurrentFrame.Dispose();
        }
        private void RedrawCurrentFrameVLine(int channelCount)
        {
            _canvasCurrentFrame.Height = _scoreManager.GfxValues.ChannelHeight * channelCount;
            _canvasCurrentFrame.DrawLine(new LingoPoint(0, 0), new LingoPoint(0, _canvasCurrentFrame.Height), LingoColorList.Red, 2);
        }
        protected void SetChannels(DirScoreChannel[] channels)
        {
            if (_channels.Length > 0)
            {
                foreach (var ch in _channels)
                    ch.Dispose();
            }
            _channels = channels;
            _channelsDic = _channels.ToDictionary(x => x.SpriteNumWithChannelNum);
            foreach (var channel in _channels)
            {
                channel.SetShowDialogMethod(_showConfirmDialog);
            }
            _framework?.RequireRecreateChannels();
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

        //public void HandleMouseEvent(LingoMouseEvent mouseEvent)
        //{
            
        //    if (_movie == null) return;
        //    float frameF = (mouseEvent.MouseH + _gfxValues.ChannelInfoWidth - 3) / _gfxValues.FrameWidth;
        //    var mouseFrame = Math.Clamp(MathL.RoundToInt(frameF) + 1, 1, _movie.FrameCount);
        //    var displayIndex = Math.Clamp(MathL.RoundToInt((mouseEvent.MouseV - Position.Y + 4 - _gfxValues.ChannelHeight) / _gfxValues.ChannelHeight), 1, 999) -1;
        //    var ch = GetChannelByDisplayIndex(displayIndex);
        //    if (ch != null)
        //        _spritesManager.ScoreManager.HandleMouse(mouseEvent, ch.SpriteNumWithChannelNum, mouseFrame);
        //}

        public virtual void SetMovie(LingoMovie? movie)
        {
            _movie = movie;
            if (_currentFrame <= 0)
                CurrentFrameChanged(1);
            if (_channels == null || _channels.Length == 0)
                return;
            
            foreach (var ch in _channels)
                ch.SetMovie(movie);
            UpdateSize();
            UpdateChannelsPosition();
            
        }

        public void CurrentFrameChanged(int currentFrame)
        {
            int cur = currentFrame - 1;
            if (cur < 0) cur = 0;
            CurrentFrameX = _gfxValues.LeftMargin + cur * _gfxValues.FrameWidth + _gfxValues.FrameWidth / 2f - 1; 
            if (_framework != null)
            {
                _framework.CurrentFrameX = CurrentFrameX;
                _currentFrame = currentFrame;
            }
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
            RedrawCurrentFrameVLine(_channels.Length);
            if (_framework != null)
            {
                _framework.UpdateSize();
                _framework.RequireRedrawChannels();
            }
        }
    }

}
