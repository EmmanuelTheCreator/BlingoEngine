using Godot;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Sprites;
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
   
}


internal abstract class DirGodotBaseSprite<TSprite> : DirGodotBaseSprite
    where TSprite : LingoSprite
{
    protected TSprite _sprite;
    public TSprite Sprite => _sprite;
    public DirScoreSprite SpriteUI { get; private set; }
    internal override int BeginFrame { get => _sprite.BeginFrame; set => _sprite.BeginFrame = value; }
    internal override int EndFrame { get => _sprite.EndFrame; set => _sprite.EndFrame = value; }
#pragma warning disable CS8618
    public DirGodotBaseSprite()
#pragma warning restore CS8618 
    { }

    public IDirSpritesManager SpritesManager { get; set; }
    internal void Init(TSprite sprite)
    {
        _sprite = sprite;
        SpriteUI = new DirScoreSprite(sprite, SpritesManager);
        //SpritesManager.ScoreManager.RegisterSprite(SpriteUI);
    }
   
}



