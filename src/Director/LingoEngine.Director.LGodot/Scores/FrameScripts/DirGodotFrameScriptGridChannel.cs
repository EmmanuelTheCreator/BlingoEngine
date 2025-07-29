using LingoEngine.Movies;
using LingoEngine.Director.LGodot.Scores.FrameScripts;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Scripts;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotFrameScriptGridChannel : DirGodotTopGridChannel<ILingoFrameScriptSpriteManager, DirGodotFrameScriptSprite, LingoSpriteFrameScript>
{

    protected override DirGodotFrameScriptSprite CreateUISprite(LingoSpriteFrameScript sprite, IDirSpritesManager spritesManager) => new DirGodotFrameScriptSprite(sprite, spritesManager);

    protected override ILingoFrameScriptSpriteManager GetManager(LingoMovie movie) => movie.FrameScripts;

    public DirGodotFrameScriptGridChannel(int spriteNum, IDirSpritesManager spritesManager)
        : base(spriteNum, spritesManager)
    {
    }

}
