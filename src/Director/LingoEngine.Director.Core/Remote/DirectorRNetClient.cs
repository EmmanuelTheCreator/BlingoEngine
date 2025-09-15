using System;
using AbstUI.Commands;
using LingoEngine.Net.RNetProjectClient;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Director.Core.Remote.Commands;

namespace LingoEngine.Director.Core.Remote;

/// <summary>
/// Director-specific proxy that exposes command handlers for the shared RNet client.
/// </summary>
public sealed class DirectorRNetClient :
    IAbstCommandHandler<ConnectRNetClientCommand>,
    IAbstCommandHandler<DisconnectRNetClientCommand>
{
    private readonly ILingoRNetProjectClient _client;
    private readonly IRNetConfiguration _config;

    public DirectorRNetClient(ILingoRNetProjectClient client, IRNetConfiguration config)
    {
        _client = client;
        _config = config;
    }

    public event Action<LingoNetConnectionState>
        ConnectionStatusChanged
    {
        add => _client.ConnectionStatusChanged += value;
        remove => _client.ConnectionStatusChanged -= value;
    }

    public event Action<IRNetCommand> NetCommandReceived
    {
        add => _client.NetCommandReceived += value;
        remove => _client.NetCommandReceived -= value;
    }

    public bool CanExecute(ConnectRNetClientCommand command) => !_client.IsConnected;

    public bool Handle(ConnectRNetClientCommand command)
    {
        var uri = new Uri($"http://localhost:{_config.Port}/director");
        _client.ConnectAsync(uri, new HelloDto("director", "client", "1.0", "")).GetAwaiter().GetResult();
        return true;
    }

    public bool CanExecute(DisconnectRNetClientCommand command) => _client.IsConnected;

    public bool Handle(DisconnectRNetClientCommand command)
    {
        _client.DisconnectAsync().GetAwaiter().GetResult();
        return true;
    }
}

