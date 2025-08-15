namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific implementation of a slider input control.
    /// </summary>
    public interface IAbstUIFrameworkGfxInputSlider<TValue> : IAbstUIFrameworkGfxNodeInput
    {
        TValue Value { get; set; }
        TValue MinValue { get; set; }
        TValue MaxValue { get; set; }
        TValue Step { get; set; }
    }
}
