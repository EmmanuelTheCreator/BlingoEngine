namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Common interface for all framework input controls.
    /// </summary>
    public interface IAbstFrameworkNodeInput : IAbstFrameworkLayoutNode
    {
        /// <summary>Whether the control is enabled.</summary>
        bool Enabled { get; set; }
        /// <summary>Raised when the value of the input changes.</summary>
        event Action? ValueChanged;
    }
}
