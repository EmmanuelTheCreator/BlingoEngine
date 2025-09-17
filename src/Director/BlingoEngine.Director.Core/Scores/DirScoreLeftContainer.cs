using AbstUI.Components;
using AbstUI.Components.Graphics;
using AbstUI.Inputs;
using AbstUI.Primitives;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Events;
using BlingoEngine.FrameworkCommunication;


namespace BlingoEngine.Director.Core.Scores
{
    public abstract class DirScoreLeftContainer : IDisposable
    {

        protected readonly DirScoreGfxValues _gfxValues;
        protected APoint _position;
        private readonly IDirectorEventMediator _mediator;
        protected readonly AbstGfxCanvas _canvas;
        private readonly IDirectorEventSubscription _mediatorSub;
        protected DirScoreChannelHeader[] _channels = [];
        
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public IAbstFrameworkNode FrameworkGfxNode => _canvas.FrameworkObj;
        public AbstGfxCanvas Canvas => _canvas;

        public DirScoreLeftContainer(DirScoreGfxValues gfxValues, IBlingoFrameworkFactory factory, APoint position, int initialChannelCount, IDirectorEventMediator mediator)
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
        public void UpdatePosition(APoint position)
        {
            _position = position;
            _canvas.Y = position.Y;
        }
        public void RaiseMouseDown(AbstMouseEvent mouse, int spriteNumWithChannel)
        {
            var header = GetChannel(spriteNumWithChannel);
            header.ToggleMute();
            Draw();
        }
        protected abstract DirScoreChannelHeader GetChannel(int spriteNumWithChannel);
        

        
    }
}

