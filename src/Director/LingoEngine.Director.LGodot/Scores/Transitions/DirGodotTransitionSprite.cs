using LingoEngine.Transitions;

namespace LingoEngine.Director.LGodot.Scores.Transitions;

internal class DirGodotTransitionSprite : DirGodotTopSprite<LingoTransitionSprite>
{
    public DirGodotTransitionSprite(LingoTransitionSprite sprite, Core.Sprites.IDirSpritesManager spritesManager)
    {
        SpritesManager = spritesManager;
        Init(sprite);
    }
}
