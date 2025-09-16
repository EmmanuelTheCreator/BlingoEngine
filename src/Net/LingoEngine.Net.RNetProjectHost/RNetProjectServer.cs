using LingoEngine.Core;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Net.RNetHost.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace LingoEngine.Net.RNetProjectHost;

/// <summary>
/// Hosts the SignalR server within the RNet process.
/// </summary>
public interface IRNetProjectServer : ILingoRNetServer { }

/// <summary>
/// Default implementation of <see cref="IRNetProjectServer"/>.
/// </summary>
public sealed class RNetProjectServer : IRNetProjectServer
{
    private WebApplication? _app;
    private CancellationTokenSource? _cts;
    private LingoNetConnectionState _state = LingoNetConnectionState.Disconnected;
    private IRNetProjectBus? _bus;
    private Task? _commandLoop;
    private readonly IRNetConfiguration _config;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public RNetProjectServer(IRNetConfiguration config, ILogger<RNetProjectServer> logger, IServiceProvider serviceProvider)
    {
        _config = config;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public RNetProjectServer(ILogger<RNetProjectServer> logger, IServiceProvider serviceProvider) : this(new DefaultConfig(), logger, serviceProvider) { }

    private sealed class DefaultConfig : IRNetConfiguration
    {
        public int Port { get; set; } = 61699;
        public bool AutoStartRNetHostOnStartup { get; set; }
        public string ClientName { get; set; } = "TheHost";
        public RNetClientType ClientType { get; set; } = RNetClientType.Project;
    }
    public bool DetailedLogging { get; set; } = true;

    public string ServerIp { get; set; } = "localhost";

    public bool UseHttps { get; set; } = false;

    /// <inheritdoc />
    public event Action<LingoNetConnectionState>? ConnectionStatusChanged;

    /// <inheritdoc />
    public event Action<IRNetCommand>? NetCommandReceived;

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
            _logger.LogInformation($"RNet connectionstate: {_state}");
        }
    }

    /// <inheritdoc />
    public bool IsEnabled => ConnectionState == LingoNetConnectionState.Connected;

    /// <inheritdoc />
    public IRNetPublisherEngineBridge Publisher
        => _app?.Services.GetRequiredService<IRNetPublisherEngineBridge>()
           ?? throw new InvalidOperationException("Server not started.");

    /// <summary>Configures the SignalR host.</summary>
    public WebApplication Configure(WebApplicationBuilder? builder = null)
    {
        if (_app is not null)
        {
            return _app;
        }

        var address = BuildAddress();
        builder ??= WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel();
        builder.WebHost.UseUrls(address);
        builder.Services.AddSingleton(p => _serviceProvider.GetRequiredService<IRNetPublisherEngineBridge>());
        builder.Services.AddSingleton(p => _serviceProvider.GetRequiredService<IRNetProjectBus>());
        builder.Services.AddSingleton(p => _serviceProvider.GetRequiredService<ILingoPlayer>());
        builder.Services.AddSignalR(o =>
        {
            o.EnableDetailedErrors = DetailedLogging;
            o.MaximumReceiveMessageSize = 1024 * 1024;
        });

        var app = builder.Build();
        app.MapHub<LingoRNetProjectHub>("/director");
        _app = app;
        return app;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_app is null)
        {
            Configure();
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var address = BuildAddress();
        _logger.LogInformation($"RNet Starting at: {address}");
        await _app!.StartAsync(_cts.Token).ConfigureAwait(false);
        _bus = _app.Services.GetRequiredService<IRNetProjectBus>();
        _commandLoop = Task.Run(() => PumpCommandsAsync(_bus.Commands.Reader, _cts.Token), _cts.Token);
        ConnectionState = LingoNetConnectionState.Connected;
    }

    private string BuildAddress()
        => $"{(UseHttps ? "https" : "http")}://{ServerIp}:{_config.Port}";

    /// <inheritdoc />
    public async Task StopAsync()
    {
        if (_app is null)
        {
            return;
        }

        try
        {
            _logger.LogInformation($"RNet Stop request");
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

    private async Task PumpCommandsAsync(ChannelReader<IRNetCommand> reader, CancellationToken ct)
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
