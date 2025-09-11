using LingoEngine.Core;

namespace LingoEngine.Demo.TetriGrounds.Core
{
    public class GlobalVars : LingoGlobalVars
    {
        public string LastInfo { get; internal set; } = string.Empty;

        // Globals created in StarMovieScript
        public ParentScripts.SpriteManager? SpriteManager { get; set; }
        public ParentScripts.MousePointer? MousePointer { get; set; }
        public bool GameIsRunning { get; internal set; }
        public string PlayerName { get; internal set; } = "";

        protected override void OnClearGlobals()
        {
            base.OnClearGlobals();
            SpriteManager = null;
            MousePointer = null;
            GameIsRunning = false;
            LastInfo = string.Empty;
        }
    }
    public class ScoresContent
    {
        public DateTime LastUpdated { get; set; }
        public record ScoreEntry(string PlayerName, int Score, DateTime when, DateTime duration);
        public List<ScoresContent.ScoreEntry> Scores { get; set; } = [];
    }
    public class TetrigroundsRootJson
    {
        public ScoresContent Scores { get; set; } = new();
        public string Version { get; set; } = "1.0";
    }


}

