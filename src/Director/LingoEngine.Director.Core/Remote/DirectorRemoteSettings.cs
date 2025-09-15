using LingoEngine.Net.RNetContracts;

namespace LingoEngine.Director.Core.Remote;

/// <summary>Settings for RNet remote connections.</summary>
public class DirectorRemoteSettings : IRNetConfiguration
{
    /// <summary>Port used by the RNet server and client.</summary>
    public int Port { get; set; } = 61699;
}

