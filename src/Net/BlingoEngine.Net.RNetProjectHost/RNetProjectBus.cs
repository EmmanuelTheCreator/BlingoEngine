using System.Threading.Channels;
using BlingoEngine.Net.RNetContracts;

namespace BlingoEngine.Net.RNetProjectHost;

/// <summary>
/// Channels used to communicate between the BlingoEngine runtime and the SignalR hub.
/// </summary>
public interface IRNetProjectBus
{
    /// <summary>Channel carrying complete stage frames.</summary>
    Channel<StageFrameDto> Frames { get; }

    /// <summary>Channel carrying sprite deltas.</summary>
    Channel<SpriteDeltaDto> Deltas { get; }

    /// <summary>Channel carrying keyframe information.</summary>
    Channel<KeyframeDto> Keyframes { get; }

    /// <summary>Channel carrying film loop state changes.</summary>
    Channel<FilmLoopDto> FilmLoops { get; }

    /// <summary>Channel carrying sound events.</summary>
    Channel<SoundEventDto> Sounds { get; }

    /// <summary>Channel carrying tempo changes.</summary>
    Channel<TempoDto> Tempos { get; }

    /// <summary>Channel carrying color palette updates.</summary>
    Channel<ColorPaletteDto> ColorPalettes { get; }

    /// <summary>Channel carrying frame scripts.</summary>
    Channel<FrameScriptDto> FrameScripts { get; }

    /// <summary>Channel carrying transitions.</summary>
    Channel<TransitionDto> Transitions { get; }

    /// <summary>Channel carrying member property updates.</summary>
    Channel<RNetMemberPropertyDto> MemberProperties { get; }

    /// <summary>Channel carrying text style updates.</summary>
    Channel<TextStyleDto> TextStyles { get; }

    /// <summary>Channel carrying movie property updates.</summary>
    Channel<RNetMoviePropertyDto> MovieProperties { get; }

    /// <summary>Channel carrying stage property updates.</summary>
    Channel<RNetStagePropertyDto> StageProperties { get; }

    /// <summary>Channel carrying sprite collection change events.</summary>
    Channel<RNetSpriteCollectionEventDto> SpriteCollectionEvents { get; }

    /// <summary>Channel carrying commands from the client.</summary>
    Channel<IRNetCommand> Commands { get; }
}

/// <summary>
/// Default implementation of <see cref="IRNetProjectBus"/>.
/// </summary>
internal sealed class RNetProjectBus : IRNetProjectBus
{
    private static BoundedChannelOptions Opts(int capacity) => new(capacity)
    {
        SingleWriter = true,
        SingleReader = false,
        FullMode = BoundedChannelFullMode.DropOldest
    };

    public Channel<StageFrameDto> Frames { get; } =
        Channel.CreateBounded<StageFrameDto>(Opts(2));

    public Channel<SpriteDeltaDto> Deltas { get; } =
        Channel.CreateBounded<SpriteDeltaDto>(Opts(256));

    public Channel<KeyframeDto> Keyframes { get; } =
        Channel.CreateBounded<KeyframeDto>(Opts(256));

    public Channel<FilmLoopDto> FilmLoops { get; } =
        Channel.CreateBounded<FilmLoopDto>(Opts(64));

    public Channel<SoundEventDto> Sounds { get; } =
        Channel.CreateBounded<SoundEventDto>(Opts(64));

    public Channel<TempoDto> Tempos { get; } =
        Channel.CreateBounded<TempoDto>(Opts(64));

    public Channel<ColorPaletteDto> ColorPalettes { get; } =
        Channel.CreateBounded<ColorPaletteDto>(Opts(2));

    public Channel<FrameScriptDto> FrameScripts { get; } =
        Channel.CreateBounded<FrameScriptDto>(Opts(256));

    public Channel<TransitionDto> Transitions { get; } =
        Channel.CreateBounded<TransitionDto>(Opts(64));

    public Channel<RNetMemberPropertyDto> MemberProperties { get; } =
        Channel.CreateBounded<RNetMemberPropertyDto>(Opts(256));

    public Channel<TextStyleDto> TextStyles { get; } =
        Channel.CreateBounded<TextStyleDto>(Opts(256));

    public Channel<RNetMoviePropertyDto> MovieProperties { get; } =
        Channel.CreateBounded<RNetMoviePropertyDto>(Opts(64));

    public Channel<RNetStagePropertyDto> StageProperties { get; } =
        Channel.CreateBounded<RNetStagePropertyDto>(Opts(64));

    public Channel<RNetSpriteCollectionEventDto> SpriteCollectionEvents { get; } =
        Channel.CreateBounded<RNetSpriteCollectionEventDto>(Opts(64));

    public Channel<IRNetCommand> Commands { get; } =
        Channel.CreateUnbounded<IRNetCommand>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
}

