using System;
using System.Collections.Generic;
using LingoEngine.Net.RNetContracts;
using Microsoft.AspNetCore.SignalR.Client;

namespace LingoEngine.Net.RNetClient;

/// <summary>
/// High-level API for interacting with a running RNet host.
/// </summary>
public interface ILingoRNetClient : IAsyncDisposable
{
    /// <summary>Connects to the RNet hub and sends the initial hello payload.</summary>
    Task ConnectAsync(Uri hubUrl, HelloDto hello, CancellationToken ct = default);

    /// <summary>Disconnects from the hub.</summary>
    Task DisconnectAsync();

    /// <summary>Streams stage frames from the host.</summary>
    IAsyncEnumerable<StageFrameDto> StreamFramesAsync(CancellationToken ct = default);

    /// <summary>Streams sprite deltas from the host.</summary>
    IAsyncEnumerable<SpriteDeltaDto> StreamDeltasAsync(CancellationToken ct = default);

    /// <summary>Streams keyframes from the host.</summary>
    IAsyncEnumerable<KeyframeDto> StreamKeyframesAsync(CancellationToken ct = default);

    /// <summary>Streams film loop state changes from the host.</summary>
    IAsyncEnumerable<FilmLoopDto> StreamFilmLoopsAsync(CancellationToken ct = default);

    /// <summary>Streams sound events from the host.</summary>
    IAsyncEnumerable<SoundEventDto> StreamSoundsAsync(CancellationToken ct = default);

    /// <summary>Streams tempo changes from the host.</summary>
    IAsyncEnumerable<TempoDto> StreamTemposAsync(CancellationToken ct = default);

    /// <summary>Streams color palette updates from the host.</summary>
    IAsyncEnumerable<ColorPaletteDto> StreamColorPalettesAsync(CancellationToken ct = default);

    /// <summary>Streams frame scripts from the host.</summary>
    IAsyncEnumerable<FrameScriptDto> StreamFrameScriptsAsync(CancellationToken ct = default);

    /// <summary>Streams transitions from the host.</summary>
    IAsyncEnumerable<TransitionDto> StreamTransitionsAsync(CancellationToken ct = default);

    /// <summary>Streams member property updates from the host.</summary>
    IAsyncEnumerable<MemberPropertyDto> StreamMemberPropertiesAsync(CancellationToken ct = default);

    /// <summary>Streams movie property updates from the host.</summary>
    IAsyncEnumerable<MoviePropertyDto> StreamMoviePropertiesAsync(CancellationToken ct = default);

    /// <summary>Streams stage property updates from the host.</summary>
    IAsyncEnumerable<StagePropertyDto> StreamStagePropertiesAsync(CancellationToken ct = default);

    /// <summary>Streams sprite collection change events from the host.</summary>
    IAsyncEnumerable<SpriteCollectionEventDto> StreamSpriteCollectionEventsAsync(CancellationToken ct = default);

    /// <summary>Streams text style updates from the host.</summary>
    IAsyncEnumerable<TextStyleDto> StreamTextStylesAsync(CancellationToken ct = default);

    /// <summary>Requests a snapshot of the current movie state.</summary>
    Task<MovieStateDto> GetMovieSnapshotAsync(CancellationToken ct = default);
    Task<MovieJsonDto> GetMovieAsync(CancellationToken ct = default);

    /// <summary>Sends a debug command to the host.</summary>
    Task SendCommandAsync(DebugCommandDto cmd, CancellationToken ct = default);

    /// <summary>Sends a heartbeat message to keep the session alive.</summary>
    Task SendHeartbeatAsync(TimeSpan? timeout = null, CancellationToken ct = default);
}

/// <summary>
/// Default implementation of <see cref="ILingoRNetClient"/>.
/// </summary>
public sealed class LingoRNetClient : ILingoRNetClient
{
    private HubConnection? _connection;

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

        await _connection.StartAsync(ct).ConfigureAwait(false);
        await _connection.InvokeAsync("SessionHello", hello, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DisconnectAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
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
    public IAsyncEnumerable<MemberPropertyDto> StreamMemberPropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<MemberPropertyDto>("StreamMemberProperties", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<MoviePropertyDto> StreamMoviePropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<MoviePropertyDto>("StreamMovieProperties", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StagePropertyDto> StreamStagePropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<StagePropertyDto>("StreamStageProperties", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<SpriteCollectionEventDto> StreamSpriteCollectionEventsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<SpriteCollectionEventDto>("StreamSpriteCollectionEvents", ct);
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
    public Task<MovieJsonDto> GetMovieAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.InvokeAsync<MovieJsonDto>("GetMovie", ct);
    }

    /// <inheritdoc />
    public Task SendCommandAsync(DebugCommandDto cmd, CancellationToken ct = default)
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
