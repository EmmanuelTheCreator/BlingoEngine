using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using AbstUI.Primitives;
using AbstUI.Texts;

namespace LingoEngine.Tools
{
    public static class RtfToMarkdown
    {
        private record StyleDef(AbstTextStyle Style, bool HasFont, bool HasSize, bool HasColor);

        /// <summary>
        /// Converts an RTF string into the custom AbstMarkdown format used by <see cref="AbstMarkdownRenderer"/>.
        /// Returns the Markdown string along with the style segments and stylesheet definitions used to build it.
        /// </summary>
        public static AbstMarkdownData Convert(string rtfContent)
        {
            var fontEntries = ParseFontTable(rtfContent);
            var colorEntries = ParseColorTable(rtfContent);
            var styleMap = ParseStyles(rtfContent, fontEntries, colorEntries);
            var styles = styleMap.ToDictionary(kv => kv.Key, kv => kv.Value.Style);
            var segments = ParseSegments(rtfContent, fontEntries, colorEntries);

            var sb = new StringBuilder();
            AbstMDSegment? prev = null;
            foreach (var seg in segments)
            {
                AbstTextStyle? styleDef = null;
                bool styleHasFont = false;
                bool styleHasSize = false;
                bool styleHasColor = false;
                if (seg.StyleId >= 0)
                {
                    sb.Append("{{STYLE:" + seg.StyleId + "}}");
                    if (styleMap.TryGetValue(seg.StyleId.ToString(), out var meta))
                    {
                        styleDef = meta.Style;
                        styleHasFont = meta.HasFont;
                        styleHasSize = meta.HasSize;
                        styleHasColor = meta.HasColor;
                    }
                }

                if (!styleHasFont && (prev == null || seg.FontName != prev.FontName))
                    sb.Append("{{FONT-FAMILY:" + seg.FontName + "}}");
                if (!styleHasSize && (prev == null || seg.Size != prev.Size))
                    sb.Append("{{FONT-SIZE:" + seg.Size + "}}");
                if (!styleHasColor && (prev == null || (seg.Color?.ToHex() != prev.Color?.ToHex())))
                    sb.Append("{{COLOR:" + (seg.Color?.ToHex() ?? "#000000") + "}}");
                if (prev == null || seg.Alignment != prev.Alignment)
                    sb.Append("{{ALIGN:" + seg.Alignment.ToString().ToLowerInvariant() + "}}");

                var text = ApplyStyle(seg.Text, seg);
                sb.Append(text);
                if (seg.StyleId >= 0)
                    sb.Append("{{/STYLE}}");
                prev = seg;
            }

            var plainText = string.Concat(segments.Select(s => s.Text));
            return new AbstMarkdownData
            {
                Markdown = sb.ToString(),
                PlainText = plainText,
                Segments = segments,
                Styles = styles
            };
        }
        private static string ApplyStyle(string text, AbstMDSegment seg)
        {
            if (seg.Bold)
                text = $"**{text}**";
            if (seg.Italic)
                text = $"*{text}*";
            if (seg.Underline)
                text = $"__{text}__";
            return text;
        }

        private static List<AbstMDSegment> ParseSegments(string rtfContent, Dictionary<int, string> fontEntries, List<AColor> colorEntries)
        {
            var segments = new List<AbstMDSegment>();

            var sheet = ExtractGroup(rtfContent, "\\stylesheet");
            if (!string.IsNullOrEmpty(sheet))
                rtfContent = rtfContent.Replace(sheet, string.Empty);

            var blockMatches = Regex.Matches(
                rtfContent,
                @"{[^{}]*?(?:\\plain)?[^{}]*?(?:\\s(?<s>\d+))?[^{}]*?\\f(?<f>\d+)[^{}]*?(?:\\fs(?<fs>\d+))?[^{}]*?\\cf(?<cf>\d+)[^{}]*?(?<text>(?:\\.|[^{}\\])+)}",
                RegexOptions.Singleline);

            if (blockMatches.Count == 0)
                return segments;

            var alignmentMatch = Regex.Match(rtfContent, @"\\q(l|r|j|c)\b");
            AbstTextAlignment alignment = AbstTextAlignment.Left;
            if (alignmentMatch.Success)
            {
                switch (alignmentMatch.Groups[1].Value)
                {
                    case "l": alignment = AbstTextAlignment.Left; break;
                    case "r": alignment = AbstTextAlignment.Right; break;
                    case "c": alignment = AbstTextAlignment.Center; break;
                    case "j": alignment = AbstTextAlignment.Justified; break;
                }
            }

            var marginLeftMatch = Regex.Match(rtfContent, @"\\li(?<val>-?\d+)");
            var marginRightMatch = Regex.Match(rtfContent, @"\\ri(?<val>-?\d+)");
            int marginLeft = marginLeftMatch.Success ? int.Parse(marginLeftMatch.Groups["val"].Value) / 20 : 0;
            int marginRight = marginRightMatch.Success ? int.Parse(marginRightMatch.Groups["val"].Value) / 20 : 0;

            foreach (Match match in blockMatches.Cast<Match>())
            {
                int fontIndex = int.Parse(match.Groups["f"].Value);
                int fontSizeHalfPoints = match.Groups["fs"].Success ? int.Parse(match.Groups["fs"].Value) : 24;
                int colorIndex = int.Parse(match.Groups["cf"].Value);

                fontEntries.TryGetValue(fontIndex, out var fontName);
                AColor? colorL = null;
                if (colorIndex - 1 >= 0 && colorIndex - 1 < colorEntries.Count)
                {
                    colorL = colorEntries[colorIndex - 1];
                }

                bool bold = Regex.IsMatch(match.Value, @"\\b(?!0)");
                bool italic = Regex.IsMatch(match.Value, @"\\i(?!0)");
                bool underline = Regex.IsMatch(match.Value, @"\\ul(?!none|\d)");

                var rawText = match.Groups["text"].Value;
                bool isParagraph = rawText.Contains("\\par");
                var textContent = Regex.Replace(rawText, @"\\'([0-9a-fA-F]{2})", m => ((char)System.Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
                textContent = textContent.Replace("\\par", "\n").Replace("\\tab", "\t").Replace("\\\\", "\\").TrimStart();

                var styleId = match.Groups["s"].Success ? int.Parse(match.Groups["s"].Value) : -1;

                segments.Add(new AbstMDSegment
                {
                    FontName = !string.IsNullOrWhiteSpace(fontName) ? fontName.TrimEnd('*').Trim() : null,
                    Size = System.Convert.ToInt32(fontSizeHalfPoints / 2f),
                    Color = colorL,
                    Text = textContent,
                    Alignment = alignment,
                    Bold = bold,
                    Italic = italic,
                    Underline = underline,
                    MarginLeft = marginLeft,
                    MarginRight = marginRight,
                    StyleId = styleId,
                    IsParagraph = isParagraph
                });
            }

            return segments;
        }

        private static Dictionary<int, string> ParseFontTable(string rtfContent) =>
            Regex.Matches(rtfContent, @"{\\f(?<index>\d+)[^;]*?([^\\;]+);}")
                .Cast<Match>()
                .ToDictionary(
                    m => int.Parse(m.Groups["index"].Value),
                    m =>
                    {
                        var raw = m.Groups[1].Value.Trim();
                        var cleaned = Regex.Replace(raw, @"^(fnil|fswiss|fmodern|fdecor|fscript|ftech|fbidi)\s*", "", RegexOptions.IgnoreCase);
                        return cleaned;
                    });

        private static List<AColor> ParseColorTable(string rtfContent)
        {
            var colorTableMatch = Regex.Match(rtfContent, @"\\colortbl(?<colortbl>[^}]+)}");
            return Regex.Matches(colorTableMatch.Groups["colortbl"].Value, @"\\red(?<r>\d+)\\green(?<g>\d+)\\blue(?<b>\d+);")
                .Cast<Match>()
                .Select(m => new AColor(-1, byte.Parse(m.Groups["r"].Value), byte.Parse(m.Groups["g"].Value), byte.Parse(m.Groups["b"].Value)))
                .ToList();
        }

        private static Dictionary<string, StyleDef> ParseStyles(string rtfContent, Dictionary<int, string> fontEntries, List<AColor> colorEntries)
        {
            var styles = new Dictionary<string, StyleDef>();
            var sheet = ExtractGroup(rtfContent, "\\stylesheet");
            if (string.IsNullOrEmpty(sheet))
                return styles;

            var styleMatches = Regex.Matches(sheet, @"{\\s(?<id>\d+)(?<def>[^}]*)}");
            foreach (Match m in styleMatches.Cast<Match>())
            {
                var id = m.Groups["id"].Value;
                var def = m.Groups["def"].Value;
                var style = new AbstTextStyle { Name = id };
                bool hasFont = false, hasSize = false, hasColor = false;

                var fMatch = Regex.Match(def, @"\\f(\d+)");
                if (fMatch.Success && fontEntries.TryGetValue(int.Parse(fMatch.Groups[1].Value), out var fontName))
                {
                    style.Font = fontName.TrimEnd('*').Trim();
                    hasFont = true;
                }

                var fsMatch = Regex.Match(def, @"\\fs(\d+)");
                if (fsMatch.Success)
                {
                    style.FontSize = int.Parse(fsMatch.Groups[1].Value) / 2;
                    hasSize = true;
                }

                var cfMatch = Regex.Match(def, @"\\cf(\d+)");
                if (cfMatch.Success)
                {
                    int idx = int.Parse(cfMatch.Groups[1].Value) - 1;
                    if (idx >= 0 && idx < colorEntries.Count)
                    {
                        style.Color = colorEntries[idx];
                        hasColor = true;
                    }
                }

                var qMatch = Regex.Match(def, @"\\q(l|r|c|j)");
                if (qMatch.Success)
                {
                    style.Alignment = qMatch.Groups[1].Value switch
                    {
                        "l" => AbstTextAlignment.Left,
                        "r" => AbstTextAlignment.Right,
                        "c" => AbstTextAlignment.Center,
                        "j" => AbstTextAlignment.Justified,
                        _ => AbstTextAlignment.Left
                    };
                }

                if (Regex.IsMatch(def, @"\\b(?!0)")) style.Bold = true;
                if (Regex.IsMatch(def, @"\\i(?!0)")) style.Italic = true;
                if (Regex.IsMatch(def, @"\\ul(?!none|\d)")) style.Underline = true;

                var liMatch = Regex.Match(def, @"\\li(-?\d+)");
                if (liMatch.Success)
                    style.MarginLeft = int.Parse(liMatch.Groups[1].Value) / 20;
                var riMatch = Regex.Match(def, @"\\ri(-?\d+)");
                if (riMatch.Success)
                    style.MarginRight = int.Parse(riMatch.Groups[1].Value) / 20;

                styles[id] = new StyleDef(style, hasFont, hasSize, hasColor);
            }

            return styles;
        }

        private static string ExtractGroup(string rtf, string tag)
        {
            var index = rtf.IndexOf(tag, StringComparison.Ordinal);
            if (index < 0) return string.Empty;
            var start = rtf.LastIndexOf('{', index);
            if (start < 0) return string.Empty;
            int depth = 0;
            for (int i = start; i < rtf.Length; i++)
            {
                if (rtf[i] == '{') depth++;
                else if (rtf[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                        return rtf.Substring(start, i - start + 1);
                }
            }
            return string.Empty;
        }
    }
}
