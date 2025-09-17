using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Scores.Sprites2D
{
    internal class DirScoreSprite2D : DirScoreSprite<BlingoSprite2D>
    {
        
        public DirScoreSprite2D(BlingoSprite2D sprite, Sprites.IDirSpritesManager spritesManager)
            : base(sprite, spritesManager)
        {
            //Init(sprite);
        }
    }
}


