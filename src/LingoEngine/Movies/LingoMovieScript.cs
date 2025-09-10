using LingoEngine.Core;

namespace LingoEngine.Movies
{
    public interface ILingoMovieScript : ILingoScriptBase
    {
    }
    public class LingoMovieScript : LingoScriptBase , ILingoMovieScript
    {
        public LingoMovieScript(ILingoMovieEnvironment env) : base(env)
        {
        }
    }
}
