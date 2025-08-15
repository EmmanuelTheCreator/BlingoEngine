namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a slider input control.
    /// </summary>
    public class AbstUIGfxInputSlider<TValue> : AbstUIGfxInputBase<IAbstUIFrameworkGfxInputSlider<TValue>>
    {
        /// <summary>Current value of the slider.</summary>
        public TValue Value { get => _framework.Value; set => _framework.Value = value; }
        /// <summary>Minimum allowed value.</summary>
        public TValue MinValue { get => _framework.MinValue; set => _framework.MinValue = value; }
        /// <summary>Maximum allowed value.</summary>
        public TValue MaxValue { get => _framework.MaxValue; set => _framework.MaxValue = value; }
        /// <summary>Step size when moving the slider.</summary>
        public TValue Step { get => _framework.Step; set => _framework.Step = value; }
    }
}
