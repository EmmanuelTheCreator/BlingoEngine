// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 20_StartData_get.ls
    public class StartDataGetScript : BlingoParentScript, IHasStepFrameEvent
    {
        private string myURL = string.Empty;
        private object? myNetID;
        private bool myDone;
        private string myErr = string.Empty;
        private string myData = string.Empty;
        private readonly object? myParent;
        private int myShowType = 1;

        public StartDataGetScript(IBlingoMovieEnvironment env, object? parent = null) : base(env)
        {
            myParent = parent;
        }

        public void SetURL(string scriptURL) => myURL = scriptURL;

        public void Download()
        {
            myErr = string.Empty;
            myDone = false;
            myData = string.Empty;
            // TODO: perform network download, store handle in myNetID
            myNetID = null;
            _Movie.ActorList.Add(this);
        }

        public void StepFrame()
        {
            // TODO: check network status via myNetID
            if (myParent != null)
            {
                var mi = myParent.GetType().GetMethod("DataLoaded");
                mi?.Invoke(myParent, new object[] { myData, this });
            }
            myDone = true;
            _Movie.ActorList.Remove(this);
        }

        public string GetData() => myData;
        public string GetErr() => myErr;
        public bool IsDone() => myDone;

        public void Destroy()
        {
            _Movie.ActorList.Remove(this);
        }
    }
}

