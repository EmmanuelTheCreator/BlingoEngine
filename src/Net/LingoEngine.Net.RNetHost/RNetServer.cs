using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using LingoEngine.Net.RNetContracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LingoEngine.Net.RNetHost;

/// <summary>
/// Hosts the SignalR server within the RNet process.
/// </summary>
public interface IRNetServer
{
    /// <summary>Provides access to the publisher used by the game loop.</summary>
    IRNetPublisher Publisher { get; }

    /// <summary>Gets the current connection state.</summary>
    LingoNetConnectionState ConnectionState { get; }

    /// <summary>Indicates whether the server is currently running.</summary>
    bool IsEnabled { get; }

    /// <summary>Raised when the connection state changes.</summary>
    event Action<LingoNetConnectionState> ConnectionStatusChanged;

    /// <summary>Raised when a command is received from a client.</summary>
    event Action<INetCommand> NetCommandReceived;

    /// <summary>Starts the server without blocking.</summary>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>Stops the server and disposes all resources.</summary>
    Task StopAsync();
}

/// <summary>
/// Default implementation of <see cref="IRNetServer"/>.
/// </summary>
public sealed class RNetServer : IRNetServer
{
    private WebApplication? _app;
    private CancellationTokenSource? _cts;
    private LingoNetConnectionState _state = LingoNetConnectionState.Disconnected;
    private IBus? _bus;
    private Task? _commandLoop;
    private readonly IRNetConfiguration _config;

    public RNetServer(IRNetConfiguration config)
    {
        _config = config;
    }

    public RNetServer() : this(new DefaultConfig()) { }

    private sealed class DefaultConfig : IRNetConfiguration
    {
        public int Port { get; set; } = 61699;
    }

    /// <inheritdoc />
    public event Action<LingoNetConnectionState>? ConnectionStatusChanged;

    /// <inheritdoc />
    public event Action<INetCommand>? NetCommandReceived;

    /// <inheritdoc />
    public LingoNetConnectionState ConnectionState
    {
        get => _state;
        private set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            ConnectionStatusChanged?.Invoke(_state);
        }
    }

    /// <inheritdoc />
    public bool IsEnabled => ConnectionState == LingoNetConnectionState.Connected;

    /// <inheritdoc />
    public IRNetPublisher Publisher
        => _app?.Services.GetRequiredService<IRNetPublisher>()
           ?? throw new InvalidOperationException("Server not started.");

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_app is not null)
        {
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel();
        builder.WebHost.UseUrls($"http://localhost:{_config.Port}");
        builder.Services.AddSingleton<IBus, Bus>();
        builder.Services.AddSingleton<IRNetPublisher, RNetPublisher>();
        builder.Services.AddSignalR();

        var app = builder.Build();
        app.MapHub<LingoRNetHub>("/director");
        await app.StartAsync(_cts.Token).ConfigureAwait(false);

        _app = app;
        _bus = app.Services.GetRequiredService<IBus>();
        _commandLoop = Task.Run(() => PumpCommandsAsync(_bus.Commands.Reader, _cts.Token), _cts.Token);
        ConnectionState = LingoNetConnectionState.Connected;
    }

    /// <inheritdoc />
    public async Task StopAsync()
    {
        if (_app is null)
        {
            return;
        }

        try
        {
            _cts?.Cancel();
            if (_commandLoop is not null)
            {
                try
                {
                    await _commandLoop.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation
                }
            }
            await _app.StopAsync().ConfigureAwait(false);
            await _app.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _cts?.Dispose();
            _app = null;
            _cts = null;
            _bus = null;
            _commandLoop = null;
            ConnectionState = LingoNetConnectionState.Disconnected;
        }
    }

    private async Task PumpCommandsAsync(ChannelReader<INetCommand> reader, CancellationToken ct)
    {
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var cmd))
            {
                NetCommandReceived?.Invoke(cmd);
            }
        }
    }

}
