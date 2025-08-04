using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Core;

namespace LingoEngine.Director.Core.Scores
{
    public class DirectorScoreWindow : DirectorWindow<IDirFrameworkScoreWindow>
    {
        private readonly IDirSpritesManager _spritesManager;
        private readonly LingoPlayer _player;
        private LingoMovie? _movie;

        public DirectorScoreWindow(IDirSpritesManager spritesManager, ILingoPlayer player, ILingoFrameworkFactory factory) : base(factory)
        {
            _spritesManager = spritesManager;
            _player = (LingoPlayer)player;
            _player.ActiveMovieChanged += OnActiveMovieChanged;
        }

        private void OnActiveMovieChanged(ILingoMovie? movie) => _movie = movie as LingoMovie;

        protected override void OnRaiseKeyDown(LingoKey lingoKey)
        {
            if (_movie != null && string.Equals(lingoKey.Key, "Delete", StringComparison.OrdinalIgnoreCase))
                _spritesManager.DeleteSelected(_movie);
        }

        protected override void OnRaiseKeyUp(LingoKey lingoKey) { }

        public override void Dispose()
        {
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            base.Dispose();
        }
    }
}
