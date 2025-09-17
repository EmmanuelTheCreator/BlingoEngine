using AbstUI.Primitives;
using BlingoEngine.FilmLoops;
using BlingoEngine.Members;
using BlingoEngine.Sprites;

namespace BlingoEngine.Bitmaps
{
    /// <summary>
    /// Framework specific implementation details for a film loop member.
    /// </summary>
    public interface IBlingoFrameworkMemberFilmLoop : IBlingoFrameworkMemberWithTexture
    {
        /// <summary>
        /// Raw data representing the film loop media. The exact format is framework dependent.
        /// </summary>
        byte[]? Media { get; set; }

        /// <summary>
        /// Determines how the film loop should be framed within its sprite rectangle.
        /// Mirrors the Lingo <c>framing</c> property with values <c>#crop</c>, <c>#scale</c>, or <c>#auto</c>.
        /// </summary>
        BlingoFilmLoopFraming Framing { get; set; }

        /// <summary>
        /// Controls whether playback should loop when the last frame is reached.
        /// Corresponds to the Lingo <c>loop</c> property used with film loop sprites.
        /// </summary>
        bool Loop { get; set; }

        /// <summary>
        /// Offset from the film loop's registration point to the top-left corner of its composed texture.
        /// </summary>
        APoint Offset { get; }

        /// <summary>
        /// Composes the active film loop layers into a single texture.
        /// </summary>
        /// <param name="hostSprite">The sprite hosting the film loop.</param>
        /// <param name="layers">Currently active layers for this frame.</param>
        IAbstTexture2D ComposeTexture(IBlingoSprite2DLight hostSprite, IReadOnlyList<BlingoSprite2DVirtual> layers, int frame);

        
    }
}

