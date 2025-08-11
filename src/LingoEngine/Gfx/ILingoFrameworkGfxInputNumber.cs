using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
   
    public interface ILingoFrameworkGfxInputNumber : ILingoFrameworkGfxNodeInput
    {
        LingoNumberType NumberType { get; set; }
        int FontSize { get; set; }
    }
    public interface ILingoFrameworkGfxInputNumber<TValue> : ILingoFrameworkGfxInputNumber
        where TValue : System.Numerics.INumber<TValue>
    {
        TValue Value { get; set; }
        TValue Min { get; set; }
        TValue Max { get; set; }
      
    }
}
