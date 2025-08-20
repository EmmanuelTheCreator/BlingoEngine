namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Framework specific spinbox input.
    /// </summary>
    public interface IAbstFrameworkSpinBox : IAbstFrameworkNodeInput
    {
        float Value { get; set; }
        float Min { get; set; }
        float Max { get; set; }
    }
}
