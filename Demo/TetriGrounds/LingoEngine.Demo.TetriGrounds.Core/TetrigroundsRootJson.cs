namespace LingoEngine.Demo.TetriGrounds.Core
{
    public class TetrigroundsRootJson
    {
        public ScoresContent ScoresData { get; set; } = new();
        public string Version { get; set; } = "1.0";
        public record ScoreEntry(string PlayerName, int Score, DateTime when, TimeSpan duration, int Level);
        public record ScoresContent
        {
            public DateTime LastUpdated { get; set; }
            public List<ScoreEntry> Scores { get; set; } = [];
        }
    }


}

