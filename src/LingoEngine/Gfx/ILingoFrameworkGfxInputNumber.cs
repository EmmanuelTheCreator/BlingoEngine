using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{

    public interface ILingoFrameworkGfxInputNumber : ILingoFrameworkGfxNodeInput
    {
        LingoNumberType NumberType { get; set; }
        int FontSize { get; set; }
    }
    public interface ILingoFrameworkGfxInputNumber<TValue> : ILingoFrameworkGfxInputNumber
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
