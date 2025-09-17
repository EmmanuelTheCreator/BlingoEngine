using BlingoEngine.Core;

namespace BlingoEngine.Movies
{
    public interface IBlingoMovieScript : IBlingoScriptBase
    {
    }
    public class BlingoMovieScript : BlingoScriptBase , IBlingoMovieScript
    {
        public BlingoMovieScript(IBlingoMovieEnvironment env) : base(env)
        {
        }
    }
}

