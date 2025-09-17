// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;

namespace BlingoEngine.Demo.TetriGrounds.Core
{
    public class GlobalVars : BlingoGlobalVars
    {
        public string LastInfo { get; internal set; } = string.Empty;

        // Globals created in StarMovieScript
        public ParentScripts.SpriteManager? SpriteManager { get; set; }
        public ParentScripts.MousePointer? MousePointer { get; set; }
        public bool GameIsRunning { get; internal set; }
        public string PlayerName { get; internal set; } = "";
        public TetrigroundsRootJson.ScoresContent Scores { get; internal set; } = new();

        protected override void OnClearGlobals()
        {
            base.OnClearGlobals();
            SpriteManager = null;
            MousePointer = null;
            GameIsRunning = false;
            LastInfo = string.Empty;
        }
    }


}


