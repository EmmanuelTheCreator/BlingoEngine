using BlingoEngine.Core;
using BlingoEngine.Net.RNetContracts;

namespace BlingoEngine.Net.RNetHost.Common;

/// <summary>
/// Extends <see cref="IRNetPublisher"/> with hooks for wiring up the live engine runtime.
/// </summary>
public interface IRNetPublisherEngineBridge : IRNetPublisher
{
    /// <summary>Enables publishing by subscribing to the provided player.</summary>
    void Enable(IBlingoPlayer player);

    /// <summary>Disables publishing and unsubscribes from all events.</summary>
    void Disable();
}

