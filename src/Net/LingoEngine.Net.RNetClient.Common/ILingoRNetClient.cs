using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LingoEngine.Net.RNetContracts;

namespace LingoEngine.Net.RNetClient.Common;

/// <summary>
/// Shared contract implemented by RNet clients regardless of transport.
/// </summary>
public interface ILingoRNetClient : IAsyncDisposable
{
    /// <summary>Connects to the RNet host and sends the initial hello payload.</summary>
    Task ConnectAsync(Uri hubUrl, HelloDto hello, CancellationToken ct = default);

    /// <summary>Gets the current connection state.</summary>
    LingoNetConnectionState ConnectionState { get; }

    /// <summary>Indicates whether the client is currently connected.</summary>
    bool IsConnected { get; }

    /// <summary>Raised when the connection state changes.</summary>
    event Action<LingoNetConnectionState> ConnectionStatusChanged;

    /// <summary>Raised when a command is received from the host.</summary>
    event Action<IRNetCommand> NetCommandReceived;

    /// <summary>Disconnects from the host.</summary>
    Task DisconnectAsync();

    /// <summary>Streams stage frames from the host.</summary>
    IAsyncEnumerable<StageFrameDto> StreamFramesAsync(CancellationToken ct = default);

    /// <summary>Streams sprite deltas from the host.</summary>
    IAsyncEnumerable<SpriteDeltaDto> StreamDeltasAsync(CancellationToken ct = default);

    /// <summary>Streams keyframes from the host.</summary>
    IAsyncEnumerable<KeyframeDto> StreamKeyframesAsync(CancellationToken ct = default);

    /// <summary>Streams tempo changes from the host.</summary>
    IAsyncEnumerable<TempoDto> StreamTemposAsync(CancellationToken ct = default);

    /// <summary>Streams color palette updates from the host.</summary>
    IAsyncEnumerable<ColorPaletteDto> StreamColorPalettesAsync(CancellationToken ct = default);

    /// <summary>Streams frame scripts from the host.</summary>
    IAsyncEnumerable<FrameScriptDto> StreamFrameScriptsAsync(CancellationToken ct = default);

    /// <summary>Streams transitions from the host.</summary>
    IAsyncEnumerable<TransitionDto> StreamTransitionsAsync(CancellationToken ct = default);

    /// <summary>Streams member property updates from the host.</summary>
    IAsyncEnumerable<RNetMemberPropertyDto> StreamMemberPropertiesAsync(CancellationToken ct = default);

    /// <summary>Streams movie property updates from the host.</summary>
    IAsyncEnumerable<RNetMoviePropertyDto> StreamMoviePropertiesAsync(CancellationToken ct = default);

    /// <summary>Streams stage property updates from the host.</summary>
    IAsyncEnumerable<RNetStagePropertyDto> StreamStagePropertiesAsync(CancellationToken ct = default);

    /// <summary>Streams sprite collection change events from the host.</summary>
    IAsyncEnumerable<RNetSpriteCollectionEventDto> StreamSpriteCollectionEventsAsync(CancellationToken ct = default);

    /// <summary>Streams text style updates from the host.</summary>
    IAsyncEnumerable<TextStyleDto> StreamTextStylesAsync(CancellationToken ct = default);

    /// <summary>Streams film loop state changes from the host.</summary>
    IAsyncEnumerable<FilmLoopDto> StreamFilmLoopsAsync(CancellationToken ct = default);

    /// <summary>Streams sound events from the host.</summary>
    IAsyncEnumerable<SoundEventDto> StreamSoundsAsync(CancellationToken ct = default);

    /// <summary>Requests a snapshot of the current movie state.</summary>
    Task<MovieStateDto> GetMovieSnapshotAsync(CancellationToken ct = default);

    /// <summary>Requests the serialized representation of the current project.</summary>
    Task<LingoProjectJsonDto> GetCurrentProjectAsync(CancellationToken ct = default);

    /// <summary>Sends a debug command to the host.</summary>
    Task SendCommandAsync(RNetCommand cmd, CancellationToken ct = default);

    /// <summary>Sends a heartbeat message to keep the session alive.</summary>
    Task SendHeartbeatAsync(TimeSpan? timeout = null, CancellationToken ct = default);
}
