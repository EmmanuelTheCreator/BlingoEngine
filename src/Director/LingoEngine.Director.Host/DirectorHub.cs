using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using LingoEngine.Director.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace LingoEngine.Director.Host;

/// <summary>
/// SignalR hub exposing the debugging stream.
/// </summary>
public sealed class DirectorHub : Hub
{
    private readonly IBus _bus;
    private static readonly ConcurrentDictionary<string, DateTime> _heartbeats = new();

    /// <summary>Initializes a new instance of the <see cref="DirectorHub"/> class.</summary>
    public DirectorHub(IBus bus) => _bus = bus;

    // Client->Server
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
        => Task.FromResult(new MovieStateDto(0, 0, false));

    // Server->Client (streams)
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
}
