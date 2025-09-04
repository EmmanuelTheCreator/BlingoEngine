using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
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
        private IAbstImagePainter? _canvas;
        private readonly IAbstFontManager _fontManager;
        private readonly Func<string, (byte[] data, int width, int height, APixelFormat format)>? _imageLoader;

        private string _markdown = string.Empty;

        private readonly AbstTextStyle _currentStyle = new() { Font = "Arial", FontSize = 12, Color = AColors.Black, Alignment = AbstTextAlignment.Left };
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

        /// <summary>
        /// Sets the markdown data including precomputed styles and segments.
        /// </summary>
        public void SetText(AbstMarkdownData data)
            => SetText(data.Markdown, data.Styles.Values);

        /// <summary>Renders markdown text on the canvas starting from the given position.</summary>
        public void Render(IAbstImagePainter canvas, APoint start)
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

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                ApplyLeadingStyle(ref line);
                if (_currentStyle.FontSize <= 0)
                    _currentStyle.FontSize = 12;

                // determine header level
                int headerLevel = 0;
                while (headerLevel < line.Length && line[headerLevel] == '#')
                    headerLevel++;
                var content = headerLevel > 0 ? line.Substring(headerLevel).TrimStart() : line;

                int usedFontSize = headerLevel > 0 ? 32 - (headerLevel - 1) * 4 : _currentStyle.FontSize;
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

                var segments = ParseInlineSegments(content, usedFontSize, headerBold || _currentStyle.Bold, _currentStyle.Italic, _currentStyle.Underline, out float lineWidth);

                if (segments.Count > 0)
                {
                    float lineX = pos.X;
                    if (_currentStyle.Alignment == AbstTextAlignment.Center)
                        lineX -= lineWidth / 2f;
                    else if (_currentStyle.Alignment == AbstTextAlignment.Right)
                        lineX -= lineWidth;
                    lineX += _currentStyle.MarginLeft;
                    if (_currentStyle.Alignment == AbstTextAlignment.Right)
                        lineX -= _currentStyle.MarginRight;
                    else if (_currentStyle.Alignment == AbstTextAlignment.Center)
                        lineX -= _currentStyle.MarginRight / 2f;

                    var firstSeg = segments[0];
                    var firstStyle = AbstFontStyle.Regular;
                    if (firstSeg.Bold) firstStyle |= AbstFontStyle.Bold;
                    if (firstSeg.Italic) firstStyle |= AbstFontStyle.Italic;
                    var fontInfo = _fontManager.GetFontInfo(firstSeg.FontFamily, firstSeg.FontSize, firstStyle);
                    RenderSegments(segments, new APoint(lineX, pos.Y - fontInfo.TopIndentation));

                    int lineHeight = _currentStyle.LineHeight > 0
                        ? _currentStyle.LineHeight
                        : segments.Max(s => _fontManager.GetFontInfo(s.FontFamily, s.FontSize, (s.Bold ? AbstFontStyle.Bold : AbstFontStyle.Regular) | (s.Italic ? AbstFontStyle.Italic : AbstFontStyle.Regular)).FontHeight);
                    pos.Offset(0, lineHeight);
                }
                else
                {
                    int lineHeight = _currentStyle.LineHeight > 0 ? _currentStyle.LineHeight : usedFontSize;
                    pos.Offset(0, lineHeight);
                }
            }
        }

        private void RenderFast(APoint start)
        {
            var style = _styles.Values.First();
            var lines = _markdown.Split('\n');
            var pos = start;
            var fontSize = style.FontSize;
            if (fontSize <= 0) fontSize = 12;
            var baseStyle = AbstFontStyle.Regular;
            if (style.Bold) baseStyle |= AbstFontStyle.Bold;
            if (style.Italic) baseStyle |= AbstFontStyle.Italic;
            var fontInfo = _fontManager.GetFontInfo(style.Font, fontSize, baseStyle);
            int lineHeight = style.LineHeight > 0 ? style.LineHeight : fontInfo.FontHeight;
            bool firstLine = true;

            // 1) measure max width of all lines
            float fullWidth = 0f;

            var lineWidths = new List<float>(lines.Length);
            foreach (var raw in lines)
            {
                var line = raw.TrimEnd('\r');
                var fontStyleForWidth = AbstFontStyle.Regular;
                if (style.Bold)
                    fontStyleForWidth |= AbstFontStyle.Bold;
                if (style.Italic)
                    fontStyleForWidth |= AbstFontStyle.Italic;
                var lineWidth = EstimateWidth(line, style.Font, fontSize, fontStyleForWidth);
                lineWidths.Add(lineWidth);
                fullWidth = MathF.Max(fullWidth, lineWidth);
            }
            // include margins in the box width, if you want the right edge to respect MarginRight:
            float contentWidth = MathF.Max(0, fullWidth);
            float originX = pos.X + style.MarginLeft;

            var lineIndex = 0;
            foreach (var raw in lines)
            {
                var line = raw.TrimEnd('\r');
                float lineW = lineWidths[lineIndex];

                // align within the measured content box (same right edge for every line)
                float xOff = 0f;
                switch (style.Alignment)
                {
                    case AbstTextAlignment.Center:
                        xOff = (contentWidth - lineW) * 0.5f;
                        break;
                    case AbstTextAlignment.Right:
                        xOff = (contentWidth - lineW);
                        break;
                    default:
                        xOff = 0f;
                        break;
                }

                float lineX = originX + xOff;
                var fontStyle = AbstFontStyle.Regular;
                if (style.Bold)
                    fontStyle |= AbstFontStyle.Bold;
                if (style.Italic)
                    fontStyle |= AbstFontStyle.Italic;
                _canvas!.DrawSingleLine(
                    new APoint(lineX, pos.Y - (firstLine ? fontInfo.TopIndentation : 0)),
                    line, style.Font, style.Color, style.FontSize, (int)MathF.Ceiling(lineW), fontInfo.FontHeight, style.Alignment, fontStyle);

                pos.Offset(0, lineHeight);
                firstLine = false;
                lineIndex++;
            }
        }

        private void ApplyStyle(AbstTextStyle style)
            => _currentStyle.CopyFrom(style);

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
                    _styleStack.Push(_currentStyle.Clone());
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

        private float EstimateWidth(string text, string fontFamily, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)
            => _fontManager.MeasureTextWidth(text, fontFamily, fontSize, style);

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

        private record TextSegment(string Text, string FontFamily, int FontSize, AColor Color, bool Bold, bool Italic, bool Underline);

        private List<TextSegment> ParseInlineSegments(string content, int initialFontSize, bool initialBold, bool initialItalic, bool initialUnderline, out float totalWidth)
        {
            int i = 0;
            var segments = new List<TextSegment>();
            var sb = new StringBuilder();
            float width = 0f;

            var style = _currentStyle.Clone();
            style.FontSize = initialFontSize;
            style.Bold = initialBold;
            style.Italic = initialItalic;
            style.Underline = initialUnderline;

            var localStack = new Stack<AbstTextStyle>(_styleStack.Select(s => s.Clone()).Reverse());

            void Flush()
            {
                if (sb.Length == 0) return;
                string text = sb.ToString();
                var styleFlags = AbstFontStyle.Regular;
                if (style.Bold) styleFlags |= AbstFontStyle.Bold;
                if (style.Italic) styleFlags |= AbstFontStyle.Italic;
                float segW = EstimateWidth(text, style.Font, style.FontSize, styleFlags);
                width += segW;
                segments.Add(new TextSegment(text, style.Font, style.FontSize, style.Color, style.Bold, style.Italic, style.Underline));
                sb.Clear();
            }

            while (i < content.Length)
            {
                if (content.IndexOf("{{", i, StringComparison.Ordinal) == i)
                {
                    int end = content.IndexOf("}}", i + 2, StringComparison.Ordinal);
                    if (end != -1)
                    {
                        Flush();
                        string tag = content.Substring(i + 2, end - i - 2);
                        if (tag.StartsWith("FONT-SIZE:", StringComparison.OrdinalIgnoreCase))
                        {
                            if (int.TryParse(tag.Substring(10), out var sz))
                                style.FontSize = sz;
                        }
                        else if (tag.StartsWith("FONT-FAMILY:", StringComparison.OrdinalIgnoreCase))
                        {
                            style.Font = tag.Substring(12);
                        }
                        else if (tag.StartsWith("COLOR:", StringComparison.OrdinalIgnoreCase))
                        {
                            try { style.Color = AColor.FromHex(tag.Substring(6).Trim()); } catch { }
                        }
                        else if (tag.StartsWith("ALIGN:", StringComparison.OrdinalIgnoreCase))
                        {
                            var val = tag.Substring(6).Trim().ToLowerInvariant();
                            style.Alignment = val switch
                            {
                                "center" => AbstTextAlignment.Center,
                                "right" => AbstTextAlignment.Right,
                                "justify" or "justified" => AbstTextAlignment.Justified,
                                _ => AbstTextAlignment.Left,
                            };
                        }
                        else if (tag.StartsWith("STYLE:", StringComparison.OrdinalIgnoreCase))
                        {
                            var name = tag.Substring(6).Trim();
                            localStack.Push(style.Clone());
                            if (_styles.TryGetValue(name, out var s))
                                style.CopyFrom(s);
                        }
                        else if (tag.Equals("STYLE", StringComparison.OrdinalIgnoreCase) || tag.Equals("/STYLE", StringComparison.OrdinalIgnoreCase))
                        {
                            if (localStack.Count > 0)
                                style.CopyFrom(localStack.Pop());
                        }
                        i = end + 2;
                        continue;
                    }
                }
                if (content.IndexOf("**", i, StringComparison.Ordinal) == i)
                {
                    Flush();
                    style.Bold = !style.Bold;
                    i += 2;
                    continue;
                }
                if (content.IndexOf("__", i, StringComparison.Ordinal) == i)
                {
                    Flush();
                    style.Underline = !style.Underline;
                    i += 2;
                    continue;
                }
                if (content[i] == '*')
                {
                    Flush();
                    style.Italic = !style.Italic;
                    i++;
                    continue;
                }
                sb.Append(content[i]);
                i++;
            }
            Flush();

            _currentStyle.CopyFrom(style);

            _styleStack.Clear();
            foreach (var s in localStack.Reverse())
                _styleStack.Push(s);

            totalWidth = width;
            return segments;
        }

        private void RenderSegments(List<TextSegment> segments, APoint topLeft)
        {
            float currentX = topLeft.X;
            var firstInfo = _fontManager.GetFontInfo(segments[0].FontFamily, segments[0].FontSize);
            float baselineY = topLeft.Y + firstInfo.TopIndentation;
            foreach (var seg in segments)
            {
                var segStyle = AbstFontStyle.Regular;
                if (seg.Bold) segStyle |= AbstFontStyle.Bold;
                if (seg.Italic) segStyle |= AbstFontStyle.Italic;
                float width = EstimateWidth(seg.Text, seg.FontFamily, seg.FontSize, segStyle);
                var fontInfo = _fontManager.GetFontInfo(seg.FontFamily, seg.FontSize, segStyle);
                float topY = baselineY - fontInfo.TopIndentation;
                var fontStyle = AbstFontStyle.Regular;
                if (seg.Bold)
                    fontStyle |= AbstFontStyle.Bold;
                if (seg.Italic)
                    fontStyle |= AbstFontStyle.Italic;
                _canvas!.DrawSingleLine(new APoint(currentX, topY), seg.Text, seg.FontFamily, seg.Color, seg.FontSize, (int)MathF.Ceiling(width), fontInfo.FontHeight, _currentStyle.Alignment, fontStyle);
                if (seg.Underline)
                    _canvas!.DrawLine(new APoint(currentX, topY + seg.FontSize), new APoint(currentX + width, topY + seg.FontSize), seg.Color, 1);
                currentX += width;
            }
        }
    }
}
