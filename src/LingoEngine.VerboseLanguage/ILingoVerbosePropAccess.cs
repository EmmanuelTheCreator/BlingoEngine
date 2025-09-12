namespace LingoEngine.VerboseLanguage
{
    public interface ILingoVerbosePropAccess<TValue>
    {
        TValue this[ILingoVerbosePropAccess<TValue> p] { get; set; }

        TValue Value { get; set; }
        ILingoVerbosePropAccess<TValue> To(TValue value);
    }

}



