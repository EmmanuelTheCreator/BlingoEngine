namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Represents the connection state of an RNet server or client.
/// </summary>
public enum LingoNetConnectionState
{
    /// <summary>No active connection.</summary>
    Disconnected,
    /// <summary>Connection is being established.</summary>
    Connecting,
    /// <summary>Connection is established.</summary>
    Connected
}
