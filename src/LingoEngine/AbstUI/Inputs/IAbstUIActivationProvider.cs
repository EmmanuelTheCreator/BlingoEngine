namespace LingoEngine.AbstUI.Inputs
{
    /// <summary>
    /// Provides activation state for input proxies.
    /// </summary>
    public interface IAbstUIActivationProvider
    {
        /// <summary>
        /// Indicates whether the proxy should emit input events.
        /// </summary>
        bool IsActivated { get; }
    }
}

