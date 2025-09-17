namespace BlingoEngine.Net.RNetContracts;

/// <summary>Configuration settings for RNet server and client.</summary>
public interface IRNetConfiguration
{
    /// <summary>Port used for RNet connections.</summary>
    int Port { get; set; }

    /// <summary>Automatically start the RNet host on engine startup.</summary>
    bool AutoStartRNetHostOnStartup { get; set; }
    /// <summary>Name of the client application.</summary>
    string ClientName { get; set; }

    /// <summary>Indicates whether the instance runs as a remote host or client.</summary>
    RNetRemoteRole RemoteRole { get; set; }

    /// <summary>The transport implementation that should be used by the client.</summary>
    RNetClientType ClientType { get; set; }
}


