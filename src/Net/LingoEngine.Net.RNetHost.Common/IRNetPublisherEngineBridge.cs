using LingoEngine.Core;
using LingoEngine.Net.RNetContracts;

namespace LingoEngine.Net.RNetHost.Common;

/// <summary>
/// Extends <see cref="IRNetPublisher"/> with hooks for wiring up the live engine runtime.
/// </summary>
public interface IRNetPublisherEngineBridge : IRNetPublisher
{
    /// <summary>Enables publishing by subscribing to the provided player.</summary>
    void Enable(ILingoPlayer player);

    /// <summary>Disables publishing and unsubscribes from all events.</summary>
    void Disable();
}
