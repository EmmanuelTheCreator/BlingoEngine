using BlingoEngine.Members;

namespace BlingoEngine.Sprites
{
    /// <summary>
    /// Sprite that exposes its underlying member.
    /// </summary>
    public interface IBlingoSpriteWithMember : IBlingoSpriteBase, IMemberRefUser
    {
        IBlingoMember? GetMember();
    }
}

