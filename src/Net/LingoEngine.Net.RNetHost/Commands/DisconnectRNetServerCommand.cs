using AbstUI.Commands;

namespace LingoEngine.Net.RNetHost.Commands;

/// <summary>Stops the RNet server.</summary>
public sealed record DisconnectRNetServerCommand() : IAbstCommand;

