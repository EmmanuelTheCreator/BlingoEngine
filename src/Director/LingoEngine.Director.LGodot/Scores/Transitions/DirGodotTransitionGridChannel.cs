using LingoEngine.Movies;
using LingoEngine.Director.LGodot.Scores.Transitions;
using LingoEngine.Transitions;
using LingoEngine.Director.Core.Sprites;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotTransitionGridChannel : DirGodotTopGridChannel<ILingoSpriteTransitionManager, DirGodotTransitionSprite, LingoTransitionSprite>
{
    public DirGodotTransitionGridChannel(IDirSpritesManager spritesManager) 
        : base(3, spritesManager)
    {
    }

    protected override DirGodotTransitionSprite CreateUISprite(LingoTransitionSprite sprite, Core.Sprites.IDirSpritesManager spritesManager) => new DirGodotTransitionSprite(sprite, spritesManager);

    protected override ILingoSpriteTransitionManager GetManager(LingoMovie movie) => movie.Transitions;
}
