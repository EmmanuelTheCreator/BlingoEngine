namespace LingoEngine.Animations
{
    /// <summary>
    /// Represents a position of a sprite for a specific frame.
    /// </summary>

    /* Unmerged change from project 'LingoEngine (net8.0)'
    Before:
        public record LingoSpriteMotionFrame(int Frame, Primitives.LingoPoint Position, bool IsKeyFrame);
    After:
        public record LingoSpriteMotionFrame(int Frame, LingoPoint Position, bool IsKeyFrame);
    */
    public record LingoSpriteMotionFrame(int Frame, AbstUI.Primitives.APoint Position, bool IsKeyFrame);

    /// <summary>
    /// Contains a collection of sprite motion frames describing its motion path.
    /// </summary>
    public class LingoSpriteMotionPath
    {
        public List<LingoSpriteMotionFrame> Frames { get; } = new();
    }
}
