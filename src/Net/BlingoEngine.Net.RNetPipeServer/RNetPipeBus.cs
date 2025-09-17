using System.Threading.Channels;
using BlingoEngine.Net.RNetContracts;

namespace BlingoEngine.Net.RNetPipeServer;

/// <summary>
/// Channels used to communicate between the Lingo engine runtime and the pipe transport.
/// </summary>
public interface IRNetPipeBus
{
    Channel<StageFrameDto> Frames { get; }
    Channel<SpriteDeltaDto> Deltas { get; }
    Channel<KeyframeDto> Keyframes { get; }
    Channel<FilmLoopDto> FilmLoops { get; }
    Channel<SoundEventDto> Sounds { get; }
    Channel<TempoDto> Tempos { get; }
    Channel<ColorPaletteDto> ColorPalettes { get; }
    Channel<FrameScriptDto> FrameScripts { get; }
    Channel<TransitionDto> Transitions { get; }
    Channel<RNetMemberPropertyDto> MemberProperties { get; }
    Channel<TextStyleDto> TextStyles { get; }
    Channel<RNetMoviePropertyDto> MovieProperties { get; }
    Channel<RNetStagePropertyDto> StageProperties { get; }
    Channel<RNetSpriteCollectionEventDto> SpriteCollectionEvents { get; }
    Channel<IRNetCommand> Commands { get; }
}

internal sealed class RNetPipeBus : IRNetPipeBus
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
            SingleWriter = false,
            SingleReader = false
        });
}

