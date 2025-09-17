using BlingoEngine.Movies;
using BlingoEngine.Sounds;
using BlingoEngine.Director.Core.Sprites;

namespace BlingoEngine.Director.Core.Scores.Sounds;

internal partial class DirScoreAudioGridChannel : DirScoreChannel<IBlingoSpriteAudioManager, DirScoreSoundSprite, BlingoSpriteSound>
{

    protected override DirScoreSoundSprite CreateUISprite(BlingoSpriteSound sprite, IDirSpritesManager spritesManager) => new DirScoreSoundSprite(sprite, spritesManager);

    protected override IBlingoSpriteAudioManager GetManager(BlingoMovie movie) => movie.Audio;

    public DirScoreAudioGridChannel(int spriteNum, IDirScoreManager scoreManager)
        : base(spriteNum + BlingoSpriteSound.SpriteNumOffset, scoreManager)
    {
    }

}

