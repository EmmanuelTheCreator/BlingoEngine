using BlingoEngine.Sounds;

namespace BlingoEngine.Director.Core.Scores.Sounds;

/// <summary>
/// Drawable tempo keyframe.
/// </summary>
internal class DirScoreSoundSprite : DirScoreSprite<BlingoSpriteSound>
{
    
    public DirScoreSoundSprite(BlingoSpriteSound sprite, Sprites.IDirSpritesManager spritesManager)
        : base(sprite, spritesManager)
    {
        //Init(sprite);
    }
}

