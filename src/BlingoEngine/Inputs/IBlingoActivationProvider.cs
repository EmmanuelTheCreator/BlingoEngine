namespace BlingoEngine.Inputs
{
    /// <summary>
    /// Provides activation state for input proxies.
    /// </summary>
    public interface IBlingoActivationProvider
    {
        /// <summary>
        /// Indicates whether the proxy should emit input events.
        /// </summary>
        bool IsActivated { get; }
    }
}


