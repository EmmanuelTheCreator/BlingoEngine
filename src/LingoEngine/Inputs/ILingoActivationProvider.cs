namespace LingoEngine.Inputs
{
    /// <summary>
    /// Provides activation state for input proxies.
    /// </summary>
    public interface ILingoActivationProvider
    {
        /// <summary>
        /// Indicates whether the proxy should emit input events.
        /// </summary>
        bool IsActivated { get; }
    }
}

