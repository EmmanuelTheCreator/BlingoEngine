namespace LingoEngine.Net.RNetContracts;

/// <summary>Configuration settings for RNet server and client.</summary>
public interface IRNetConfiguration
{
    /// <summary>Port used for RNet connections.</summary>
    int Port { get; set; }
}

