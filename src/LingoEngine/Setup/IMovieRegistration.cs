using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Sprites;
namespace LingoEngine.Setup
{
    /// <summary>
    /// Movie Registration interface.
    /// </summary>
    public interface IMovieRegistration
    {
        IMovieRegistration AddBehavior<T>() where T : LingoSpriteBehavior;
        IMovieRegistration AddParentScript<T>() where T : LingoParentScript;
        IMovieRegistration AddMovieScript<T>() where T : LingoMovieScript;
    }
}

