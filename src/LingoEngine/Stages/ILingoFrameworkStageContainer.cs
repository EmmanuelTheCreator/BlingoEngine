namespace LingoEngine.Stages
{

    /// <summary>
    /// Lingo Framework Stage Container interface.
    /// </summary>
    public interface ILingoFrameworkStageContainer
    {
        /// <summary>Assigns the framework-specific stage object.</summary>
        void SetStage(ILingoFrameworkStage stage);
    }
}
