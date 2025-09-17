using System.Globalization;
using System.Net.WebSockets;
using LingoEngine.IO;
using LingoEngine.Net.RNetClient.Common;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Net.RNetPipeClient;
using LingoEngine.Net.RNetProjectClient;
using Terminal.Gui;
using Timer = System.Timers.Timer;

namespace LingoEngine.Net.RNetTerminal;

public enum RNetTerminalTransport
{
    Http,
    Pipe
}

public sealed record class RNetTerminalConnectionOptions(int Port, RNetTerminalTransport Transport);

public sealed class RNetTerminalConnection : IAsyncDisposable
{
    private readonly TerminalDataStore _store;
    private ILingoRNetClient? _client;
    private CancellationTokenSource? _cts;
    private Timer? _heartbeatTimer;
    private Task? _frameTask;
    private Task? _deltaTask;
    private Task? _memberTask;

    public event Action<LingoNetConnectionState>? ConnectionStateChanged;
    public event Action<int>? PlayFrameReceived;
    public event Action<string>? LogMessage;

    public bool IsConnected => _client?.IsConnected == true;

    public LingoNetConnectionState ConnectionState { get; private set; } = LingoNetConnectionState.Disconnected;

    public RNetTerminalConnection()
    {
        _store = TerminalDataStore.Instance;
        _store.SpriteMoveRequested += OnSpriteMoveRequested;
    }

    public async Task ConnectAsync(RNetTerminalConnectionOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        await DisconnectAsync().ConfigureAwait(false);

        var client = CreateClient(options.Transport);
        _client = client;
        client.ConnectionStatusChanged += HandleClientConnectionStatus;

        var hubUri = BuildUri(options);
        var hello = new HelloDto("test-project", "console", "1.0", "RNetTerminal");

        try
        {
            await client.ConnectAsync(hubUri, hello, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            client.ConnectionStatusChanged -= HandleClientConnectionStatus;
            await client.DisposeAsync().ConfigureAwait(false);
            _client = null;
            UpdateConnectionState(LingoNetConnectionState.Disconnected);
            throw;
        }

        StartBackgroundReaders(client);
        StartHeartbeat();

        await LoadProjectDataAsync(client).ConfigureAwait(false);
    }

    public async Task DisconnectAsync()
    {
        var client = _client;
        if (client is null)
        {
            return;
        }

        _cts?.Cancel();
        await WaitForTaskAsync(_frameTask).ConfigureAwait(false);
        await WaitForTaskAsync(_deltaTask).ConfigureAwait(false);
        await WaitForTaskAsync(_memberTask).ConfigureAwait(false);
        _frameTask = _deltaTask = _memberTask = null;

        _cts?.Dispose();
        _cts = null;

        _heartbeatTimer?.Stop();
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;

        client.ConnectionStatusChanged -= HandleClientConnectionStatus;

        await client.DisposeAsync().ConfigureAwait(false);

        _client = null;
        UpdateConnectionState(LingoNetConnectionState.Disconnected);
    }

    public void QueueGoToFrameCommand(int frame)
    {
        var client = _client;
        if (client is null || !client.IsConnected)
        {
            return;
        }

        FireAndForget(() => client.SendCommandAsync(new GoToFrameCmd(frame)));
    }

    public void QueueSpritePropertyChange(SpriteRef sprite, string propertyName, string value)
    {
        var client = _client;
        if (client is null || !client.IsConnected)
        {
            return;
        }

        FireAndForget(() => client.SendCommandAsync(new SetSpritePropCmd(sprite.SpriteNum, sprite.BeginFrame, propertyName, value)));
    }

    public void QueueMemberPropertyChange(int castLibNum, int memberNum, string propertyName, string value)
    {
        var client = _client;
        if (client is null || !client.IsConnected)
        {
            return;
        }

        FireAndForget(() => client.SendCommandAsync(new SetMemberPropCmd(castLibNum, memberNum, propertyName, value)));
    }

    public async ValueTask DisposeAsync()
    {
        _store.SpriteMoveRequested -= OnSpriteMoveRequested;
        await DisconnectAsync().ConfigureAwait(false);
    }

    private static ILingoRNetClient CreateClient(RNetTerminalTransport transport)
        => transport switch
        {
            RNetTerminalTransport.Http => new LingoRNetProjectClient(),
            RNetTerminalTransport.Pipe => new LingoEngine.Net.RNetPipeClient.RNetPipeClient(),
            _ => throw new ArgumentOutOfRangeException(nameof(transport), transport, null)
        };

    private static Uri BuildUri(RNetTerminalConnectionOptions options)
        => options.Transport switch
        {
            RNetTerminalTransport.Http => new Uri($"http://localhost:{options.Port}/director"),
            RNetTerminalTransport.Pipe => new Uri($"pipe://localhost:{options.Port}/"),
            _ => throw new ArgumentOutOfRangeException(nameof(options.Transport), options.Transport, null)
        };

    private void HandleClientConnectionStatus(LingoNetConnectionState state)
        => UpdateConnectionState(state);

    private void UpdateConnectionState(LingoNetConnectionState state)
    {
        if (ConnectionState == state)
        {
            return;
        }

        ConnectionState = state;

        if (state == LingoNetConnectionState.Connected)
        {
            LogMessage?.Invoke("Connected.");
        }
        else if (state == LingoNetConnectionState.Disconnected)
        {
            LogMessage?.Invoke("Disconnected.");
        }

        ConnectionStateChanged?.Invoke(state);
    }

    private void StartBackgroundReaders(ILingoRNetClient client)
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _frameTask = Task.Run(() => ReceiveFramesAsync(client, token), token);
        _deltaTask = Task.Run(() => ReceiveDeltasAsync(client, token), token);
        _memberTask = Task.Run(() => ReceiveMemberPropertiesAsync(client, token), token);
    }

    private void StartHeartbeat()
    {
        _heartbeatTimer = new Timer(1000)
        {
            AutoReset = true
        };
        _heartbeatTimer.Elapsed += async (_, _) => await DoHeartbeatAsync().ConfigureAwait(false);
        _heartbeatTimer.Start();
    }

    private async Task DoHeartbeatAsync()
    {
        var client = _client;
        if (client is null)
        {
            return;
        }

        try
        {
            await client.SendHeartbeatAsync().ConfigureAwait(false);
        }
        catch (WebSocketException ex) when (ex.Message.Contains("The remote party closed the WebSocket connection", StringComparison.Ordinal))
        {
            LogMessage?.Invoke("Heartbeat lost; disconnecting.");
            await DisconnectAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke("Error sending heartbeat: " + ex.Message);
        }
    }

    private async Task LoadProjectDataAsync(ILingoRNetClient client)
    {
        try
        {
            var projectJson = await client.GetCurrentProjectAsync().ConfigureAwait(false);
            var repo = new JsonStateRepository();
            var project = repo.DeserializeProject(projectJson.json);
            Dispatch(() => _store.LoadFromProject(project));
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke("Load movie failed: " + ex.Message);
        }
    }

    private async Task ReceiveFramesAsync(ILingoRNetClient client, CancellationToken ct)
    {
        try
        {
            await foreach (var frame in client.StreamFramesAsync(ct).ConfigureAwait(false))
            {
                LogMessage?.Invoke($"Frame {frame.FrameId}");
                PlayFrameReceived?.Invoke((int)frame.FrameId);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke("Frame stream error: " + ex.Message);
        }
    }

    private async Task ReceiveDeltasAsync(ILingoRNetClient client, CancellationToken ct)
    {
        try
        {
            await foreach (var delta in client.StreamDeltasAsync(ct).ConfigureAwait(false))
            {
                Dispatch(() => _store.ApplySpriteDelta(delta));
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke("Delta stream error: " + ex.Message);
        }
    }

    private async Task ReceiveMemberPropertiesAsync(ILingoRNetClient client, CancellationToken ct)
    {
        try
        {
            await foreach (var prop in client.StreamMemberPropertiesAsync(ct).ConfigureAwait(false))
            {
                Dispatch(() => _store.ApplyMemberProperty(prop));
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke("Member property stream error: " + ex.Message);
        }
    }

    private void OnSpriteMoveRequested(SpriteRef sprite, int newBegin, int newEnd)
    {
        var client = _client;
        if (client is null || !client.IsConnected)
        {
            return;
        }

        LogMessage?.Invoke($"spriteMove {sprite.SpriteNum}:{sprite.BeginFrame}->{newBegin}-{newEnd}");

        var beginValue = newBegin.ToString(CultureInfo.InvariantCulture);
        var endValue = newEnd.ToString(CultureInfo.InvariantCulture);

        FireAndForget(() => client.SendCommandAsync(new SetSpritePropCmd(sprite.SpriteNum, sprite.BeginFrame, "StartFrame", beginValue)));
        FireAndForget(() => client.SendCommandAsync(new SetSpritePropCmd(sprite.SpriteNum, sprite.BeginFrame, "EndFrame", endValue)));
    }

    private void FireAndForget(Func<Task> work)
        => Task.Run(async () =>
        {
            try
            {
                await work().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke("Command error: " + ex.Message);
            }
        });

    private static async Task WaitForTaskAsync(Task? task)
    {
        if (task is null)
        {
            return;
        }

        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static void Dispatch(Action action)
    {
        if (Application.MainLoop is { } loop)
        {
            loop.Invoke(action);
        }
        else
        {
            action();
        }
    }
}
