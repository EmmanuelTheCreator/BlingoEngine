using System;
using LingoEngine.Core;
using LingoEngine.Net.RNetClient;
using LingoEngine.Net.RNetContracts;

namespace LingoEngine.Net.RNetClientPlayer;

/// <summary>
/// Connects to an RNet host and applies incoming changes to a player instance.
/// </summary>
public sealed class LingoRNetClientPlayer : IAsyncDisposable
{
    private readonly ILingoRNetClient _client;
    private readonly RNetClientPlayerApplier _applier;
    private CancellationTokenSource? _cts;
    private Task? _pumpTask;

    public LingoRNetClientPlayer(ILingoRNetClient client, ILingoPlayer player)
    {
        _client = client;
        _applier = new RNetClientPlayerApplier(player);
    }

    /// <summary>
    /// Connects to the specified RNet host and begins applying changes.
    /// </summary>
    public async Task ConnectAsync(Uri hubUrl, HelloDto hello, CancellationToken ct = default)
    {
        await _client.ConnectAsync(hubUrl, hello, ct).ConfigureAwait(false);
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _pumpTask = PumpAsync(_cts.Token);
    }

    /// <summary>
    /// Stops listening and disconnects from the host.
    /// </summary>
    public async Task DisconnectAsync()
    {
        _cts?.Cancel();
        if (_pumpTask is not null)
        {
            await _pumpTask.ConfigureAwait(false);
        }
        _cts = null;
        await _client.DisconnectAsync().ConfigureAwait(false);
    }

    private async Task PumpAsync(CancellationToken ct)
    {
        await Task.WhenAll(
            PumpFramesAsync(ct),
            PumpDeltasAsync(ct),
            PumpKeyframesAsync(ct),
            PumpFilmLoopsAsync(ct),
            PumpSoundsAsync(ct),
            PumpTemposAsync(ct),
            PumpColorPalettesAsync(ct),
            PumpFrameScriptsAsync(ct),
            PumpTransitionsAsync(ct),
            PumpMemberPropertiesAsync(ct),
            PumpMoviePropertiesAsync(ct),
            PumpStagePropertiesAsync(ct),
            PumpSpriteCollectionEventsAsync(ct),
            PumpTextStylesAsync(ct))
            .ConfigureAwait(false);
    }

    private async Task PumpFramesAsync(CancellationToken ct)
    {
        await foreach (var frame in _client.StreamFramesAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyFrame(frame);
        }
    }

    private async Task PumpDeltasAsync(CancellationToken ct)
    {
        await foreach (var delta in _client.StreamDeltasAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyDelta(delta);
        }
    }

    private async Task PumpKeyframesAsync(CancellationToken ct)
    {
        await foreach (var keyframe in _client.StreamKeyframesAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyKeyframe(keyframe);
        }
    }

    private async Task PumpFilmLoopsAsync(CancellationToken ct)
    {
        await foreach (var loop in _client.StreamFilmLoopsAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyFilmLoop(loop);
        }
    }

    private async Task PumpSoundsAsync(CancellationToken ct)
    {
        await foreach (var sound in _client.StreamSoundsAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplySoundEvent(sound);
        }
    }

    private async Task PumpTemposAsync(CancellationToken ct)
    {
        await foreach (var tempo in _client.StreamTemposAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyTempo(tempo);
        }
    }

    private async Task PumpColorPalettesAsync(CancellationToken ct)
    {
        await foreach (var palette in _client.StreamColorPalettesAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyColorPalette(palette);
        }
    }

    private async Task PumpFrameScriptsAsync(CancellationToken ct)
    {
        await foreach (var script in _client.StreamFrameScriptsAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyFrameScript(script);
        }
    }

    private async Task PumpTransitionsAsync(CancellationToken ct)
    {
        await foreach (var transition in _client.StreamTransitionsAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyTransition(transition);
        }
    }

    private async Task PumpMemberPropertiesAsync(CancellationToken ct)
    {
        await foreach (var prop in _client.StreamMemberPropertiesAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyMemberProperty(prop);
        }
    }

    private async Task PumpMoviePropertiesAsync(CancellationToken ct)
    {
        await foreach (var prop in _client.StreamMoviePropertiesAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyMovieProperty(prop);
        }
    }

    private async Task PumpStagePropertiesAsync(CancellationToken ct)
    {
        await foreach (var prop in _client.StreamStagePropertiesAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyStageProperty(prop);
        }
    }

    private async Task PumpSpriteCollectionEventsAsync(CancellationToken ct)
    {
        await foreach (var evt in _client.StreamSpriteCollectionEventsAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplySpriteCollectionEvent(evt);
        }
    }

    private async Task PumpTextStylesAsync(CancellationToken ct)
    {
        await foreach (var style in _client.StreamTextStylesAsync(ct).ConfigureAwait(false))
        {
            _applier.ApplyTextStyle(style);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }
}
