using AbstUI.Commands;

namespace LingoEngine.Net.RNetClient.Commands;

/// <summary>Disconnects the RNet client from the host.</summary>
public sealed record DisconnectRNetClientCommand() : IAbstCommand;

