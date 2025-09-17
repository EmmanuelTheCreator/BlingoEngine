namespace BlingoEngine.VerboseLanguage
{
    public interface IBlingoVerbosePropAccess<TValue>
    {
        TValue this[IBlingoVerbosePropAccess<TValue> p] { get; set; }

        TValue Value { get; set; }
        IBlingoVerbosePropAccess<TValue> To(TValue value);
    }

}




