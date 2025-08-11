using LingoEngine.Movies;
using LingoEngine.Primitives;

namespace LingoEngine.Stages
{
    /// <summary>
    /// Represents the top-level window or stage. Implementations update the
    /// display when the active <see cref="LingoMovie"/> changes.
    /// </summary>
    public interface ILingoFrameworkStage
    {
        LingoStage LingoStage { get; }
        /// <summary>Sets the currently active movie.</summary>
        void SetActiveMovie(LingoMovie? lingoMovie);
        void ApplyPropertyChanges();

        float Scale { get; set; }
        
    }
}
