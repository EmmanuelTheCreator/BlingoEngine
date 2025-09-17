namespace BlingoEngine.Net.RNetContracts;

/// <summary>
/// Describes whether the current instance should act as a remote host or client.
/// </summary>
public enum RNetRemoteRole
{
    /// <summary>Expose the current project as a remote host.</summary>
    Host,

    /// <summary>Connect to an external host as a client.</summary>
    Client
}

