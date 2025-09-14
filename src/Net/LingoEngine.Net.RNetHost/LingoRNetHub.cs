using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using LingoEngine.Core;
using LingoEngine.Net.RNetContracts;
using LingoEngine.IO;
using LingoEngine.Movies;
using Microsoft.AspNetCore.SignalR;

namespace LingoEngine.Net.RNetHost;

/// <summary>
/// SignalR hub exposing the debugging stream.
/// </summary>
public sealed class LingoRNetHub : Hub
{
    private readonly IBus _bus;
    private readonly ILingoPlayer _player;
    private static readonly ConcurrentDictionary<string, DateTime> _heartbeats = new();

    /// <summary>Initializes a new instance of the <see cref="LingoRNetHub"/> class.</summary>
    public LingoRNetHub(IBus bus, ILingoPlayer player)
    {
        _bus = bus;
        _player = player;
    }

    
    #region Client->Server
    /// <summary>
    /// Initializes a new debug session.
    /// </summary>
    public Task SessionHello(HelloDto dto)
    {
        _heartbeats[Context.ConnectionId] = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Receives a debug command from a client.
    /// </summary>
    public Task SendCommand(DebugCommandDto cmd)
    {
        _bus.Commands.Writer.TryWrite(cmd);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Receives a heartbeat message to keep the session alive.
    /// </summary>
    public Task Heartbeat()
    {
        _heartbeats[Context.ConnectionId] = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns a snapshot of the current movie state.
    /// </summary>
    public Task<MovieStateDto> GetMovieSnapshot()
    {
        if (_player.ActiveMovie is not null)
        {
            return Task.FromResult(new MovieStateDto(
                Frame: _player.ActiveMovie.CurrentFrame,
                Tempo: _player.ActiveMovie.Tempo,
                IsPlaying: _player.ActiveMovie.IsPlaying));
        }
        return Task.FromResult(new MovieStateDto(0, 0, false));
    }
    public Task<MovieJsonDto> GetMovie()
    {
        if (_player.ActiveMovie == null)
            return Task.FromResult(new MovieJsonDto(""));
        var dtoTuplet = new JsonStateRepository().Serialize((LingoMovie)_player.ActiveMovie,new JsonStateRepository.MovieStoreOptions { });
       
        return Task.FromResult(new MovieJsonDto(dtoTuplet.JsonString));
    }

    #endregion


    #region Server->Client (streams)

    /// <summary>
    /// Streams stage frame snapshots to the connected client.
    /// </summary>
    public async IAsyncEnumerable<StageFrameDto> StreamFrames([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Frames.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var frame))
            {
                yield return frame;
            }
        }
    }

    /// <summary>
    /// Streams sprite deltas to the connected client.
    /// </summary>
    public async IAsyncEnumerable<SpriteDeltaDto> StreamDeltas([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Deltas.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var delta))
            {
                yield return delta;
            }
        }
    }

    /// <summary>
    /// Streams keyframe information to the connected client.
    /// </summary>
    public async IAsyncEnumerable<KeyframeDto> StreamKeyframes([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Keyframes.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var keyframe))
            {
                yield return keyframe;
            }
        }
    }

    /// <summary>
    /// Streams film loop state changes to the connected client.
    /// </summary>
    public async IAsyncEnumerable<FilmLoopDto> StreamFilmLoops([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.FilmLoops.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var loop))
            {
                yield return loop;
            }
        }
    }

    /// <summary>
    /// Streams sound events to the connected client.
    /// </summary>
    public async IAsyncEnumerable<SoundEventDto> StreamSounds([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Sounds.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var snd))
            {
                yield return snd;
            }
        }
    }

    /// <summary>
    /// Streams tempo changes to the connected client.
    /// </summary>
    public async IAsyncEnumerable<TempoDto> StreamTempos([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Tempos.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var tempo))
            {
                yield return tempo;
            }
        }
    }

    /// <summary>
    /// Streams color palette updates to the connected client.
    /// </summary>
    public async IAsyncEnumerable<ColorPaletteDto> StreamColorPalettes([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.ColorPalettes.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var palette))
            {
                yield return palette;
            }
        }
    }

    /// <summary>
    /// Streams frame scripts to the connected client.
    /// </summary>
    public async IAsyncEnumerable<FrameScriptDto> StreamFrameScripts([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.FrameScripts.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var script))
            {
                yield return script;
            }
        }
    }

    /// <summary>
    /// Streams transitions to the connected client.
    /// </summary>
    public async IAsyncEnumerable<TransitionDto> StreamTransitions([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Transitions.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var transition))
            {
                yield return transition;
            }
        }
    }

    /// <summary>
    /// Streams member property updates to the connected client.
    /// </summary>
    public async IAsyncEnumerable<MemberPropertyDto> StreamMemberProperties([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.MemberProperties.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var prop))
            {
                yield return prop;
            }
        }
    }

    /// <summary>
    /// Streams text style updates to the connected client.
    /// </summary>
    public async IAsyncEnumerable<TextStyleDto> StreamTextStyles([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.TextStyles.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var style))
            {
                yield return style;
            }
        }
    } 
    #endregion


}
