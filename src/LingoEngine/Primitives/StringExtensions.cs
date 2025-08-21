namespace LingoEngine.Primitives;

public static class StringExtensions
{
    public static int CharToNum(this string value)
        => string.IsNullOrEmpty(value) ? 0 : value[0];
}
