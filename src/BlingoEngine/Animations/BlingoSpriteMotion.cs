namespace BlingoEngine.Animations
{
    /// <summary>
    /// Represents a position of a sprite for a specific frame.
    /// </summary>

    /* Unmerged change from project 'BlingoEngine (net8.0)'
    Before:
        public record BlingoSpriteMotionFrame(int Frame, Primitives.BlingoPoint Position, bool IsKeyFrame);
    After:
        public record BlingoSpriteMotionFrame(int Frame, BlingoPoint Position, bool IsKeyFrame);
    */
    public record BlingoSpriteMotionFrame(int Frame, AbstUI.Primitives.APoint Position, bool IsKeyFrame);

    /// <summary>
    /// Contains a collection of sprite motion frames describing its motion path.
    /// </summary>
    public class BlingoSpriteMotionPath
    {
        public List<BlingoSpriteMotionFrame> Frames { get; } = new();
    }
}

