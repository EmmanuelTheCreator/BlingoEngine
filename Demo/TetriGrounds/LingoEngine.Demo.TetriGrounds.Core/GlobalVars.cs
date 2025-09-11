using LingoEngine.Core;

namespace LingoEngine.Demo.TetriGrounds.Core
{
    public class GlobalVars : LingoGlobalVars
    {
        public string LastInfo { get; internal set; } = "";

        // Globals created in StarMovieScript
        public ParentScripts.SpriteManager? SpriteManager { get; set; }
        public ParentScripts.MousePointer? MousePointer { get; set; }
        public bool GameIsRunning { get; internal set; }
    }
}

