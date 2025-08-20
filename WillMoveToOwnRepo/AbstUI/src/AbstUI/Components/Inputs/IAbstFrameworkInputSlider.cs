namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Framework specific implementation of a slider input control.
    /// </summary>
    public interface IAbstFrameworkInputSlider<TValue> : IAbstFrameworkNodeInput
    {
        TValue Value { get; set; }
        TValue MinValue { get; set; }
        TValue MaxValue { get; set; }
        TValue Step { get; set; }
    }
}
