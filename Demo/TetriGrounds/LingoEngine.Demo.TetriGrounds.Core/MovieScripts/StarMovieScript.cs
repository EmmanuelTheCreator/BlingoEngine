using LingoEngine.Demo.TetriGrounds.Core.ParentScripts;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;

namespace LingoEngine.Demo.TetriGrounds.Core.MovieScripts
{
    // Converted from 4_StarMovie.ls
    public class StarMovieScript : LingoMovieScript, IHasStartMovieEvent, IHasStopMovieEvent
    {
        private readonly GlobalVars _global;

        public StarMovieScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

        public void StartMovie()
        {
            if (_global.SpriteManager == null)
            {
                _global.SpriteManager = new SpriteManager(_env);
                _global.SpriteManager.Init(100);
            }
            if (_global.MousePointer == null)
            {
                _global.MousePointer = new MousePointer(_env);
                _global.MousePointer.Init(90);
            }
            _Player.SoundPlayNature();
        }

        public void StopMovie()
        {
            _global.SpriteManager?.Destroy();
            _global.MousePointer?.Destroy();
            _global.SpriteManager = null;
            _global.MousePointer = null;
            _Movie.ActorList.Clear();   
        }

        public string ReplaceSpaces(string str, int leng)
        {
            string thisField = str;
            for (int i = 0; i < thisField.Length; i++)
            {
                if (thisField[i] == ' ')
                    thisField = thisField.Substring(0, i) + "_" + thisField[(i + 1)..];
            }
            if (thisField.Length > leng) thisField = thisField.Substring(0, leng);
            return thisField;
        }
    }
}
