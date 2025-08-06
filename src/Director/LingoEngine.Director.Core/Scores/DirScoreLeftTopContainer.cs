using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Primitives;


namespace LingoEngine.Director.Core.Scores
{
    public class DirScoreLeftTopContainer : DirScoreLeftContainer
    {
        private float _frameScriptsTopOpen = 0;
        private readonly DirScoreChannelHeader[] _collapsableHeaders;
        private bool _collapsed;
        private DirScoreChannelHeader _frameScript;
        protected LingoMovie? _movie;
        public bool Collapsed
        {
            get => _collapsed;
            set
            {
                _collapsed = value;
                foreach (var collapsableHeader in _collapsableHeaders)
                    collapsableHeader.Visible = !value;
                _frameScript.Y = !value ? _frameScriptsTopOpen : 0;
                Height = !value ? _gfxValues.ChannelHeight * _channels.Length : _gfxValues.ChannelHeight;
                Canvas.Height = Height;
                Draw();
            }
        }

        public DirScoreLeftTopContainer(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, LingoPoint position, IDirectorEventMediator mediator) : base(gfxValues, factory, position,6, mediator)
        {
            
            _frameScript = new DirScoreChannelHeader("", "Scripts", gfxValues, (c, state) => { });
            DirScoreChannelHeader[] headers =
            [
                new DirScoreChannelHeader("⏱","",gfxValues, (c,state) => { if(_movie != null) _movie.Tempos.MuteChannel(1,state); }),
                new DirScoreChannelHeader("🎨","",gfxValues, (c,state) => { if(_movie != null) _movie.ColorPalettes.MuteChannel(1,state); }),
                new DirScoreChannelHeader("➡","",gfxValues, (c,state) => { if(_movie != null) _movie.Transitions.MuteChannel(1,state); }),
                new DirScoreChannelHeader("🔊","1", gfxValues, (c,state) => { if(_movie != null) _movie.Audio.MuteChannel(1,state); c.Icon = state ? "🔇" : "🔊"; }),
                new DirScoreChannelHeader("🔊","2", gfxValues, (c,state) => { if(_movie != null) _movie.Audio.MuteChannel(2,state); c.Icon = state ? "🔇" : "🔊";}),
               _frameScript,
            ];
            SetChannels(headers);

            _collapsableHeaders = headers.Take(_channels.Length - 1).ToArray();
            
            _frameScriptsTopOpen = _frameScript.Y;
        }
        public void SetMovie(LingoMovie? movie)
        {
            _movie = movie;
        }
        protected override DirScoreChannelHeader GetChannel(int spriteNumWithChannel) => _collapsed ? _frameScript : _channels[spriteNumWithChannel-1];
    } 
}
