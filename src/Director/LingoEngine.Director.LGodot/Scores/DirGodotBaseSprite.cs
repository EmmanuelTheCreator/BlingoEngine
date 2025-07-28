using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Base drawable element for score channels.
/// Provides common properties for dragging and selection.
/// </summary>
internal abstract class DirGodotBaseSprite
{
    internal bool Selected;

    /// <summary>Frame where the element begins.</summary>
    internal abstract int BeginFrame { get; set; }
    /// <summary>Frame where the element ends.</summary>
    internal abstract int EndFrame { get; set; }

    /// <summary>Draw the sprite at the given position.</summary>
    internal abstract void Draw(CanvasItem canvas, Vector2 position, float width, float height, Font font);

    /// <summary>Move the element so it starts at the given frame.</summary>
    internal virtual void MoveToFrame(int frame)
    {
        int length = EndFrame - BeginFrame;
        BeginFrame = frame;
        EndFrame = frame + length;
    }

    /// <summary>Remove the element from the provided movie.</summary>
    internal virtual void DeleteFromMovie(LingoMovie movie) { }
}
