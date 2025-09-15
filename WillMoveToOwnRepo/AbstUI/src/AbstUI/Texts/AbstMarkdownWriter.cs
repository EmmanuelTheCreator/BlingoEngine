using System.Collections.Generic;
using System.Text;
using AbstUI.Primitives;

namespace AbstUI.Texts
{
    /// <summary>
    /// Helper for emitting markdown strings with an optional style sheet.
    /// </summary>
    public static class AbstMarkdownWriter
    {
        /// <summary>
        /// Writes a basic markdown string containing a single style definition
        /// and paragraph tags for each line in the provided text.
        /// </summary>
        public static string WriteBasic(
            string text,
            string font,
            int fontSize,
            AColor color,
            AbstTextAlignment alignment,
            bool bold,
            bool italic,
            bool underline,
            int marginLeft = 0,
            int marginRight = 0,
            int lineHeight = 0,
            int letterSpacing = 0)
        {
            var sheet = new Dictionary<string, MarkdownStyleSheetTTO>();
            var style = new MarkdownStyleSheetTTO
            {
                FontFamily = string.IsNullOrEmpty(font) ? null : font,
                FontSize = fontSize > 0 ? fontSize : null,
                Color = color.ToHex() != "#000000" ? color.ToHex() : null,
                TextAlign = alignment != AbstTextAlignment.Left ? alignment.ToString().ToLowerInvariant() : null,
                FontWeight = bold ? "bold" : null,
                FontStyle = italic ? "italic" : null,
                TextDecoration = underline ? "underline" : null,
                MarginLeft = marginLeft != 0 ? marginLeft : null,
                MarginRight = marginRight != 0 ? marginRight : null,
                LineHeight = lineHeight != 0 ? lineHeight : null,
                LetterSpacing = letterSpacing != 0 ? letterSpacing : null
            };
            sheet["0"] = style;
#if NET48
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(
                sheet,
                new Newtonsoft.Json.JsonSerializerSettings
                {
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                });
#else
            var json = System.Text.Json.JsonSerializer.Serialize(
                sheet,
                new System.Text.Json.JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
#endif
            var sb = new StringBuilder();
            sb.Append("{{STYLE-SHEET:");
            sb.Append(json);
            sb.Append("}}");
            var lines = text.Replace("\r", string.Empty).Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                    sb.Append('\n');
                sb.Append("{{PARA:0}}");
                sb.Append(lines[i]);
            }
            return sb.ToString();
        }
    }
}

