using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Scores.Sprites2D
{
    internal class DirScoreSprite2D : DirScoreSprite<LingoSprite2D>
    {
        public const int FrameScriptSpriteNum = 1;
        public DirScoreSprite2D(LingoSprite2D sprite, Sprites.IDirSpritesManager spritesManager)
            : base(sprite, spritesManager)
        {
            //Init(sprite);
        }
    }
}

