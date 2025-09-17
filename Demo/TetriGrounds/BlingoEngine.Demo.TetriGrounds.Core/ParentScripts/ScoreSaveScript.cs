// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using System;
#pragma warning disable IDE1006 // Naming Styles
namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 18_score_save.ls
    public class ScoreSaveScript : BlingoParentScript, IHasStepFrameEvent
    {
        private string myURL = string.Empty;
        private object? myNetID;
        private bool myDone;
        private string myErr = string.Empty;
        private int phpErr;
        private readonly ClassSubscribeScript ancestor;

        public ScoreSaveScript(IBlingoMovieEnvironment env) : base(env)
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

