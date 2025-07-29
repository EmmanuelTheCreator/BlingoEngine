using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Inputs;
using LingoEngine.Primitives;


namespace LingoEngine.Director.Core.Scores
{
    public class DirScoreLeftContainer : IDisposable
    {

        protected readonly DirScoreGfxValues _gfxValues;
        protected readonly ILingoMouse _mouse;
        protected LingoPoint _position;
        private readonly IDirectorEventMediator _mediator;
        protected readonly LingoGfxCanvas _canvas;
        private readonly IDirectorEventSubscription _mediatorSub;
        protected DirScoreChannelHeader[] _channels = [];
        
        private ILingoMouseSubscription _mouseSub;
        public Action? RequestRedraw { get; set; }
        

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public ILingoFrameworkGfxNode FrameworkGfxNode => _canvas.FrameworkObj;

        public DirScoreLeftContainer(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, ILingoMouse mouse, LingoPoint position, int initialChannelCount, IDirectorEventMediator mediator)
        {
            _gfxValues = gfxValues;
            _mouse = mouse;
            _position = position;
            _mediator = mediator;
            Width = _gfxValues.ChannelInfoWidth;
            Height = _gfxValues.ChannelHeight * initialChannelCount;
            _mouseSub = _mouse.OnMouseDown(RaiseMouseDown);
            _canvas = factory.CreateGfxCanvas("ScoreTopHeaderContainer", Width, Height);
            _mediatorSub = _mediator.Subscribe(DirectorEventType.StagePropertiesChanged, () =>
            {
                StagePropertiesChanged();
                return true;
            });
        }
        protected virtual void StagePropertiesChanged()
        {
            RequestRedraw?.Invoke();
        }
        public void SetChannels(DirScoreChannelHeader[] channels)
        {
            _channels = channels;
            Height = _gfxValues.ChannelHeight * _channels.Length;
            _canvas.Height = Height;
            foreach (var (h, idx) in _channels.Select((h, i) => (h, i)))
                h.Y = idx * _gfxValues.ChannelHeight;
        }

        

        public void Draw()
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
        }
        private void RaiseMouseDown(LingoMouseEvent mouse)
        {
            var mouseX = mouse.MouseH - _position.X;
            var mouseY = mouse.MouseV - _position.Y;
            //Console.WriteLine("D:" + mouse.MouseH + "x" + mouse.MouseV + ":" + mouseX + "x" + mouseY);

            if (mouseX >= _gfxValues.ChannelHeight + 2 || mouseY >= Height)
                return;

            var layer = Math.Floor(mouseY / _gfxValues.ChannelHeight) ;
            if (layer < 0 || layer >= _channels.Length)
                return;

            var header = GetChannel((int)layer);
            header.ToggleMute();
            Draw();
        }
        protected virtual DirScoreChannelHeader GetChannel(int layer) => _channels[layer];
        

        public void Dispose()
        {
            _mediatorSub.Release();
            _mouseSub.Release();
        }
    }
}
