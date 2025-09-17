using AbstUI.Commands;

namespace BlingoEngine.Director.Core.Remote.Commands;

/// <summary>Disconnects the Director RNet client from the host.</summary>
public sealed record DisconnectRNetClientCommand() : IAbstCommand;


