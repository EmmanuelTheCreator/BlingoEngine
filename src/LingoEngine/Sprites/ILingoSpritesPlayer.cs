namespace LingoEngine.Sprites
{
    /// <summary>
    /// Lingo Sprites Player interface.
    /// </summary>
    public interface ILingoSpritesPlayer
    {
        public int CurrentFrame { get; }

        int GetMaxLocZ();
    }
}
