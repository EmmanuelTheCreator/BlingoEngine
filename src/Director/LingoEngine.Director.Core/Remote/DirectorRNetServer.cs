using AbstUI.Commands;
using LingoEngine.Core;
using LingoEngine.Net.RNetHost;
using LingoEngine.Director.Core.Remote.Commands;

namespace LingoEngine.Director.Core.Remote;

/// <summary>
/// Director-specific proxy that exposes command handlers for the shared RNet server.
/// </summary>
public sealed class DirectorRNetServer :
    IAbstCommandHandler<ConnectRNetServerCommand>,
    IAbstCommandHandler<DisconnectRNetServerCommand>
{
    private readonly IRNetServer _server;
    private readonly ILingoPlayer _player;

    public DirectorRNetServer(IRNetServer server, ILingoPlayer player)
    {
        _server = server;
        _player = player;
    }

    public bool CanExecute(ConnectRNetServerCommand command) => !_server.IsEnabled;

    public bool Handle(ConnectRNetServerCommand command)
    {
        _server.StartAsync().GetAwaiter().GetResult();
        _server.Publisher.Enable(_player);
        return true;
    }

    public bool CanExecute(DisconnectRNetServerCommand command) => _server.IsEnabled;

    public bool Handle(DisconnectRNetServerCommand command)
    {
        _server.Publisher.Disable();
        _server.StopAsync().GetAwaiter().GetResult();
        return true;
    }
}
