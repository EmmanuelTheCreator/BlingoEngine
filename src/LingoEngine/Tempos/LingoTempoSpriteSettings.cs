namespace LingoEngine.Tempos;

public class LingoTempoSpriteSettings
    {
    public LingoTempoSpriteAction Action { get; set; }
    public int Tempo { get; set; } = 30;

    /// <summary>
    /// Number of seconds to wait when <see cref="Action"/> is <see cref="LingoTempoSpriteAction.WaitSeconds"/>.
    /// </summary>
    public float WaitSeconds { get; set; } = 5;

    /// <summary>
    /// Cue channel to wait on when <see cref="Action"/> is <see cref="LingoTempoSpriteAction.WaitForCuePoint"/>.
    /// </summary>
    public int CueChannel { get; set; }

    /// <summary>
    /// Cue point to wait for when <see cref="Action"/> is <see cref="LingoTempoSpriteAction.WaitForCuePoint"/>.
    /// </summary>
    public int CuePoint { get; set; }

}
