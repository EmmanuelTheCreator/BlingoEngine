
using AbstUI.Primitives;

namespace AbstUI.Components
{

    public interface IAbstFrameworkInputNumber : IAbstFrameworkNodeInput
    {
        ANumberType NumberType { get; set; }
        int FontSize { get; set; }
    }
    public interface IAbstFrameworkInputNumber<TValue> : IAbstFrameworkInputNumber
#if NET48
        where TValue : struct
#else
        where TValue : System.Numerics.INumber<TValue>
#endif
    {
        TValue Value { get; set; }
        TValue Min { get; set; }
        TValue Max { get; set; }

    }
}
