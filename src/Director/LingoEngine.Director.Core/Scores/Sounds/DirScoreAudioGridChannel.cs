using LingoEngine.Movies;
using LingoEngine.Sounds;
using LingoEngine.Director.Core.Sprites;

namespace LingoEngine.Director.Core.Scores.Sounds;

internal partial class DirScoreAudioGridChannel : DirScoreChannel<ILingoSpriteAudioManager, DirScoreSoundSprite, LingoSpriteSound>
{

    protected override DirScoreSoundSprite CreateUISprite(LingoSpriteSound sprite, IDirSpritesManager spritesManager) => new DirScoreSoundSprite(sprite, spritesManager);

    protected override ILingoSpriteAudioManager GetManager(LingoMovie movie) => movie.Audio;

    public DirScoreAudioGridChannel(int spriteNum, IDirScoreManager scoreManager)
        : base(spriteNum + LingoSpriteSound.SpriteNumOffset, scoreManager)
    {
    }

}
