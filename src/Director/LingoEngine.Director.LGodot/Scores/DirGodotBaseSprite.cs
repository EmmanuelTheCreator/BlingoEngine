using Godot;
using LingoEngine.Movies;
using LingoEngine.Sprites;

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


internal abstract class DirGodotBaseSprite<TSprite> : DirGodotBaseSprite
    where TSprite : LingoSprite
{
    protected TSprite _sprite;
    public TSprite Sprite => _sprite;
    public string DrawLabel { get; set; }
    internal override int BeginFrame { get => _sprite.BeginFrame; set => _sprite.BeginFrame = value; }
    internal override int EndFrame { get => _sprite.EndFrame; set => _sprite.EndFrame = value; }
#pragma warning disable CS8618
    public DirGodotBaseSprite()
#pragma warning restore CS8618 
    { }
    internal void Init(TSprite sprite)
    {
        _sprite = sprite;
        DrawLabel = sprite.Name;
    }
    internal override void DeleteFromMovie(LingoMovie movie)
    {
        // Removing a sprite removes it from movie timeline
        Sprite.RemoveMe();
    }
}



