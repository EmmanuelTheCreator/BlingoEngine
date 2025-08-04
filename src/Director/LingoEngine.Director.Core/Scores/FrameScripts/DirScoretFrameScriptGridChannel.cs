using LingoEngine.Movies;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Scripts;

namespace LingoEngine.Director.Core.Scores.FrameScripts;

internal partial class DirScoreFrameScriptGridChannel : DirScoreChannel<ILingoFrameScriptSpriteManager, DirScoreFrameScriptSprite, LingoSpriteFrameScript>
{

    protected override DirScoreFrameScriptSprite CreateUISprite(LingoSpriteFrameScript sprite, IDirSpritesManager spritesManager) => new DirScoreFrameScriptSprite(sprite, spritesManager);

    protected override ILingoFrameScriptSpriteManager GetManager(LingoMovie movie) => movie.FrameScripts;

    public DirScoreFrameScriptGridChannel(IDirScoreManager scoreManager)
        : base(LingoSpriteFrameScript.SpriteNumOffset+1, scoreManager)
    {
    }

}
