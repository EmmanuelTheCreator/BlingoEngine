using LingoEngine.Members;

namespace LingoEngine.Sprites
{
    /// <summary>
    /// Sprite that exposes its underlying member.
    /// </summary>
    public interface ILingoSpriteWithMember : ILingoSpriteBase, IMemberRefUser
    {
        ILingoMember? GetMember();
    }
}
