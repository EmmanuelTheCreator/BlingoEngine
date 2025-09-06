using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using AbstUI.Components.Containers;
using AbstUI.Components.Graphics;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Texts;
using AbstUI.Styles;
using AbstUI.Windowing;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Texts;

namespace LingoEngine.Director.Core.Texts
{
    public class DirectorTextEditWindowV2 : DirectorWindow<IDirFrameworkTextEditWindow>
    {
        private readonly AbstInputText _markdownInput;
        private readonly AbstGfxCanvas _previewCanvas;
        private readonly AbstMarkdownRenderer _renderer;
        private readonly AbstWrapPanel _rootPanel;
        private CancellationTokenSource? _renderCts;
        private IAbstTexture2D? _previewTexture;
        private SynchronizationContext? _uiContext;
        private ILingoMemberTextBase? _member;

        public TextEditIconBar IconBar { get; }
        public AbstTextStyle CurrentStyle => IconBar.CurrentStyle;
        public AbstWrapPanel RootPanel => _rootPanel;

        public DirectorTextEditWindowV2(ILingoFrameworkFactory factory, IServiceProvider serviceProvider) : base(serviceProvider, DirectorMenuCodes.TextEditWindow)
        {
            IconBar = new TextEditIconBar(factory);
            var fontManager = factory.ComponentFactory.GetRequiredService<IAbstFontManager>();
            _renderer = new AbstMarkdownRenderer(fontManager);
            IconBar.SetFonts(fontManager.GetAllNames());
            _rootPanel = factory.CreateWrapPanel(AOrientation.Vertical, "TextEditWindowV2Root");
            _rootPanel.AddItem(IconBar.Panel);

            var columns = factory.CreateWrapPanel(AOrientation.Horizontal, "TextEditWindowV2Columns");
            _rootPanel.AddItem(columns);

            _markdownInput = factory.CreateInputText("MarkdownInput", onChange: OnTextChanged, multiLine: true);
            _markdownInput.Width = 400;
            _markdownInput.Height = 400;
            columns.AddItem(_markdownInput);

            _previewCanvas = factory.CreateGfxCanvas("MarkdownPreview", 400, 400);
            _previewCanvas.Width = 400;
            _previewCanvas.Height = 400;
            columns.AddItem(_previewCanvas);

            IconBar.BoldChanged += v =>
            {
                if (v)
                {
                    InsertAroundSelection("**", "**");
                    IconBar.SetBold(false);
                }
            };
            IconBar.ItalicChanged += v =>
            {
                if (v)
                {
                    InsertAroundSelection("*", "*");
                    IconBar.SetItalic(false);
                }
            };
            IconBar.UnderlineChanged += v =>
            {
                if (v)
                {
                    InsertAroundSelection("__", "__");
                    IconBar.SetUnderline(false);
                }
            };
            IconBar.AlignmentChanged += a =>
            {
                var style = EnsureParagraphStyle();
                style.Alignment = a;
                SyncMemberText();
                ScheduleRender(_markdownInput.Text);
            };
            IconBar.FontSizeChanged += v =>
            {
                var style = EnsureParagraphStyle();
                style.FontSize = v;
                SyncMemberText();
                ScheduleRender(_markdownInput.Text);
            };
            IconBar.FontChanged += n =>
            {
                if (string.IsNullOrEmpty(n)) return;
                var style = EnsureParagraphStyle();
                style.Font = n;
                SyncMemberText();
                ScheduleRender(_markdownInput.Text);
            };
            IconBar.ColorChanged += c =>
            {
                if (_markdownInput.HasSelection)
                {
                    var styleName = IconBar.CreateStyle(s => s.Color = c);
                    InsertAroundSelection($"{{{{STYLE:{styleName}}}}}", "{{/STYLE}}");
                }
                else
                {
                    var style = EnsureParagraphStyle();
                    style.Color = c;
                    SyncMemberText();
                    ScheduleRender(_markdownInput.Text);
                }
            };

            Width = 820;
            Height = 480;
            MinimumWidth = 400;
            MinimumHeight = 460;
            X = 1000;
            Y = 700;
        }

        protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
        {
            base.OnInit(frameworkWindow);
            _uiContext = SynchronizationContext.Current;
            Content = _rootPanel;
        }

        protected override void OnDispose()
        {
            _renderCts?.Cancel();
            _renderCts?.Dispose();
            _previewTexture?.Dispose();
            base.OnDispose();
        }

        protected override void OnResizing(bool firstLoad, int width, int height)
        {
            base.OnResizing(firstLoad, width, height);
            IconBar.OnResizing(width, height);
            _rootPanel.Width = width;
            _rootPanel.Height = height;
            var contentHeight = height - IconBar.Panel.Height;
            _markdownInput.Width = width / 2;
            _markdownInput.Height = contentHeight;
            _previewCanvas.Width = width / 2;
            _previewCanvas.Height = contentHeight;
        }

        public void SetMemberValues(ILingoMemberTextBase textMember)
        {
            _member = textMember;
            _markdownInput.Text = textMember.Text;
            IconBar.SetMemberValues(textMember);
            RenderMarkdown(_markdownInput.Text);
        }

        private void OnTextChanged(string text)
        {
            if (_member != null)
                _member.Text = text;
            ScheduleRender(text);
        }

        private void SyncMemberText()
        {
            if (_member != null)
                _member.Text = _markdownInput.Text;
        }

        private void ScheduleRender(string text)
        {
            _renderCts?.Cancel();
            var cts = new CancellationTokenSource();
            _renderCts = cts;
            Task.Delay(250, cts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    if (_uiContext != null)
                        _uiContext.Post(_ => RenderMarkdown(text), null);
                    else
                        RenderMarkdown(text);
                }
            }, TaskScheduler.Default);
        }

        private void RenderMarkdown(string text)
        {
            _previewTexture?.Dispose();
            using var painter = _factory.ComponentFactory.CreateImagePainterToTexture((int)_previewCanvas.Width, (int)_previewCanvas.Height);
            painter.AutoResize = true;
            _renderer.Reset();
            _renderer.SetText(text, IconBar.Styles);
            _renderer.Render(painter, new APoint(0, 0));
            _previewTexture = painter.GetTexture("MarkdownPreview");
            _previewCanvas.Clear(AColors.White);
            if (_previewTexture != null)
                _previewCanvas.DrawPicture(_previewTexture, _previewTexture.Width, _previewTexture.Height, new APoint(0, 0));
        }

        private AbstTextStyle EnsureParagraphStyle()
        {
            int caret = _markdownInput.GetCaretPosition();
            var text = _markdownInput.Text;
            int lineStart = text.LastIndexOf('\n', Math.Max(0, caret - 1)) + 1;
            int lineEnd = text.IndexOf('\n', caret);
            if (lineEnd < 0) lineEnd = text.Length;

            var segment = text.Substring(lineStart, lineEnd - lineStart);
            var paraMatch = Regex.Match(segment, "^\\{\\{PARA:([^}]+)\\}\\}");
            string styleName;
            bool inserted = false;
            if (paraMatch.Success)
            {
                styleName = paraMatch.Groups[1].Value;
            }
            else
            {
                styleName = IconBar.CreateStyle();
                string tag = $"{{{{PARA:{styleName}}}}}";
                text = text.Insert(lineStart, tag);
                _markdownInput.Text = text;
                _markdownInput.SetCaretPosition(caret + tag.Length);
                SyncMemberText();
                inserted = true;
            }

            var style = IconBar.EnsureStyle(styleName);
            if (inserted)
                ScheduleRender(_markdownInput.Text);
            return style;
        }

        private void InsertAroundSelection(string prefix, string suffix)
        {
            var text = _markdownInput.Text;
            int end = _markdownInput.GetCaretPosition();
            if (_markdownInput.HasSelection)
            {
                _markdownInput.DeleteSelection();
                int start = _markdownInput.GetCaretPosition();
                var selected = text.Substring(start, end - start);
                _markdownInput.InsertText(prefix + selected + suffix);
                _markdownInput.SetCaretPosition(start + prefix.Length + selected.Length + suffix.Length);
            }
            else
            {
                _markdownInput.InsertText(prefix + suffix);
                _markdownInput.SetCaretPosition(end + prefix.Length);
            }
            SyncMemberText();
            ScheduleRender(_markdownInput.Text);
        }
    }
}
