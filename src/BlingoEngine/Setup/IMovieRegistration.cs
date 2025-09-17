using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
namespace BlingoEngine.Setup
{
    /// <summary>
    /// Movie Registration interface.
    /// </summary>
    public interface IMovieRegistration
    {
        IMovieRegistration AddBehavior<T>() where T : BlingoSpriteBehavior;
        IMovieRegistration AddParentScript<T>() where T : BlingoParentScript;
        IMovieRegistration AddMovieScript<T>() where T : BlingoMovieScript;
    }
}


