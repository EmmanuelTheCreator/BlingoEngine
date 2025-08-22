using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using AbstUI.Styles;

namespace AbstUI.Texts
{
    /// <summary>
    /// Simple markdown renderer that draws on an <see cref="AbstGfxCanvas"/>.
    /// Supports headers (#), bold (**), italic (*), underline (__), images
    /// (optionally sized via <c>![alt](path size=100x50)</c>) and custom font/color
    /// tags wrapped in <c>{{...</c>}} such as <c>{{FONT-SIZE:20}}</c>,
    /// <c>{{FONT-FAMILY:Arial}}</c>, <c>{{COLOR:#FF0000}}</c>, alignment tags
    /// like <c>{{ALIGN:center}}</c> and paragraph style references
    /// <c>{{STYLE:MyStyle}}</c> that apply properties including line height
    /// and margins from predefined <see cref="AbstTextStyle"/> entries.
    /// </summary>
    public class AbstMarkdownRenderer
    {
        private AbstGfxCanvas? _canvas;
        private readonly IAbstFontManager _fontManager;
        private readonly Func<string, (byte[] data, int width, int height, APixelFormat format)>? _imageLoader;

        private string _markdown = string.Empty;

        private string _fontFamily = "Arial";
        private int _fontSize = 12;
        private AbstTextAlignment _alignment = AbstTextAlignment.Left;
        private AColor _color = AColors.Black;
        private int _lineHeight;
        private int _marginLeft;
        private int _marginRight;
        private bool _styleBold;
        private bool _styleItalic;
        private bool _styleUnderline;
        private readonly Stack<AbstTextStyle> _styleStack = new();

        private Dictionary<string, AbstTextStyle> _styles = new();

        /// <summary>Whether the renderer can use the optimized fast path.</summary>
        public bool DoFastRendering { get; private set; }

        /// <summary>Optional set of named styles that can be referenced with {{STYLE:name}} tags.</summary>
        public IEnumerable<AbstTextStyle> Styles
        {
            get => _styles.Values;
            private set => _styles = value?.ToDictionary(s => s.Name) ?? new();
        }

        public AbstMarkdownRenderer(
            IAbstFontManager fontManager,
            Func<string, (byte[] data, int width, int height, APixelFormat format)>? imageLoader = null)
        {
            _fontManager = fontManager;
            _imageLoader = imageLoader;
        }

        /// <summary>
        /// Sets the markdown text and available styles. Must be called before rendering.
        /// </summary>
        public void SetText(string markdown, IEnumerable<AbstTextStyle> styles)
        {
            _markdown = markdown ?? string.Empty;
            Styles = styles;
            _styleStack.Clear();
            if (_styles.Count > 0)
                ApplyStyle(_styles.Values.First());
            DoFastRendering = _styles.Count == 1 && !HasSpecialTags(_markdown);
        }

        /// <summary>Renders markdown text on the canvas starting from the given position.</summary>
        public void Render(AbstGfxCanvas canvas, APoint start)
        {
            _canvas = canvas;
            if (_styles.Count > 0)
                ApplyStyle(_styles.Values.First());
            _styleStack.Clear();

            if (DoFastRendering)
            {
                RenderFast(start);
                return;
            }

            var lines = _markdown.Split('\n');
            var pos = start;
            var firstLine = true;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                ApplyLeadingStyle(ref line);
                ProcessTags(ref line);

                // determine header level
                int headerLevel = 0;
                while (headerLevel < line.Length && line[headerLevel] == '#')
                    headerLevel++;
                var content = headerLevel > 0 ? line.Substring(headerLevel).TrimStart() : line;

                int usedFontSize = headerLevel > 0 ? 32 - (headerLevel - 1) * 4 : _fontSize;
                bool headerBold = headerLevel > 0;

                if (content.StartsWith("!["))
                {
                    var match = Regex.Match(content, @"!\[[^\]]*\]\(([^)\s]+)(?:\s+size=(\d+)x(\d+))?\)");
                    if (match.Success)
                    {
                        string path = match.Groups[1].Value;
                        int? w = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : null;
                        int? h = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : null;
                        int renderedHeight = RenderImage(path, pos, w, h);
                        pos.Offset(0, renderedHeight + 4);
                        continue;
                    }
                }

                var plain = StripFormatting(content);
                float lineWidth = EstimateWidth(plain, usedFontSize);
                float lineX = pos.X;
                if (_alignment == AbstTextAlignment.Center)
                    lineX -= lineWidth / 2f;
                else if (_alignment == AbstTextAlignment.Right)
                    lineX -= lineWidth;
                lineX += _marginLeft;
                if (_alignment == AbstTextAlignment.Right)
                    lineX -= _marginRight;
                else if (_alignment == AbstTextAlignment.Center)
                    lineX -= _marginRight / 2f;

                bool bold = headerBold || _styleBold;
                bool italic = _styleItalic;
                bool underline = _styleUnderline;

                var fontInfo = _fontManager.GetFontInfo(_fontFamily, usedFontSize);
                RenderInlineText(content, new APoint(lineX, pos.Y - (firstLine ? fontInfo.TopIndentation : 0)), usedFontSize, bold, italic, underline);
                int advance = _lineHeight > 0 ? _lineHeight : fontInfo.FontHeight + 4;
                pos.Offset(0, advance);
                firstLine = false;
            }
        }

        private void RenderFast(APoint start)
        {
            var style = _styles.Values.First();
            var lines = _markdown.Split('\n');
            var pos = start;
            var fontInfo = _fontManager.GetFontInfo(style.Font, style.FontSize);
            int lineHeight = style.LineHeight > 0 ? style.LineHeight : fontInfo.FontHeight + 4;
            bool firstLine = true;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                float lineWidth = EstimateWidth(line, style.FontSize);
                float lineX = pos.X;
                if (style.Alignment == AbstTextAlignment.Center)
                    lineX -= lineWidth / 2f;
                else if (style.Alignment == AbstTextAlignment.Right)
                    lineX -= lineWidth;
                lineX += style.MarginLeft;
                if (style.Alignment == AbstTextAlignment.Right)
                    lineX -= style.MarginRight;
                else if (style.Alignment == AbstTextAlignment.Center)
                    lineX -= style.MarginRight / 2f;

                _canvas!.DrawText(new APoint(lineX, pos.Y - (firstLine ? fontInfo.TopIndentation : 0)), line, style.Font, style.Color, style.FontSize, -1, AbstTextAlignment.Left);
                pos.Offset(0, lineHeight);
                firstLine = false;
            }
        }

        private void ApplyStyle(AbstTextStyle style)
        {
            _fontSize = style.FontSize;
            _fontFamily = style.Font;
            _color = style.Color;
            _alignment = style.Alignment;
            _styleBold = style.Bold;
            _styleItalic = style.Italic;
            _styleUnderline = style.Underline;
            _lineHeight = style.LineHeight;
            _marginLeft = style.MarginLeft;
            _marginRight = style.MarginRight;
        }

        private void ApplyLeadingStyle(ref string line)
        {
            while (true)
            {
                var trimmed = line.TrimStart();
                if (trimmed.StartsWith("{{STYLE:", StringComparison.Ordinal))
                {
                    int start = line.IndexOf("{{STYLE:", StringComparison.Ordinal);
                    int end = line.IndexOf("}}", start + 8, StringComparison.Ordinal);
                    if (end == -1)
                        break;
                    var name = line.Substring(start + 8, end - (start + 8)).Trim();
                    _styleStack.Push(new AbstTextStyle
                    {
                        FontSize = _fontSize,
                        Font = _fontFamily,
                        Color = _color,
                        Alignment = _alignment,
                        Bold = _styleBold,
                        Italic = _styleItalic,
                        Underline = _styleUnderline,
                        LineHeight = _lineHeight,
                        MarginLeft = _marginLeft,
                        MarginRight = _marginRight
                    });
                    if (_styles.TryGetValue(name, out var style))
                        ApplyStyle(style);
                    line = line.Remove(start, end - start + 2);
                }
                else if (trimmed.StartsWith("{{STYLE}}", StringComparison.Ordinal) || trimmed.StartsWith("{{/STYLE}}", StringComparison.Ordinal))
                {
                    int start = line.IndexOf("{{", StringComparison.Ordinal);
                    int end = line.IndexOf("}}", start + 2, StringComparison.Ordinal);
                    if (end == -1)
                        break;
                    if (_styleStack.Count > 0)
                        ApplyStyle(_styleStack.Pop());
                    line = line.Remove(start, end - start + 2);
                }
                else
                    break;
            }
        }

        private void ProcessTags(ref string line)
        {
            int index = 0;
            while (true)
            {
                int start = line.IndexOf("{{", index, StringComparison.Ordinal);
                if (start == -1)
                    break;
                int end = line.IndexOf("}}", start + 2, StringComparison.Ordinal);
                if (end == -1)
                    break;

                string tag = line.Substring(start + 2, end - start - 2);
                if (tag.StartsWith("STYLE", StringComparison.OrdinalIgnoreCase))
                {
                    index = end + 2;
                    continue;
                }
                ApplyTag(tag);
                line = line.Remove(start, end - start + 2);
            }
        }

        private void ApplyTag(string tag)
        {
            if (tag.StartsWith("FONT-SIZE:", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(tag.Substring(10), out var size))
                    _fontSize = size;
            }
            else if (tag.StartsWith("FONT-FAMILY:", StringComparison.OrdinalIgnoreCase))
            {
                _fontFamily = tag.Substring(12);
            }
            else if (tag.StartsWith("ALIGN:", StringComparison.OrdinalIgnoreCase))
            {
                var val = tag.Substring(6).Trim().ToLowerInvariant();
                _alignment = val switch
                {
                    "center" => AbstTextAlignment.Center,
                    "right" => AbstTextAlignment.Right,
                    "justify" or "justified" => AbstTextAlignment.Justified,
                    _ => AbstTextAlignment.Left,
                };
            }
            else if (tag.StartsWith("COLOR:", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    _color = AColor.FromHex(tag.Substring(6).Trim());
                }
                catch
                {
                    // ignore invalid color
                }
            }
        }

        private void RenderInlineText(string content, APoint pos, int fontSize, bool initialBold, bool initialItalic, bool initialUnderline)
        {
            int i = 0;
            bool bold = initialBold;
            bool italic = initialItalic;
            bool underline = initialUnderline;
            var sb = new StringBuilder();
            float currentX = pos.X;

            void Flush()
            {
                if (sb.Length == 0)
                    return;
                string font = _fontFamily;
                if (bold && italic)
                    font += " BoldItalic";
                else if (bold)
                    font += " Bold";
                else if (italic)
                    font += " Italic";

                string text = sb.ToString();
                float width = EstimateWidth(text, fontSize);
                _canvas!.DrawText(new APoint(currentX, pos.Y), text, font, _color, fontSize, -1, AbstTextAlignment.Left);
                if (underline)
                    _canvas!.DrawLine(new APoint(currentX, pos.Y + fontSize), new APoint(currentX + width, pos.Y + fontSize), _color, 1);
                currentX += width;
                sb.Clear();
            }

            while (i < content.Length)
            {
                if (content.IndexOf("{{STYLE:", i, StringComparison.Ordinal) == i)
                {
                    int end = content.IndexOf("}}", i + 8, StringComparison.Ordinal);
                    if (end != -1)
                    {
                        Flush();
                        var name = content.Substring(i + 8, end - (i + 8)).Trim();
                        _styleStack.Push(new AbstTextStyle
                        {
                            FontSize = _fontSize,
                            Font = _fontFamily,
                            Color = _color,
                            Alignment = _alignment,
                            Bold = bold,
                            Italic = italic,
                            Underline = underline,
                            LineHeight = _lineHeight,
                            MarginLeft = _marginLeft,
                            MarginRight = _marginRight
                        });
                        if (_styles.TryGetValue(name, out var style))
                            ApplyStyle(style);
                        bold = _styleBold;
                        italic = _styleItalic;
                        underline = _styleUnderline;
                        i = end + 2;
                        continue;
                    }
                }
                if (content.IndexOf("{{STYLE}}", i, StringComparison.Ordinal) == i || content.IndexOf("{{/STYLE}}", i, StringComparison.Ordinal) == i)
                {
                    Flush();
                    if (_styleStack.Count > 0)
                        ApplyStyle(_styleStack.Pop());
                    bold = _styleBold;
                    italic = _styleItalic;
                    underline = _styleUnderline;
                    i += content.IndexOf("{{STYLE}}", i, StringComparison.Ordinal) == i ? 9 : 10;
                    continue;
                }
                if (content.IndexOf("**", i, StringComparison.Ordinal) == i)
                {
                    Flush();
                    bold = !bold;
                    i += 2;
                    continue;
                }
                if (content.IndexOf("__", i, StringComparison.Ordinal) == i)
                {
                    Flush();
                    underline = !underline;
                    i += 2;
                    continue;
                }
                if (content[i] == '*')
                {
                    Flush();
                    italic = !italic;
                    i++;
                    continue;
                }
                sb.Append(content[i]);
                i++;
            }
            Flush();
        }

        private float EstimateWidth(string text, int fontSize)
            => _fontManager.MeasureTextWidth(text, _fontFamily, fontSize);

        private static string StripFormatting(string text)
        {
            text = Regex.Replace(text, @"!\[[^\]]*\]\([^)]+\)", string.Empty);
            text = text.Replace("**", string.Empty).Replace("*", string.Empty).Replace("__", string.Empty);
            return text;
        }

        private static bool HasSpecialTags(string text)
            => text.IndexOf("{{", StringComparison.Ordinal) >= 0
               || text.IndexOf("}}", StringComparison.Ordinal) >= 0
               || text.IndexOf("**", StringComparison.Ordinal) >= 0
               || text.IndexOf("__", StringComparison.Ordinal) >= 0
               || text.Contains('*')
               || text.IndexOf("![", StringComparison.Ordinal) >= 0
               || text.IndexOf('#') >= 0;

        private int RenderImage(string path, APoint position, int? widthOverride, int? heightOverride)
        {
            if (_imageLoader == null)
                return 0;
            var (data, width, height, format) = _imageLoader(path);
            int drawWidth = widthOverride ?? width;
            int drawHeight = heightOverride ?? height;
            _canvas!.DrawPicture(data, drawWidth, drawHeight, position, format);
            return drawHeight;
        }
    }
}
