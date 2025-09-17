using System.Collections.Concurrent;

namespace BlingoEngine.Net.RNetServer;

public sealed class ProjectRegistry
{
    public sealed class ProjectInfo
    {
        public string? HostConnectionId { get; set; }
        public ConcurrentDictionary<string, byte> Clients { get; } = new();
    }

    public ConcurrentDictionary<string, ProjectInfo> Projects { get; } = new();
}

