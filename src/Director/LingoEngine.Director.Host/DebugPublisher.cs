using LingoEngine.Director.Contracts;

namespace LingoEngine.Director.Host;

/// <summary>
/// Helpers used by the Director engine to publish debug information.
/// </summary>
public interface IDebugPublisher
{
    /// <summary>Attempts to publish a stage frame without blocking.</summary>
    void TryPublishFrame(StageFrameDto frame);

    /// <summary>Attempts to publish a sprite delta without blocking.</summary>
    void TryPublishDelta(SpriteDeltaDto delta);

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
