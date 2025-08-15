using AbstUI.Primitives;

namespace AbstUI.Components
{

    public struct NullableNum<TValue>
#if NET48
        where TValue : struct
#else
        where TValue : System.Numerics.INumber<TValue>
#endif
    {
        private TValue? _Value;
        public TValue Value => _Value ?? throw new InvalidOperationException("Value is null. Use HasValue to check if a value is present.");
        public bool HasValue { get; set; }
        public NullableNum()
        {
            HasValue = false;
            _Value = default;
        }
        public NullableNum(TValue? value)
        {
            HasValue = value != null;
            _Value = value;
        }
        public static implicit operator NullableNum<TValue>(TValue value)
           => new(value);

        public static implicit operator TValue?(NullableNum<TValue> value)
            => value.Value;
    }

    /// <summary>
    /// Engine level wrapper for a numeric input field.
    /// </summary>
    public class AbstUIGfxInputNumber : AbstUIGfxInputNumber<float>
    {

    }
    /// <summary>
    /// Engine level wrapper for a numeric input field.
    /// </summary>
    public class AbstUIGfxInputNumber<TValue> : AbstUIGfxInputBase<IAbstUIFrameworkGfxInputNumber<TValue>>
#if NET48
        where TValue : struct
#else
        where TValue : System.Numerics.INumber<TValue>
#endif
    {
        public TValue Value { get => _framework.Value; set => _framework.Value = value; }
        public TValue Min { get => _framework.Min; set => _framework.Min = value; }
        public TValue Max { get => _framework.Max; set => _framework.Max = value; }

        public ANumberType NumberType { get => _framework.NumberType; set => _framework.NumberType = value; }
        public int FontSize { get => _framework.FontSize; set => _framework.FontSize = value; }
    }
}
