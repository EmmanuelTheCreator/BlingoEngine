using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Scores
{
    public class DirScoreChannelsHeaderContainer : DirScoreHeaderContainer
    {
        protected LingoMovie? _movie;
   

        public DirScoreChannelsHeaderContainer(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, ILingoMouse mouse, LingoPoint position) : base(gfxValues, factory, mouse, position,10)
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
    }
}
