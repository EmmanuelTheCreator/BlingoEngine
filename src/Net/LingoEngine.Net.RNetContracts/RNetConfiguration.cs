namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Default configuration for RNet connections.
/// </summary>
public class RNetConfiguration : IRNetConfiguration
{
    /// <inheritdoc />
    public int Port { get; set; } = 61699;

    /// <inheritdoc />
    public bool AutoStartRNetHostOnStartup { get; set; }
}

