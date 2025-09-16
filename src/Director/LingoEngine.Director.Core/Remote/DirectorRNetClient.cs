using System;
using AbstUI.Commands;
using LingoEngine.Director.Core.Remote.Commands;
using LingoEngine.Net.RNetClient.Common;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Net.RNetPipeClient;
using LingoEngine.Net.RNetProjectClient;

namespace LingoEngine.Director.Core.Remote;

/// <summary>
/// Director-specific proxy that exposes command handlers for the shared RNet client.
/// </summary>
public sealed class DirectorRNetClient :
    IAbstCommandHandler<ConnectRNetClientCommand>,
    IAbstCommandHandler<DisconnectRNetClientCommand>
{
    private readonly ILingoRNetProjectClient _projectClient;
    private readonly ILingoRNetPipeClient _pipeClient;
    private readonly IRNetConfiguration _config;
    private ILingoRNetClient? _activeClient;

    public DirectorRNetClient(
        ILingoRNetProjectClient projectClient,
        ILingoRNetPipeClient pipeClient,
        IRNetConfiguration config)
    {
        _projectClient = projectClient;
        _pipeClient = pipeClient;
        _config = config;
    }

    public event Action<LingoNetConnectionState>? ConnectionStatusChanged;

    public event Action<IRNetCommand>? NetCommandReceived;

    public bool IsConnected => (_activeClient ?? GetClientForConfiguration()).IsConnected;

    public bool CanExecute(ConnectRNetClientCommand command)
    {
        if (_activeClient is { IsConnected: true })
        {
            return false;
        }

        return !GetClientForConfiguration().IsConnected;
    }

    public bool Handle(ConnectRNetClientCommand command)
    {
        var client = GetClientForConfiguration();
        SwapActiveClient(client);

        var uri = BuildConnectionUri(_config.ClientType, _config.Port);
        client.ConnectAsync(uri, CreateHello()).GetAwaiter().GetResult();
        return true;
    }

    public bool CanExecute(DisconnectRNetClientCommand command)
    {
        if (_activeClient is not null)
        {
            return _activeClient.IsConnected;
        }

        return GetClientForConfiguration().IsConnected;
    }

    public bool Handle(DisconnectRNetClientCommand command)
    {
        var client = _activeClient ?? GetClientForConfiguration();
        client.DisconnectAsync().GetAwaiter().GetResult();
        return true;
    }

    private HelloDto CreateHello()
    {
        var clientName = string.IsNullOrWhiteSpace(_config.ClientName)
            ? "client"
            : _config.ClientName;
        return new HelloDto("director", clientName, "1.0", string.Empty);
    }

    private ILingoRNetClient GetClientForConfiguration()
        => _config.ClientType switch
        {
            RNetClientType.Pipe => _pipeClient,
            _ => _projectClient,
        };

    private void SwapActiveClient(ILingoRNetClient client)
    {
        if (ReferenceEquals(_activeClient, client))
        {
            return;
        }

        if (_activeClient is not null)
        {
            _activeClient.ConnectionStatusChanged -= OnClientConnectionStatusChanged;
            _activeClient.NetCommandReceived -= OnClientCommandReceived;
        }

        _activeClient = client;
        _activeClient.ConnectionStatusChanged += OnClientConnectionStatusChanged;
        _activeClient.NetCommandReceived += OnClientCommandReceived;
    }

    private void OnClientConnectionStatusChanged(LingoNetConnectionState state)
        => ConnectionStatusChanged?.Invoke(state);

    private void OnClientCommandReceived(IRNetCommand command)
        => NetCommandReceived?.Invoke(command);

    private static Uri BuildConnectionUri(RNetClientType clientType, int port)
        => clientType switch
        {
            RNetClientType.Pipe => new Uri($"pipe://localhost:{port}"),
            _ => new Uri($"http://localhost:{port}/director"),
        };
}

