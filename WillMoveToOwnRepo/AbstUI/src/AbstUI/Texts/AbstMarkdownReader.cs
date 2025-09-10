
using AbstUI.Primitives;
using System.Text.RegularExpressions;

namespace AbstUI.Texts
{
    public class AbstMarkdownReader
    {
        public static AbstMarkdownData Read(string markdownContent)
        {
            var data = new AbstMarkdownData();
            data.Markdown = markdownContent;
            IEnumerable<AbstTextStyle> styles;
            if (!string.IsNullOrWhiteSpace(markdownContent) && TryExtractStyleSheet(ref markdownContent, out styles))
            {
                data.Markdown = markdownContent;
                data.Styles = styles.ToDictionary(x => x.Name);
            } 
            data.PlainText = RetrieveTextOnly(data.Markdown);
            return data;
        }

     
            /// <summary>
            /// Removes Markdown and custom {{...}} tags, leaving only plain text.
            /// </summary>
            public static string RetrieveTextOnly(string markdown)
            {
                if (string.IsNullOrEmpty(markdown))
                    return string.Empty;

                var text = markdown;

                // 1. Remove custom {{...}} tags
                text = Regex.Replace(text, @"\{\{.*?\}\}", string.Empty, RegexOptions.Singleline);

                // 2. Remove images ![alt](url)
                text = Regex.Replace(text, @"!\[.*?\]\(.*?\)", string.Empty);

                // 3. Replace links [text](url) → keep only "text"
                text = Regex.Replace(text, @"\[(.*?)\]\(.*?\)", "$1");

                // 4. Remove headings (#, ##, ###) → keep only text
                text = Regex.Replace(text, @"^\s{0,3}#{1,6}\s*", string.Empty, RegexOptions.Multiline);

                // 5. Remove bold **text** → keep only "text"
                text = Regex.Replace(text, @"\*\*(.*?)\*\*", "$1");

                // 6. Remove italic *text* → keep only "text"
                text = Regex.Replace(text, @"\*(.*?)\*", "$1");

                // 7. Remove underline __text__ → keep only "text"
                text = Regex.Replace(text, @"__(.*?)__", "$1");

                // 8. Collapse multiple spaces (but keep newlines)
                text = Regex.Replace(text, @"[^\S\r\n]+", " ");

                // 9. Collapse 3+ newlines into 2 (keep paragraph separation)
                text = Regex.Replace(text, @"(\r?\n){3,}", "\n\n");


                return text.Trim();
            }
        

        public static bool TryExtractStyleSheet(ref string markdown, out IEnumerable<AbstTextStyle> styles)
        {
            styles = Enumerable.Empty<AbstTextStyle>();
            const string tag = "{{STYLE-SHEET:";
            if (!markdown.StartsWith(tag, StringComparison.Ordinal))
                return false;
            int jsonEnd = markdown.IndexOf("}}", tag.Length, StringComparison.Ordinal);
            if (jsonEnd < 0)
                return false;
            int end = markdown.IndexOf("}}", jsonEnd + 2, StringComparison.Ordinal);
            if (end < 0)
                return false;
            var json = markdown.Substring(tag.Length, jsonEnd - tag.Length + 2);
            try
            {
#if NET48
                var sheet = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, MarkdownStyleSheetTTO>>(json);
#else
                var sheet = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, MarkdownStyleSheetTTO>>(json);
#endif
                if (sheet == null)
                    return false;
                styles = sheet.Select(kv => new AbstTextStyle
                {
                    Name = kv.Key,
                    Font = kv.Value.FontFamily ?? string.Empty,
                    FontSize = kv.Value.FontSize ?? 0,
                    Color = kv.Value.Color != null ? AColor.FromHex(kv.Value.Color) : AColors.Black,
                    Alignment = kv.Value.TextAlign?.ToLowerInvariant() switch
                    {
                        "center" => AbstTextAlignment.Center,
                        "right" => AbstTextAlignment.Right,
                        "justify" or "justified" => AbstTextAlignment.Justified,
                        _ => AbstTextAlignment.Left
                    },
                    Bold = kv.Value.FontWeight?.Equals("bold", StringComparison.OrdinalIgnoreCase) == true,
                    Italic = kv.Value.FontStyle?.Equals("italic", StringComparison.OrdinalIgnoreCase) == true,
                    Underline = kv.Value.TextDecoration?.Equals("underline", StringComparison.OrdinalIgnoreCase) == true,
                    LineHeight = kv.Value.LineHeight ?? 0,
                    MarginLeft = kv.Value.MarginLeft ?? 0,
                    MarginRight = kv.Value.MarginRight ?? 0
                }).ToList();
                markdown = markdown.Substring(end + 2);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
