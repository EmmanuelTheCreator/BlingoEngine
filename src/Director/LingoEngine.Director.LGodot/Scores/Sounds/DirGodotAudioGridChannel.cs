using LingoEngine.Movies;
using LingoEngine.Sounds;
using LingoEngine.Director.LGodot.Scores.Sounds;
using LingoEngine.Director.Core.Sprites;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotAudioGridChannel : DirGodotTopGridChannel<ILingoSpriteAudioManager, DirGodotSoundSprite, LingoSpriteSound>
{

    protected override DirGodotSoundSprite CreateUISprite(LingoSpriteSound sprite, IDirSpritesManager spritesManager) => new DirGodotSoundSprite(sprite, spritesManager);

    protected override ILingoSpriteAudioManager GetManager(LingoMovie movie) => movie.Audio;

    public DirGodotAudioGridChannel(int spriteNum, IDirSpritesManager spritesManager)
        : base(spriteNum, spritesManager)
    {
    }

}
