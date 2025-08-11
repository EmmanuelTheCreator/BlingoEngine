using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;


namespace LingoEngine.Director.Core.Scores
{
    public abstract class DirScoreLeftContainer : IDisposable
    {

        protected readonly DirScoreGfxValues _gfxValues;
        protected LingoPoint _position;
        private readonly IDirectorEventMediator _mediator;
        protected readonly LingoGfxCanvas _canvas;
        private readonly IDirectorEventSubscription _mediatorSub;
        protected DirScoreChannelHeader[] _channels = [];
        
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public ILingoFrameworkGfxNode FrameworkGfxNode => _canvas.FrameworkObj;
        public LingoGfxCanvas Canvas => _canvas;

        public DirScoreLeftContainer(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, LingoPoint position, int initialChannelCount, IDirectorEventMediator mediator)
        {
            _gfxValues = gfxValues;
            _position = position;
            _mediator = mediator;
            Width = _gfxValues.ChannelInfoWidth;
            Height = _gfxValues.ChannelHeight * initialChannelCount;
            _canvas = factory.CreateGfxCanvas("ScoreTopHeaderContainer", Width, Height);
            _mediatorSub = _mediator.Subscribe(DirectorEventType.StagePropertiesChanged, () =>
            {
                StagePropertiesChanged();
                return true;
            });
        }

        public void Dispose()
        {
            _mediatorSub.Release();
            _canvas.Dispose();
        }

        protected virtual void StagePropertiesChanged()
        {
        }
        public void SetChannels(DirScoreChannelHeader[] channels)
        {
            _channels = channels;
            Height = _gfxValues.ChannelHeight * _channels.Length;
            _canvas.Height = Height;
            foreach (var (h, idx) in _channels.Select((h, i) => (h, i)))
                h.Y = idx * _gfxValues.ChannelHeight;
            Draw();
        }

        

        protected virtual void Draw()
        {
            _canvas.Clear(DirectorColors.BG_WhiteMenus);
            foreach (var header in _channels)
            {
                if (!header.Visible) continue;
                header.Y = header.Y;
                header.Draw(_canvas);
            }
        }
        public void UpdatePosition(LingoPoint position)
        {
            _position = position;
            _canvas.Y = position.Y;
        }
        public void RaiseMouseDown(LingoMouseEvent mouse, int spriteNumWithChannel)
        {
            var header = GetChannel(spriteNumWithChannel);
            header.ToggleMute();
            Draw();
        }
        protected abstract DirScoreChannelHeader GetChannel(int spriteNumWithChannel);
        

        
    }
}
