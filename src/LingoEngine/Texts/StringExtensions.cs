namespace LingoEngine.Texts
{
    public static class StringExtensions
    {
        public static string Line(this string text, int index)
        {
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            return lines[index - 1];
        }
        public static string Word(this string text, int index)
        {
            var words = text.Split(new[] { ' ', '\t', '\r', '\n' ,'.',',','!','-','_','?',';',':','/','|'}, StringSplitOptions.RemoveEmptyEntries);
            return words[index - 1];
        }

    }
}
