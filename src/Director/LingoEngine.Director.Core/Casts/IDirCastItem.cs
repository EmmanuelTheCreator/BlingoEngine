namespace LingoEngine.Director.Core.Casts
{
    using LingoEngine.Members;

    /// <summary>
    /// Common interface for cast items displayed in the cast window.
    /// </summary>
    public interface IDirCastItem
    {
        ILingoMember? Member { get; }
        void SetSelected(bool selected);
        void SetHovered(bool hovered);
    }
}
