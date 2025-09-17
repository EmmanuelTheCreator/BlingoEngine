using System;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;

namespace BlingoEngine.Net.RNetPipeServer;

/// <summary>
/// Pipe-based implementation of <see cref="IRNetPublisher"/> that pushes updates through the shared bus.
/// </summary>
public sealed class RNetPipePublisher : RNetPublisherBase
{
    private readonly IRNetPipeBus _bus;

    public RNetPipePublisher(IRNetPipeBus bus) => _bus = bus;

    protected override bool TryPublishFrameCore(StageFrameDto frame)
        => _bus.Frames.Writer.TryWrite(frame);

    protected override bool TryPublishDeltaCore(SpriteDeltaDto delta)
        => _bus.Deltas.Writer.TryWrite(delta);

    protected override bool TryPublishKeyframeCore(KeyframeDto keyframe)
        => _bus.Keyframes.Writer.TryWrite(keyframe);

    protected override bool TryPublishFilmLoopCore(FilmLoopDto filmLoop)
        => _bus.FilmLoops.Writer.TryWrite(filmLoop);

    protected override bool TryPublishSoundCore(SoundEventDto sound)
        => _bus.Sounds.Writer.TryWrite(sound);

    protected override bool TryPublishTempoCore(TempoDto tempo)
        => _bus.Tempos.Writer.TryWrite(tempo);

    protected override bool TryPublishColorPaletteCore(ColorPaletteDto palette)
        => _bus.ColorPalettes.Writer.TryWrite(palette);

    protected override bool TryPublishFrameScriptCore(FrameScriptDto script)
        => _bus.FrameScripts.Writer.TryWrite(script);

    protected override bool TryPublishTransitionCore(TransitionDto transition)
        => _bus.Transitions.Writer.TryWrite(transition);

    protected override bool TryPublishTextStyleCore(TextStyleDto style)
        => _bus.TextStyles.Writer.TryWrite(style);

    protected override bool TryPublishSpriteCollectionEventCore(RNetSpriteCollectionEventDto evt)
        => _bus.SpriteCollectionEvents.Writer.TryWrite(evt);

    protected override bool TryPublishMemberPropertyCore(RNetMemberPropertyDto property)
        => _bus.MemberProperties.Writer.TryWrite(property);

    protected override bool TryPublishMoviePropertyCore(RNetMoviePropertyDto property)
        => _bus.MovieProperties.Writer.TryWrite(property);

    protected override bool TryPublishStagePropertyCore(RNetStagePropertyDto property)
        => _bus.StageProperties.Writer.TryWrite(property);

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

