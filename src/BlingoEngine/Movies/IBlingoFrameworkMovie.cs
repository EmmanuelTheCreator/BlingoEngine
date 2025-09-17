using AbstUI.Components;
using AbstUI.Primitives;

namespace BlingoEngine.Movies
{
    /// <summary>
    /// Interface implemented by platform specific movie objects. It connects a
    /// <see cref="BlingoMovie"/> to the underlying rendering framework.
    /// </summary>
    public interface IBlingoFrameworkMovie : IAbstFrameworkNode
    {
        /// <summary>Updates the associated stage after the movie changes.</summary>
        void UpdateStage();
        /// <summary>Removes the movie from the stage.</summary>
        void RemoveMe();
        /// <summary>Retrieves the current mouse position in global coordinates.</summary>
        APoint GetGlobalMousePosition();
    }
}

