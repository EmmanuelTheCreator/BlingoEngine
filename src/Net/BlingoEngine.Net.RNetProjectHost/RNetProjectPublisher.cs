using System;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;

namespace BlingoEngine.Net.RNetProjectHost;

/// <summary>
/// Default implementation of <see cref="IRNetPublisher"/> that pushes updates through the shared bus used by the SignalR host.
/// </summary>

internal sealed class RNetProjectPublisher : RNetPublisherBase
{
    private readonly IRNetProjectBus _bus;

    /// <summary>Initializes a new instance of the <see cref="RNetProjectPublisher"/> class.</summary>
    public RNetProjectPublisher(IRNetProjectBus bus) => _bus = bus;

    /// <inheritdoc />
    protected override bool TryPublishFrameCore(StageFrameDto frame)
        => _bus.Frames.Writer.TryWrite(frame);

    /// <inheritdoc />
    protected override bool TryPublishDeltaCore(SpriteDeltaDto delta)
        => _bus.Deltas.Writer.TryWrite(delta);

    /// <inheritdoc />
    protected override bool TryPublishKeyframeCore(KeyframeDto keyframe)
        => _bus.Keyframes.Writer.TryWrite(keyframe);

    /// <inheritdoc />
    protected override bool TryPublishFilmLoopCore(FilmLoopDto filmLoop)
        => _bus.FilmLoops.Writer.TryWrite(filmLoop);

    /// <inheritdoc />
    protected override bool TryPublishSoundCore(SoundEventDto sound)
        => _bus.Sounds.Writer.TryWrite(sound);

    /// <inheritdoc />
    protected override bool TryPublishTempoCore(TempoDto tempo)
        => _bus.Tempos.Writer.TryWrite(tempo);

    /// <inheritdoc />
    protected override bool TryPublishColorPaletteCore(ColorPaletteDto palette)
        => _bus.ColorPalettes.Writer.TryWrite(palette);

    /// <inheritdoc />
    protected override bool TryPublishFrameScriptCore(FrameScriptDto script)
        => _bus.FrameScripts.Writer.TryWrite(script);

    /// <inheritdoc />
    protected override bool TryPublishTransitionCore(TransitionDto transition)
        => _bus.Transitions.Writer.TryWrite(transition);

    /// <inheritdoc />
    protected override bool TryPublishTextStyleCore(TextStyleDto style)
        => _bus.TextStyles.Writer.TryWrite(style);

    /// <inheritdoc />
    protected override bool TryPublishSpriteCollectionEventCore(RNetSpriteCollectionEventDto evt)
        => _bus.SpriteCollectionEvents.Writer.TryWrite(evt);

    /// <inheritdoc />
    protected override bool TryPublishMemberPropertyCore(RNetMemberPropertyDto property)
        => _bus.MemberProperties.Writer.TryWrite(property);

    /// <inheritdoc />
    protected override bool TryPublishMoviePropertyCore(RNetMoviePropertyDto property)
        => _bus.MovieProperties.Writer.TryWrite(property);

    /// <inheritdoc />
    protected override bool TryPublishStagePropertyCore(RNetStagePropertyDto property)
        => _bus.StageProperties.Writer.TryWrite(property);

    /// <inheritdoc />
    public override bool TryDrainCommands(Action<IRNetCommand> apply)
    {
        var reader = _bus.Commands.Reader;
        var had = false;
        while (reader.TryRead(out var cmd))
        {
            had = true;
            apply(cmd);
        }

        return had;
    }
}

