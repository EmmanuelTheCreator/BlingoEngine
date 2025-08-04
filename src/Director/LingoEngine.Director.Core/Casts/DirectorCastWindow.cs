using LingoEngine.Director.Core.Windowing;
using LingoEngine.Movies;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Casts
{
    public class DirectorCastWindow : DirectorWindow<IDirFrameworkCastWindow>
    {
        public DirectorCastWindow(ILingoFrameworkFactory factory) : base(factory) { }

        public void LoadMovie(ILingoMovie lingoMovie) => Framework.SetActiveMovie(lingoMovie);
    }
}
