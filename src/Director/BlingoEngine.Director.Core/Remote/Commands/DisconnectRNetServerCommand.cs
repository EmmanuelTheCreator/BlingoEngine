using AbstUI.Commands;

namespace BlingoEngine.Director.Core.Remote.Commands;

/// <summary>Stops the RNet server.</summary>
public sealed record DisconnectRNetServerCommand() : IAbstCommand;

