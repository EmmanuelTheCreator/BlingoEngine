namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific implementation of a slider input control.
    /// </summary>
    public interface ILingoFrameworkGfxInputSlider<TValue> : ILingoFrameworkGfxNodeInput
    {
        TValue Value { get; set; }
        TValue MinValue { get; set; }
        TValue MaxValue { get; set; }
        TValue Step { get; set; }
    }
}
