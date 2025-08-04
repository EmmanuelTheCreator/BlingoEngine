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
        private readonly DirScoreManager _scoreManager;
        private readonly LingoPlayer _player;
        private LingoMovie? _movie;

        public DirScoreTopGridContainer TopContainer { get; private set; }

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
            TopContainer = new DirScoreTopGridContainer(_scoreManager, Mouse);
        }
        private void OnActiveMovieChanged(ILingoMovie? movie)
        {
            if (_movie != null)
                _movie.CurrentFrameChanged -= CurrentFrameChanged;
            _movie = movie as LingoMovie;
            if (_movie != null)
                _movie.CurrentFrameChanged += CurrentFrameChanged;
            TopContainer.SetMovie(_movie);
        }

        public void CurrentFrameChanged(int currentFrame)
        {
            TopContainer.CurrentFrameChanged(currentFrame);
        }

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
