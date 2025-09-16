using System;
using System.Threading;
using System.Threading.Tasks;
using LingoEngine.Net.RNetContracts;

namespace LingoEngine.Net.RNetHost.Common;

/// <summary>
/// Shared contract implemented by concrete RNet host transports.
/// </summary>
public interface ILingoRNetServer
{
    /// <summary>Provides access to the publisher used by the game loop.</summary>
    IRNetPublisherEngineBridge Publisher { get; }

    /// <summary>Gets the current connection state.</summary>
    LingoNetConnectionState ConnectionState { get; }

    /// <summary>Indicates whether the server is currently running.</summary>
    bool IsEnabled { get; }

    /// <summary>Raised when the connection state changes.</summary>
    event Action<LingoNetConnectionState> ConnectionStatusChanged;

    /// <summary>Raised when a command is received from a client.</summary>
    event Action<IRNetCommand> NetCommandReceived;

    /// <summary>Starts the server without blocking.</summary>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>Stops the server and disposes all resources.</summary>
    Task StopAsync();
}
