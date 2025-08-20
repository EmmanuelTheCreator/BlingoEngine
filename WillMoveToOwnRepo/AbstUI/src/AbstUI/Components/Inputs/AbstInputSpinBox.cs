namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Engine level wrapper for a spinbox input.
    /// </summary>
    public class AbstInputSpinBox : AbstInputBase<IAbstFrameworkSpinBox>
    {
        public float Value { get => _framework.Value; set => _framework.Value = value; }
        public float Min { get => _framework.Min; set => _framework.Min = value; }
        public float Max { get => _framework.Max; set => _framework.Max = value; }
    }
}
