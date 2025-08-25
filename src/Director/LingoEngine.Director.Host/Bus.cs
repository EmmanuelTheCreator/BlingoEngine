using System.Threading.Channels;
using LingoEngine.Director.Contracts;

namespace LingoEngine.Director.Host;

/// <summary>
/// Channels used to communicate between the Director runtime and the SignalR hub.
/// </summary>
public interface IBus
{
    /// <summary>Channel carrying complete stage frames.</summary>
    Channel<StageFrameDto> Frames { get; }

    /// <summary>Channel carrying sprite deltas.</summary>
    Channel<SpriteDeltaDto> Deltas { get; }

    /// <summary>Channel carrying commands from the client.</summary>
    Channel<DebugCommandDto> Commands { get; }
}

/// <summary>
/// Default implementation of <see cref="IBus"/>.
/// </summary>
internal sealed class Bus : IBus
{
    public Channel<StageFrameDto> Frames { get; } =
        Channel.CreateBounded<StageFrameDto>(new BoundedChannelOptions(2)
        {
            SingleWriter = true,
            SingleReader = false,
            FullMode = BoundedChannelFullMode.DropOldest
        });

    public Channel<SpriteDeltaDto> Deltas { get; } =
        Channel.CreateBounded<SpriteDeltaDto>(new BoundedChannelOptions(256)
        {
            SingleWriter = true,
            SingleReader = false,
            FullMode = BoundedChannelFullMode.DropOldest
        });

    public Channel<DebugCommandDto> Commands { get; } =
        Channel.CreateUnbounded<DebugCommandDto>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
}
