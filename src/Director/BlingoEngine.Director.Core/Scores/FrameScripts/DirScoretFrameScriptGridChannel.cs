using BlingoEngine.Movies;
using BlingoEngine.Director.Core.Sprites;
using BlingoEngine.Scripts;

namespace BlingoEngine.Director.Core.Scores.FrameScripts;

internal partial class DirScoreFrameScriptGridChannel : DirScoreChannel<IBlingoFrameScriptSpriteManager, DirScoreFrameScriptSprite, BlingoFrameScriptSprite>
{

    protected override DirScoreFrameScriptSprite CreateUISprite(BlingoFrameScriptSprite sprite, IDirSpritesManager spritesManager) => new DirScoreFrameScriptSprite(sprite, spritesManager);

    protected override IBlingoFrameScriptSpriteManager GetManager(BlingoMovie movie) => movie.FrameScripts;

    public DirScoreFrameScriptGridChannel(IDirScoreManager scoreManager)
        : base(BlingoFrameScriptSprite.SpriteNumOffset+1, scoreManager)
    {
        IsSingleFrame = true;
    }

}

