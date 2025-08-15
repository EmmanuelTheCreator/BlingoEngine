namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific spinbox input.
    /// </summary>
    public interface IAbstUIFrameworkGfxSpinBox : IAbstUIFrameworkGfxNodeInput
    {
        float Value { get; set; }
        float Min { get; set; }
        float Max { get; set; }
    }
}
