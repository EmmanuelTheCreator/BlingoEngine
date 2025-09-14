using LingoEngine.Net.RNetContracts;

namespace LingoEngine.Net.RNetHost;

/// <summary>
/// Helpers used by the Director engine to publish debug information.
/// </summary>
public interface IDebugPublisher
{
    /// <summary>Attempts to publish a stage frame without blocking.</summary>
    void TryPublishFrame(StageFrameDto frame);

    /// <summary>Attempts to publish a sprite delta without blocking.</summary>
    void TryPublishDelta(SpriteDeltaDto delta);

    /// <summary>Attempts to publish a keyframe without blocking.</summary>
    void TryPublishKeyframe(KeyframeDto keyframe);

    /// <summary>Attempts to publish a film loop state change without blocking.</summary>
    void TryPublishFilmLoop(FilmLoopDto filmLoop);

    /// <summary>Attempts to publish a sound event without blocking.</summary>
    void TryPublishSound(SoundEventDto sound);

    /// <summary>Attempts to publish a tempo change without blocking.</summary>
    void TryPublishTempo(TempoDto tempo);

    /// <summary>Attempts to publish a color palette without blocking.</summary>
    void TryPublishColorPalette(ColorPaletteDto palette);

    /// <summary>Attempts to publish a frame script without blocking.</summary>
    void TryPublishFrameScript(FrameScriptDto script);

    /// <summary>Attempts to publish a transition without blocking.</summary>
    void TryPublishTransition(TransitionDto transition);

    /// <summary>Attempts to publish a member property without blocking.</summary>
    void TryPublishMemberProperty(MemberPropertyDto property);

    /// <summary>Attempts to publish a text style without blocking.</summary>
    void TryPublishTextStyle(TextStyleDto style);

    /// <summary>Drains queued debug commands and applies them through the provided delegate.</summary>
    /// <param name="apply">Delegate invoked for each command.</param>
    /// <returns><c>true</c> if any command was processed; otherwise, <c>false</c>.</returns>
    bool TryDrainCommands(Action<DebugCommandDto> apply);
}

/// <summary>
/// Default implementation of <see cref="IDebugPublisher"/>.
/// </summary>
internal sealed class DebugPublisher : IDebugPublisher
{
    private readonly IBus _bus;

    /// <summary>Initializes a new instance of the <see cref="DebugPublisher"/> class.</summary>
    public DebugPublisher(IBus bus) => _bus = bus;

    /// <inheritdoc />
    public void TryPublishFrame(StageFrameDto frame)
        => _bus.Frames.Writer.TryWrite(frame);

    /// <inheritdoc />
    public void TryPublishDelta(SpriteDeltaDto delta)
        => _bus.Deltas.Writer.TryWrite(delta);

    /// <inheritdoc />
    public void TryPublishKeyframe(KeyframeDto keyframe)
        => _bus.Keyframes.Writer.TryWrite(keyframe);

    /// <inheritdoc />
    public void TryPublishFilmLoop(FilmLoopDto filmLoop)
        => _bus.FilmLoops.Writer.TryWrite(filmLoop);

    /// <inheritdoc />
    public void TryPublishSound(SoundEventDto sound)
        => _bus.Sounds.Writer.TryWrite(sound);

    /// <inheritdoc />
    public void TryPublishTempo(TempoDto tempo)
        => _bus.Tempos.Writer.TryWrite(tempo);

    /// <inheritdoc />
    public void TryPublishColorPalette(ColorPaletteDto palette)
        => _bus.ColorPalettes.Writer.TryWrite(palette);

    /// <inheritdoc />
    public void TryPublishFrameScript(FrameScriptDto script)
        => _bus.FrameScripts.Writer.TryWrite(script);

    /// <inheritdoc />
    public void TryPublishTransition(TransitionDto transition)
        => _bus.Transitions.Writer.TryWrite(transition);

    /// <inheritdoc />
    public void TryPublishMemberProperty(MemberPropertyDto property)
        => _bus.MemberProperties.Writer.TryWrite(property);

    /// <inheritdoc />
    public void TryPublishTextStyle(TextStyleDto style)
        => _bus.TextStyles.Writer.TryWrite(style);

    /// <inheritdoc />
    public bool TryDrainCommands(Action<DebugCommandDto> apply)
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
