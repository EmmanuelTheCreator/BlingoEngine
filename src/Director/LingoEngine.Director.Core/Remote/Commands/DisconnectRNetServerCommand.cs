using AbstUI.Commands;

namespace LingoEngine.Director.Core.Remote.Commands;

/// <summary>Stops the RNet server.</summary>
public sealed record DisconnectRNetServerCommand() : IAbstCommand;
