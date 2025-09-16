using System;
using System.Collections.Generic;
using LingoEngine.Net.RNetClient.Common;
using LingoEngine.Net.RNetContracts;
using Microsoft.AspNetCore.SignalR.Client;

namespace LingoEngine.Net.RNetProjectClient;

/// <summary>
/// High-level API for interacting with a running RNet project host.
/// </summary>
public interface ILingoRNetProjectClient : ILingoRNetClient { }

/// <summary>
/// Default implementation of <see cref="ILingoRNetProjectClient"/>.
/// </summary>
public sealed class LingoRNetProjectClient : ILingoRNetProjectClient
{
    private HubConnection? _connection;
    private LingoNetConnectionState _state = LingoNetConnectionState.Disconnected;
    private readonly IRNetConfiguration _config;

    public LingoRNetProjectClient(IRNetConfiguration config)
    {
        _config = config;
    }

    public LingoRNetProjectClient() : this(new DefaultConfig()) { }

    private sealed class DefaultConfig : IRNetConfiguration
    {
        public int Port { get; set; } = 61699;
        public bool AutoStartRNetHostOnStartup { get; set; }
        public string ClientName { get; set; } = "Some client";
    }

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
        }
    }

    /// <inheritdoc />
    public bool IsConnected => ConnectionState == LingoNetConnectionState.Connected;

    /// <inheritdoc />
    public async Task ConnectAsync(Uri hubUrl, HelloDto hello, CancellationToken ct = default)
    {
        if (_connection is not null)
        {
            return;
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.Reconnecting += _ =>
        {
            ConnectionState = LingoNetConnectionState.Connecting;
            return Task.CompletedTask;
        };
        _connection.Reconnected += _ =>
        {
            ConnectionState = LingoNetConnectionState.Connected;
            return Task.CompletedTask;
        };
        _connection.Closed += _ =>
        {
            ConnectionState = LingoNetConnectionState.Disconnected;
            return Task.CompletedTask;
        };

        _connection.On<RNetCommand>("Command", cmd => NetCommandReceived?.Invoke(cmd));

        ConnectionState = LingoNetConnectionState.Connecting;
        await _connection.StartAsync(ct).ConfigureAwait(false);
        await _connection.InvokeAsync("SessionHello", hello, ct).ConfigureAwait(false);
        ConnectionState = LingoNetConnectionState.Connected;
    }

    /// <inheritdoc />
    public async Task DisconnectAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
            ConnectionState = LingoNetConnectionState.Disconnected;
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StageFrameDto> StreamFramesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<StageFrameDto>("StreamFrames", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<SpriteDeltaDto> StreamDeltasAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<SpriteDeltaDto>("StreamDeltas", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<KeyframeDto> StreamKeyframesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<KeyframeDto>("StreamKeyframes", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<FilmLoopDto> StreamFilmLoopsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<FilmLoopDto>("StreamFilmLoops", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<SoundEventDto> StreamSoundsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<SoundEventDto>("StreamSounds", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TempoDto> StreamTemposAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<TempoDto>("StreamTempos", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ColorPaletteDto> StreamColorPalettesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<ColorPaletteDto>("StreamColorPalettes", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<FrameScriptDto> StreamFrameScriptsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<FrameScriptDto>("StreamFrameScripts", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TransitionDto> StreamTransitionsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<TransitionDto>("StreamTransitions", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetMemberPropertyDto> StreamMemberPropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<RNetMemberPropertyDto>("StreamMemberProperties", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetMoviePropertyDto> StreamMoviePropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<RNetMoviePropertyDto>("StreamMovieProperties", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetStagePropertyDto> StreamStagePropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<RNetStagePropertyDto>("StreamStageProperties", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetSpriteCollectionEventDto> StreamSpriteCollectionEventsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<RNetSpriteCollectionEventDto>("StreamSpriteCollectionEvents", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TextStyleDto> StreamTextStylesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<TextStyleDto>("StreamTextStyles", ct);
    }

    /// <inheritdoc />
    public Task<MovieStateDto> GetMovieSnapshotAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.InvokeAsync<MovieStateDto>("GetMovieSnapshot", ct);
    }
    /// <inheritdoc />
    public Task<LingoProjectJsonDto> GetCurrentProjectAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.InvokeAsync<LingoProjectJsonDto>("GetCurrentProject", ct);
    }

    /// <inheritdoc />
    public Task SendCommandAsync(RNetCommand cmd, CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.InvokeAsync("SendCommand", cmd, ct).WaitAsync(TimeSpan.FromSeconds(5), ct);
    }

    /// <inheritdoc />
    public Task SendHeartbeatAsync(TimeSpan? timeout = null, CancellationToken ct = default)
    {
        EnsureConnected();
        var invoke = _connection!.InvokeAsync("Heartbeat", ct);
        return timeout.HasValue ? invoke.WaitAsync(timeout.Value, ct) : invoke;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }

    private void EnsureConnected()
    {
        if (_connection is null)
        {
            throw new InvalidOperationException("Not connected.");
        }
    }

}
