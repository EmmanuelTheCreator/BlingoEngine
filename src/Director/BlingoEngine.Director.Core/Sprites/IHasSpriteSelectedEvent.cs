using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Sprites
{
    public interface IHasSpriteSelectedEvent
    {
        void SpriteSelected(IBlingoSpriteBase sprite);
    }
}

