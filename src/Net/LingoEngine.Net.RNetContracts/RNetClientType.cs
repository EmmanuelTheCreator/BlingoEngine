namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Specifies which RNet client implementation should be used.
/// </summary>
public enum RNetClientType
{
    /// <summary>Use the SignalR-based project client.</summary>
    Project,

    /// <summary>Use the pipe-based client.</summary>
    Pipe
}

