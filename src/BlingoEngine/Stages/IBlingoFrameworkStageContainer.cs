namespace BlingoEngine.Stages
{

    /// <summary>
    /// Lingo Framework Stage Container interface.
    /// </summary>
    public interface IBlingoFrameworkStageContainer
    {
        /// <summary>Assigns the framework-specific stage object.</summary>
        void SetStage(IBlingoFrameworkStage stage);
    }
}

