namespace BlingoEngine.Sprites
{
    /// <summary>
    /// Lingo Sprites Player interface.
    /// </summary>
    public interface IBlingoSpritesPlayer
    {
        public int CurrentFrame { get; }

        int GetMaxLocZ();
    }
}

