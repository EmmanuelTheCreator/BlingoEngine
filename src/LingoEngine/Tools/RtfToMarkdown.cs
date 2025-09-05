using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using AbstUI.Primitives;
using AbstUI.Texts;

namespace LingoEngine.Tools
{
    public static class RtfToMarkdown
    {
        private record StyleDef(AbstTextStyle Style, bool HasFont, bool HasSize, bool HasColor, bool HasAlignment);

        /// <summary>
        /// Converts an RTF string into the custom AbstMarkdown format used by <see cref="AbstMarkdownRenderer"/>.
        /// Returns the Markdown string along with the style segments and stylesheet definitions used to build it.
        /// </summary>
        public static AbstMarkdownData Convert(string rtfContent, bool includeStyleSheet = false)
        {
            var fontEntries = ParseFontTable(rtfContent);
            var colorEntries = ParseColorTable(rtfContent, out int colorOffset);
            var styleMap = ParseStyles(rtfContent, fontEntries, colorEntries, colorOffset);
            var segments = ParseSegments(rtfContent, fontEntries, colorEntries, colorOffset);

            if (segments.Select(s => s.StyleId).All(id => id < 0))
            {
                var distinct = segments.Select(s => (s.FontName, s.Size, s.Color?.ToHex(), s.Alignment, s.MarginLeft, s.MarginRight, s.LineHeight)).Distinct().Count();
                if (distinct == 1 && segments.Count > 0)
                {
                    var seg = segments[0];
                    var style = new AbstTextStyle
                    {
                        Name = "0",
                        Font = seg.FontName ?? "",
                        FontSize = seg.Size,
                        Color = seg.Color ?? AColors.Black,
                        Alignment = seg.Alignment,
                        LineHeight = seg.LineHeight,
                        MarginLeft = seg.MarginLeft,
                        MarginRight = seg.MarginRight
                    };
                    styleMap["0"] = new StyleDef(style, true, true, seg.Color != null, true);
                    foreach (var s in segments)
                    {
                        s.StyleId = 0;
                    }
                }
            }

            if (segments.Count > 0 && segments[0].StyleId < 0)
            {
                int newId = 0;
                while (styleMap.ContainsKey(newId.ToString()))
                    newId++;
                var first = segments[0];
                var style = new AbstTextStyle
                {
                    Name = newId.ToString(),
                    Font = first.FontName,
                    FontSize = first.Size,
                    Color = first.Color ?? AColors.Black,
                    Alignment = first.Alignment,
                    LineHeight = first.LineHeight,
                    MarginLeft = first.MarginLeft,
                    MarginRight = first.MarginRight
                };
                styleMap[style.Name] = new StyleDef(style, true, true, first.Color != null, true);
                first.StyleId = newId;
            }

            var sb = new StringBuilder();
            AbstMDSegment? prev = null;
            string? currentStyle = null;
            bool styleOpened = false;
            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                bool styleHasFont = false;
                bool styleHasSize = false;
                bool styleHasColor = false;
                bool styleHasAlignment = false;

                bool isParagraphStart = i == 0 || (prev?.IsParagraph == true);
                if (isParagraphStart)
                {
                    if (seg.StyleId < 0)
                    {
                        var match = styleMap.FirstOrDefault(kv => StyleMatches(kv.Value.Style, seg));
                        if (match.Value != null)
                        {
                            seg.StyleId = int.Parse(match.Key);
                        }
                        else
                        {
                            int newId = 0;
                            while (styleMap.ContainsKey(newId.ToString()))
                                newId++;
                            var style = new AbstTextStyle
                            {
                                Name = newId.ToString(),
                                Font = seg.FontName ?? string.Empty,
                                FontSize = seg.Size,
                                Color = seg.Color ?? AColors.Black,
                                Alignment = seg.Alignment,
                                LineHeight = seg.LineHeight,
                                MarginLeft = seg.MarginLeft,
                                MarginRight = seg.MarginRight
                            };
                            styleMap[style.Name] = new StyleDef(style, true, true, seg.Color != null, true);
                            seg.StyleId = newId;
                        }
                    }

                    var paraTag = seg.StyleId >= 0 ? $"{{{{PARA:{seg.StyleId}}}}}" : "{{PARA}}";
                    if (i == 0)
                        sb.Append(paraTag);
                    else
                    {
                        var idx = sb.ToString().LastIndexOf("{{PARA", StringComparison.Ordinal);
                        if (idx >= 0)
                        {
                            var end = sb.ToString().IndexOf("}}", idx, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                sb.Remove(idx, end - idx + 2);
                                sb.Insert(idx, paraTag);
                            }
                        }
                    }

                    if (seg.StyleId >= 0 && styleMap.TryGetValue(seg.StyleId.ToString(), out var meta))
                    {
                        styleHasFont = meta.HasFont;
                        styleHasSize = meta.HasSize;
                        styleHasColor = meta.HasColor;
                        styleHasAlignment = meta.HasAlignment;
                        currentStyle = seg.StyleId.ToString();
                    }
                    else
                    {
                        currentStyle = null;
                    }
                }
                else
                {
                    if (seg.StyleId >= 0)
                    {
                        var id = seg.StyleId.ToString();
                        if (currentStyle != id)
                        {
                            if (styleOpened)
                                sb.Append("{{/STYLE}}");
                            sb.Append("{{STYLE:" + id + "}}");
                            styleOpened = true;
                            currentStyle = id;
                        }
                        if (styleMap.TryGetValue(id, out var meta))
                        {
                            styleHasFont = meta.HasFont;
                            styleHasSize = meta.HasSize;
                            styleHasColor = meta.HasColor;
                            styleHasAlignment = meta.HasAlignment;
                        }
                    }
                    else
                    {
                        if (styleOpened)
                            sb.Append("{{/STYLE}}");
                        styleOpened = false;
                        currentStyle = null;
                        // (No style meta, but check for parameter diff below)
                    }
                }

                if (!styleHasFont && (prev == null || seg.FontName != prev.FontName))
                    sb.Append("{{FONT-FAMILY:" + seg.FontName + "}}");
                if (!styleHasSize && (prev == null || seg.Size != prev.Size))
                    sb.Append("{{FONT-SIZE:" + seg.Size + "}}");
                if (!styleHasColor && (prev == null || (seg.Color?.ToHex() != prev.Color?.ToHex())))
                    sb.Append("{{COLOR:" + (seg.Color?.ToHex() ?? "#000000") + "}}");
                if (!styleHasAlignment && (prev == null || seg.Alignment != prev.Alignment))
                    sb.Append("{{ALIGN:" + seg.Alignment.ToString().ToLowerInvariant() + "}}");

                var text = ApplyStyle(seg.Text, seg);
                text = text.Replace("\r\n", "\n");
                text = Regex.Replace(text, @"\n{2,}", "\n");
                if (seg.IsParagraph)
                {
                    var paraTag = seg.StyleId >= 0 ? $"{{{{PARA:{seg.StyleId}}}}}" : "{{PARA}}";
                    text = text.Replace("\n", "\n" + paraTag);
                }
                sb.Append(text);
                prev = seg;
            }
            if (styleOpened)
                sb.Append("{{/STYLE}}");

            var styles = styleMap.ToDictionary(kv => kv.Key, kv => kv.Value.Style);
            var markdown = Regex.Replace(sb.ToString(), @"\n{2,}", "\n");
            markdown = Regex.Replace(markdown, @"\n\s+", "\n");

            if (includeStyleSheet)
            {
                var sheet = styles.ToDictionary(kv => kv.Key, kv =>
                {
                    var s = kv.Value;
                    var tto = new MarkdownStyleSheetTTO();
                    if (!string.IsNullOrEmpty(s.Font))
                        tto.FontFamily = s.Font;
                    if (s.FontSize > 0)
                        tto.FontSize = s.FontSize;
                    var hex = s.Color.ToHex();
                    if (!string.Equals(hex, "#000000", StringComparison.OrdinalIgnoreCase))
                        tto.Color = hex;
                    if (s.Alignment != AbstTextAlignment.Left)
                        tto.TextAlign = s.Alignment.ToString().ToLowerInvariant();
                    if (s.Bold)
                        tto.FontWeight = "bold";
                    if (s.Italic)
                        tto.FontStyle = "italic";
                    if (s.Underline)
                        tto.TextDecoration = "underline";
                    if (s.LineHeight > 0)
                        tto.LineHeight = s.LineHeight;
                    if (s.MarginLeft != 0)
                        tto.MarginLeft = s.MarginLeft;
                    if (s.MarginRight != 0)
                        tto.MarginRight = s.MarginRight;
                    return tto;
                });
                var json = JsonSerializer.Serialize(sheet, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
                markdown = $"{{{{STYLE-SHEET:{json}}}}}" + markdown;
            }

            var plainTextRaw = string.Concat(segments.Select(s => s.Text));
            var plainText = Regex.Replace(plainTextRaw, @"\n{2,}", "\n");
            plainText = Regex.Replace(plainText, @"\n\s+", "\n");
            return new AbstMarkdownData
            {
                Markdown = markdown,
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

        private static bool StyleMatches(AbstTextStyle style, AbstMDSegment seg)
        {
            var styleColor = style.Color.ToHex();
            var segColor = (seg.Color ?? AColors.Black).ToHex();

            return string.Equals(style.Font, seg.FontName, StringComparison.OrdinalIgnoreCase)
                && style.FontSize == seg.Size
                && string.Equals(styleColor, segColor, StringComparison.OrdinalIgnoreCase)
                && style.Alignment == seg.Alignment
                && style.MarginLeft == seg.MarginLeft
                && style.MarginRight == seg.MarginRight
                && style.LineHeight == seg.LineHeight;
        }

        private static List<AbstMDSegment> ParseSegments(string rtfContent, Dictionary<int, string> fontEntries, List<AColor> colorEntries, int colorOffset)
        {
            var segments = new List<AbstMDSegment>();

            var sheet = ExtractGroup(rtfContent, "\\stylesheet");
            if (!string.IsNullOrEmpty(sheet))
                rtfContent = rtfContent.Replace(sheet, string.Empty);

            var fonttbl = ExtractGroup(rtfContent, "\\fonttbl");
            if (!string.IsNullOrEmpty(fonttbl))
                rtfContent = rtfContent.Replace(fonttbl, string.Empty);

            var colortbl = ExtractGroup(rtfContent, "\\colortbl");
            if (!string.IsNullOrEmpty(colortbl))
                rtfContent = rtfContent.Replace(colortbl, string.Empty);

            var blockMatches = Regex.Matches(
                rtfContent,
                @"{[^{}]*?(?:\\plain)?[^{}]*?(?:\\s(?<s>\d+))?[^{}]*?\\f(?<f>\d+)[^{}]*?(?:\\fs(?<fs>\d+))?[^{}]*?\\cf(?<cf>\d+)[^{}]*?(?<text>(?:\\.|[^{}\\])+)}",
                RegexOptions.Singleline);

            if (blockMatches.Count == 0)
                return segments;

            int lastIndex = 0;

            var alignmentMatch = Regex.Match(rtfContent, @"\\q(l|r|j|c)\b");
            AbstTextAlignment defaultAlignment = AbstTextAlignment.Left;
            if (alignmentMatch.Success)
            {
                defaultAlignment = alignmentMatch.Groups[1].Value switch
                {
                    "l" => AbstTextAlignment.Left,
                    "r" => AbstTextAlignment.Right,
                    "c" => AbstTextAlignment.Center,
                    "j" => AbstTextAlignment.Justified,
                    _ => AbstTextAlignment.Left
                };
            }

            var marginLeftMatch = Regex.Match(rtfContent, @"\\li(?<val>-?\d+)");
            var marginRightMatch = Regex.Match(rtfContent, @"\\ri(?<val>-?\d+)");
            int defaultMarginLeft = marginLeftMatch.Success ? int.Parse(marginLeftMatch.Groups["val"].Value) / 20 : 0;
            int defaultMarginRight = marginRightMatch.Success ? int.Parse(marginRightMatch.Groups["val"].Value) / 20 : 0;

            foreach (Match match in blockMatches.Cast<Match>())
            {
                var gap = rtfContent.Substring(lastIndex, match.Index - lastIndex);
                int gapPars = Regex.Matches(gap, @"\\par").Count;
                if (gapPars > 0 && segments.Count > 0)
                {
                    segments[segments.Count - 1].Text += new string('\n', gapPars);
                    segments[segments.Count - 1].IsParagraph = true;
                }

                int fontIndex = int.Parse(match.Groups["f"].Value);
                int fontSizeHalfPoints = match.Groups["fs"].Success ? int.Parse(match.Groups["fs"].Value) : 24;
                int colorIndex = int.Parse(match.Groups["cf"].Value);

                fontEntries.TryGetValue(fontIndex, out var fontName);
                AColor? colorL = null;
                int mappedIndex = colorIndex - colorOffset;
                if (mappedIndex >= 0 && mappedIndex < colorEntries.Count)
                {
                    colorL = colorEntries[mappedIndex];
                }

                bool bold = Regex.IsMatch(match.Value, @"\\b(?!0)");
                bool italic = Regex.IsMatch(match.Value, @"\\i(?!0)");
                bool underline = Regex.IsMatch(match.Value, @"\\ul(?!none|\d)");

                var rawText = match.Groups["text"].Value;
                bool isParagraph = rawText.Contains("\\par");

                var alignMatch = Regex.Match(rawText, @"\\q(l|r|j|c)\b");
                AbstTextAlignment alignment = defaultAlignment;
                if (alignMatch.Success)
                {
                    alignment = alignMatch.Groups[1].Value switch
                    {
                        "l" => AbstTextAlignment.Left,
                        "r" => AbstTextAlignment.Right,
                        "c" => AbstTextAlignment.Center,
                        "j" => AbstTextAlignment.Justified,
                        _ => AbstTextAlignment.Left
                    };
                    rawText = rawText.Replace(alignMatch.Value, string.Empty);
                }

                var liMatch = Regex.Match(rawText, @"\\li(-?\d+)");
                int marginLeft = defaultMarginLeft;
                if (liMatch.Success)
                {
                    marginLeft = int.Parse(liMatch.Groups[1].Value) / 20;
                    rawText = rawText.Replace(liMatch.Value, string.Empty);
                }
                var riMatch = Regex.Match(rawText, @"\\ri(-?\d+)");
                int marginRight = defaultMarginRight;
                if (riMatch.Success)
                {
                    marginRight = int.Parse(riMatch.Groups[1].Value) / 20;
                    rawText = rawText.Replace(riMatch.Value, string.Empty);
                }

                var expndMatch = Regex.Match(rawText, @"\\expnd(-?\d+)");
                if (expndMatch.Success)
                    rawText = rawText.Replace(expndMatch.Value, string.Empty);
                var slMatch = Regex.Match(rawText, @"\\sl(-?\d+)");
                int lineHeight = 0;
                if (slMatch.Success)
                {
                    lineHeight = int.Parse(slMatch.Groups[1].Value) / 20;
                    rawText = rawText.Replace(slMatch.Value, string.Empty);
                }

                rawText = Regex.Replace(rawText, @"\\tx-?\d*", string.Empty);

                var textContent = Regex.Replace(rawText, @"\\'([0-9a-fA-F]{2})", m => ((char)System.Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
                textContent = Regex.Replace(textContent, @"\\u(-?\d+)\??", m => ((char)int.Parse(m.Groups[1].Value)).ToString());
                textContent = textContent.Replace("\\par", "\n").Replace("\\tab", "\t").Replace("\\\\", "\\");
                textContent = Regex.Replace(textContent, @"\\([{}:])", "$1");
                if (isParagraph && string.IsNullOrEmpty(textContent))
                    textContent = "\n";
                if (textContent.StartsWith(" "))
                    textContent = textContent.Substring(1);

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
                    LineHeight = lineHeight,
                    StyleId = styleId,
                    IsParagraph = isParagraph
                });
                lastIndex = match.Index + match.Length;
            }

            var tailGap = rtfContent.Substring(lastIndex);
            int tailPars = Regex.Matches(tailGap, @"\\par").Count;
            if (tailPars > 0 && segments.Count > 0)
            {
                segments[segments.Count - 1].Text += new string('\n', tailPars);
                segments[segments.Count - 1].IsParagraph = true;
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

        private static List<AColor> ParseColorTable(string rtfContent, out int baseIndex)
        {
            var colorTableMatch = Regex.Match(rtfContent, @"\\colortbl(?<colortbl>[^}]+)}");
            var table = colorTableMatch.Groups["colortbl"].Value;
            baseIndex = table.StartsWith(";") ? 1 : 0;
            return Regex.Matches(table, @"\\red(?<r>\d+)\\green(?<g>\d+)\s*\\blue(?<b>\d+);")
                .Cast<Match>()
                .Select(m => new AColor(-1, byte.Parse(m.Groups["r"].Value), byte.Parse(m.Groups["g"].Value), byte.Parse(m.Groups["b"].Value)))
                .ToList();
        }

        private static Dictionary<string, StyleDef> ParseStyles(string rtfContent, Dictionary<int, string> fontEntries, List<AColor> colorEntries, int colorOffset)
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
                bool hasFont = false, hasSize = false, hasColor = false, hasAlignment = false;

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
                    int idx = int.Parse(cfMatch.Groups[1].Value) - colorOffset;
                    if (idx >= 0 && idx < colorEntries.Count)
                    {
                        style.Color = colorEntries[idx];
                        hasColor = true;
                    }
                }

                var slMatch = Regex.Match(def, @"\\sl(-?\d+)");
                if (slMatch.Success)
                    style.LineHeight = int.Parse(slMatch.Groups[1].Value) / 20;

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
                    hasAlignment = true;
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

                styles[id] = new StyleDef(style, hasFont, hasSize, hasColor, hasAlignment);
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
