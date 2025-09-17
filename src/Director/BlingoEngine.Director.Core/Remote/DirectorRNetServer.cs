using AbstUI.Commands;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Remote.Commands;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;

namespace BlingoEngine.Director.Core.Remote;

/// <summary>
/// Director-specific proxy that exposes command handlers for the shared RNet server.
/// </summary>
public sealed class DirectorRNetServer :
    IAbstCommandHandler<ConnectRNetServerCommand>,
    IAbstCommandHandler<DisconnectRNetServerCommand>
{
    private readonly IRNetProjectServer _projectServer;
    private readonly IRNetPipeServer? _pipeServer;
    private readonly IRNetConfiguration _config;
    private readonly IBlingoPlayer _player;
    private IBlingoRNetServer? _activeServer;
    private readonly bool _projectServerIsDummy;

    public DirectorRNetServer(
        IRNetProjectServer projectServer,
        IBlingoPlayer player,
        IRNetConfiguration config,
        IRNetPipeServer? pipeServer = null)
    {
        _projectServer = projectServer;
        _pipeServer = pipeServer;
        _config = config;
        _player = player;
        _projectServerIsDummy = projectServer is DummyRNetProjectServer;
    }

    public event Action<BlingoNetConnectionState>? ConnectionStatusChanged;

    public event Action<IRNetCommand>? NetCommandReceived;

    public bool IsEnabled => _activeServer?.IsEnabled ?? false;

    public bool CanExecute(ConnectRNetServerCommand command)
    {
        if (_config.RemoteRole != RNetRemoteRole.Host)
        {
            return false;
        }

        var server = GetServerForConfiguration();
        return server is { IsEnabled: false };
    }

    public bool Handle(ConnectRNetServerCommand command)
    {
        if (_config.RemoteRole != RNetRemoteRole.Host)
        {
            return false;
        }

        var server = GetServerForConfiguration();
        if (server is null)
        {
            return false;
        }

        SwapActiveServer(server);
        server.StartAsync().GetAwaiter().GetResult();
        server.Publisher.Enable(_player);
        return true;
    }

    public bool CanExecute(DisconnectRNetServerCommand command)
    {
        var server = _activeServer ?? GetServerForConfiguration();
        return server is { IsEnabled: true };
    }

    public bool Handle(DisconnectRNetServerCommand command)
    {
        var server = _activeServer ?? GetServerForConfiguration();
        if (server is null || !server.IsEnabled)
        {
            return false;
        }

        server.Publisher.Disable();
        server.StopAsync().GetAwaiter().GetResult();
        return true;
    }

    private IBlingoRNetServer? GetServerForConfiguration()
        => _config.ClientType switch
        {
            RNetClientType.Pipe => _pipeServer,
            _ => _projectServerIsDummy ? null : _projectServer,
        };

    private void SwapActiveServer(IBlingoRNetServer server)
    {
        if (ReferenceEquals(_activeServer, server))
        {
            return;
        }

        if (_activeServer is not null)
        {
            _activeServer.ConnectionStatusChanged -= OnServerConnectionStatusChanged;
            _activeServer.NetCommandReceived -= OnServerCommandReceived;
        }

        _activeServer = server;
        _activeServer.ConnectionStatusChanged += OnServerConnectionStatusChanged;
        _activeServer.NetCommandReceived += OnServerCommandReceived;
    }

    private void OnServerConnectionStatusChanged(BlingoNetConnectionState state)
        => ConnectionStatusChanged?.Invoke(state);

    private void OnServerCommandReceived(IRNetCommand command)
        => NetCommandReceived?.Invoke(command);
}

