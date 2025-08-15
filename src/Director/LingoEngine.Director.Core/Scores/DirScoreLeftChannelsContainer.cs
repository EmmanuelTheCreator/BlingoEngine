using LingoEngine.AbstUI.Primitives;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;

namespace LingoEngine.Director.Core.Scores
{
    public class DirScoreLeftChannelsContainer : DirScoreLeftContainer
    {
        protected LingoMovie? _movie;
   

        public DirScoreLeftChannelsContainer(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, APoint position, IDirectorEventMediator mediator) : base(gfxValues, factory, position,10, mediator)
        {
        }

        public void SetMovie(LingoMovie? movie)
        {
            _movie = movie;
            if (_movie != null)
            {
                RegenerateChannels();
            }
        }
        protected override void StagePropertiesChanged()
        {
            RegenerateChannels();
            base.StagePropertiesChanged();

        }
        private void RegenerateChannels()
        {
            if (_movie == null) return;
            var channels = new List<DirScoreChannelHeader>();
            _canvas.Height = _gfxValues.ChannelHeight * _movie.MaxSpriteChannelCount;
            for (int c = 1; c < _movie.MaxSpriteChannelCount; c++)
            {
                var ch = _movie.Channel(c);
                var header = new DirScoreChannelHeader("", c.ToString(), _gfxValues, (c, state) => { if (_movie != null) ch.Visibility = state; });
                channels.Add(header);
            }
            SetChannels(channels.ToArray());
        }

        protected override DirScoreChannelHeader GetChannel(int spriteNumWithChannel) => _channels[spriteNumWithChannel - 7];// -6 of 6 channels + -1 for 0 index array
    }
}
