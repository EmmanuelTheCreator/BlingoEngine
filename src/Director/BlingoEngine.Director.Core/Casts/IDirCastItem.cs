namespace BlingoEngine.Director.Core.Casts
{
    using BlingoEngine.Members;

    /// <summary>
    /// Common interface for cast items displayed in the cast window.
    /// </summary>
    public interface IDirCastItem
    {
        IBlingoMember? Member { get; }
        void SetSelected(bool selected);
        void SetHovered(bool hovered);
    }
}

