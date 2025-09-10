using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using System;
#pragma warning disable IDE1006 // Naming Styles
namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 18_score_save.ls
    public class ScoreSaveScript : LingoParentScript, IHasStepFrameEvent
    {
        private string myURL = string.Empty;
        private object? myNetID;
        private bool myDone;
        private string myErr = string.Empty;
        private int phpErr;
        private readonly ClassSubscribeScript ancestor;

        public ScoreSaveScript(ILingoMovieEnvironment env) : base(env)
        {
            ancestor = new ClassSubscribeScript(env);
        }

        public void SetURL(string scriptURL) => myURL = scriptURL;

        public void PostScore(string name, int score)
        {
          
        }

        public void StepFrame()
        {
            // TODO: check network status via myNetID
            myDone = true;
            _Movie.ActorList.Remove(this);
        }

        public string GetErr() => myErr;
        public int GetPhpErr() => phpErr;
        public bool IsDone() => myDone;

        public void Destroy()
        {
            ancestor.SubscribersDestroy();
            _Movie.ActorList.Remove(this);
        }

        public int Encryptke(string name, int score)
        {
           
            return 0;
        }
    }
}
