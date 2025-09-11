using AbstUI.Texts;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Texts.FrameworkCommunication;
using AbstUI.Components;
using System.Collections.Generic;
using System.Text;

namespace LingoEngine.Texts
{
    /// <summary>
    /// Lingo Member Text Base Interal interface.
    /// </summary>
    public interface ILingoMemberTextBaseInteral : ILingoMemberTextBase
    {
    }
    public abstract class LingoMemberTextBase<TFrameworkType> : LingoMember, ILingoMemberTextBase, ILingoMemberTextBaseInteral
        where TFrameworkType : ILingoFrameworkMemberTextBase
    {
        private readonly IAbstComponentFactory _componentFactory;
        protected readonly TFrameworkType _frameworkMember;
        protected int _selectionStart;
        protected int _selectionEnd;
        protected string _selectedText = "";
        protected string _markdown = "";
        protected AbstMarkdownData? _mdData;

        protected LingoLines _Line;
        private AbstMarkdownRenderer _markDownRenderer;
        protected LingoWords _word = new LingoWords("");
        protected LingoChars _char = new LingoChars();
        protected LingoParagraphs _Paragraph = new LingoParagraphs();
        private string _paragraphSourceText = string.Empty;
        private bool _paragraphParsed;
        private IAbstTexture2D? _texture;
        private bool _isRendering;
        private AbstTextStyle _mdStyle = new AbstTextStyle();

        private bool _hasLoadedTexTure;
        private IAbstUITextureUserSubscription? _textureUser;

        public T Framework<T>() where T : class, TFrameworkType => (T)_frameworkMember;

        public bool TextChanged { get; private set; }

        #region Properties

        /// <inheritdoc/>
        public string Text
        {
            get => _frameworkMember.Text;
            set
            {
                _mdData = null;
                var newValue = value.Replace("\r\n","\r");
                _markdown = newValue;
                UpdateText(newValue);
                _frameworkMember.Text = newValue;
                Height = 0; // force re-render
            }
        }


        /// <inheritdoc/>
        public int ScrollTop
        {
            get => _frameworkMember.ScrollTop;
            set { _frameworkMember.ScrollTop = value; }
        }
        /// <inheritdoc/>
        public bool Editable { get; set; }
        /// <inheritdoc/>
        public bool WordWrap
        {
            get => _frameworkMember.WordWrap;
            set { 
                if (_frameworkMember.WordWrap == value) return;
                _frameworkMember.WordWrap = value;
                UpdateMDStyle();
            }
        }
        /// <inheritdoc/>
        public string Font
        {
            get => _frameworkMember.FontName;
            set { 
                if (_frameworkMember.FontName == value) return;
                _frameworkMember.FontName = value;
                UpdateMDStyle();
            }
        }
        /// <inheritdoc/>
        public int FontSize
        {
            get => _frameworkMember.FontSize;
            set {
                if (_frameworkMember.FontSize == value) return;
                _frameworkMember.FontSize = value; 
                UpdateMDStyle();
            }
        }

        /// <inheritdoc/>
        public AColor Color
        {
            get => _frameworkMember.TextColor;
            set { 
                if (_frameworkMember.TextColor == value) return;
                _frameworkMember.TextColor = value;
                UpdateMDStyle();
            }
        }
        /// <inheritdoc/>
        public LingoTextStyle FontStyle
        {
            get => _frameworkMember.FontStyle;
            set {
                if (_frameworkMember.FontStyle == value) return;
                _frameworkMember.FontStyle = value;
                UpdateMDStyle();
            }
        }
        /// <inheritdoc/>
        public bool Bold
        {
            get => (_frameworkMember.FontStyle & LingoTextStyle.Bold) != 0;
            set
            {
                var style = _frameworkMember.FontStyle;
                if (value)
                    style |= LingoTextStyle.Bold;
                else
                    style &= ~LingoTextStyle.Bold;
                _frameworkMember.FontStyle = style;
                UpdateMDStyle();
            }
        }
        /// <inheritdoc/>
        public bool Italic
        {
            get => (_frameworkMember.FontStyle & LingoTextStyle.Italic) != 0;
            set
            {
                var style = _frameworkMember.FontStyle;
                if (value)
                    style |= LingoTextStyle.Italic;
                else
                    style &= ~LingoTextStyle.Italic;
                _frameworkMember.FontStyle = style;
                UpdateMDStyle();
            }
        }
        /// <inheritdoc/>
        public bool Underline
        {
            get => (_frameworkMember.FontStyle & LingoTextStyle.Underline) != 0;
            set
            {
                var style = _frameworkMember.FontStyle;
                if (value)
                    style |= LingoTextStyle.Underline;
                else
                    style &= ~LingoTextStyle.Underline;
                _frameworkMember.FontStyle = style;
                UpdateMDStyle();
            }
        }
        /// <inheritdoc/>
        public AbstTextAlignment Alignment
        {
            get => _frameworkMember.Alignment;
            set { 
                if (_frameworkMember.Alignment == value) return;
                _frameworkMember.Alignment = value;
                UpdateMDStyle();
            }
        }
        /// <inheritdoc/>
        public int Margin
        {
            get => _frameworkMember.Margin;
            set {
                if (_frameworkMember.Margin == value) return;
                _frameworkMember.Margin = value;
                UpdateMDStyle();
            }
        }

        /// <inheritdoc/>
        public LingoLines Line => _Line;
        /// <inheritdoc/>
        public LingoWords Word => _word;
        /// <inheritdoc/>
        public LingoParagraphs Paragraph
        {
            get
            {
                if (!_paragraphParsed)
                {
                    if (_mdData != null)
                    {
                        var paragraphs = new List<string>();
                        var sb = new StringBuilder();
                        foreach (var seg in _mdData.Segments)
                        {
                            sb.Append(seg.Text);
                            if (seg.IsParagraph)
                            {
                                paragraphs.Add(sb.ToString().TrimEnd('\n', '\r'));
                                sb.Clear();
                            }
                        }
                        if (sb.Length > 0)
                            paragraphs.Add(sb.ToString().TrimEnd('\n', '\r'));
                        _Paragraph.SetText(string.Join("\n", paragraphs));
                    }
                    else
                    {
                        _Paragraph.SetText(_paragraphSourceText);
                    }
                    _paragraphParsed = true;
                }
                return _Paragraph;
            }
        }
        /// <inheritdoc/>
        public LingoChars Char => _char;
        private int _width;
        public override int Width
        {
            get
            {
                if (!_hasLoadedTexTure && base.Width <= 0)
                    RenderText();
                return _width;
            }

            set
            {
                if (_width == value) return;
                _width = value;
                base.Width = value;
                _frameworkMember.Width = value;
                //if (_width>0 && !_hasLoadedTexTure)
                //    RenderText();
            }
        }
        private int _height;
        public override int Height
        {
            get
            {
                if (!_hasLoadedTexTure && _height <= 0)
                    RenderText();
                return _height;
            }

            set
            {
                if (_height == value) return;
                base.Height = value;
                _height = value;
                _frameworkMember.Height = value;

            }
        }

        public IAbstTexture2D? TextureLingo => _frameworkMember.TextureLingo;
        #endregion



        public LingoMemberTextBase(LingoMemberType type, LingoCast cast, TFrameworkType frameworkMember, int numberInCast, IAbstComponentFactory componentFactory, string name = "", string fileName = "", APoint regPoint = default)
            : base(frameworkMember, type, cast, numberInCast, name, fileName, regPoint)
        {
            _componentFactory = componentFactory;
            _frameworkMember = frameworkMember;
            _Line = new LingoLines(LineTextChanged);
            _markDownRenderer = new AbstMarkdownRenderer(_frameworkMember.FontManager);

        }

        public void InitDefaults()
        {
            FontSize = 12;
        }
        private void LineTextChanged() => Text = _Line.ToString();

        private void UpdateText(string text)
        {
            _char.SetText(text);
            _word.SetText(text);
            _Line.SetText(text);
            _paragraphSourceText = text;
            _paragraphParsed = false;
            TextChanged = true;
            HasChanged = true;
        }

        public override void ChangesHasBeenApplied()
        {
            TextChanged = false;
            base.ChangesHasBeenApplied();
        }

        public void RequireRedraw()
        {
            _hasLoadedTexTure = false;
        }
        public IAbstTexture2D? GetTexture()
        {
            RenderText();
            return _texture;
        }
        public override void Preload()
        {
            base.Preload();
            RenderText();
        }
        /// <inheritdoc/>
        public void SetTextMD(AbstMarkdownData data)
        {
            _mdData = data;
            _markdown = data.Markdown;
            UpdateText(data.PlainText);
            _frameworkMember.Text = data.PlainText;
        }
        

        private void RenderText()
        {
            if (_isRendering || _hasLoadedTexTure) return;
            _isRendering = true;
            if (_texture != null)
            {
                _textureUser?.Release();
            }

            _markDownRenderer.Reset();
            if (_mdData != null)
            {
                _markDownRenderer.SetText(_mdData);
                if (_mdData.Styles.Count > 0)
                    UpdateMDStyle(_mdData.Styles.First());
            }
            else
            {
                UpdateMDStyle();
                _markDownRenderer.SetText(_markdown.Replace('\r', '\n'), [_mdStyle]);
            }
            var painter = _componentFactory.CreateImagePainterToTexture(Width, Height);
            if (Width == 10)
            {

            }
            painter.Width = Width;
            painter.AutoResizeWidth = Width == 0;
            painter.AutoResizeHeight = true; // Width > 0
            _markDownRenderer.Render(painter, new APoint(0, 0));
            _texture = painter.GetTexture("Text_" + Name);
            if (Width <= 0) Width = _texture.Width;
            Height = _texture.Height;
            _hasLoadedTexTure = true;
            _textureUser = _texture.AddUser(this);
            _isRendering = false;
        }

        private bool _skipStyleUpdate = false;
        private void UpdateMDStyle(KeyValuePair<string, AbstTextStyle> style1)
        {
            _skipStyleUpdate = true;
            FontStyle = LingoTextStyle.None;
            if (style1.Value.Bold) FontStyle |= LingoTextStyle.Bold;
            if (style1.Value.Italic) FontStyle |= LingoTextStyle.Italic;
            if (style1.Value.Underline) FontStyle |= LingoTextStyle.Underline;
            Font = style1.Value.Font;
            FontSize = style1.Value.FontSize;
            Color = style1.Value.Color;
            Alignment = style1.Value.Alignment;
            _skipStyleUpdate = false;
            UpdateMDStyle();
        }

        private void UpdateMDStyle()
        {
            if (_skipStyleUpdate) return;
            _mdStyle.Bold = (FontStyle & LingoTextStyle.Bold) != 0;
            _mdStyle.Italic = (FontStyle & LingoTextStyle.Italic) != 0;
            _mdStyle.Underline = (FontStyle & LingoTextStyle.Underline) != 0;
            _mdStyle.Font = Font ?? "";
            _mdStyle.FontSize = FontSize;
            _mdStyle.Color = Color;
            _mdStyle.Alignment = Alignment;
        }

        public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
        {
            RenderText();
            if (_texture != null)
                return _texture;
            return _frameworkMember.RenderToTexture(ink, transparentColor);
        }
        //        public void ApplyMemberChanges()
        //        {
        //#if DEBUG
        //            if (Name.Contains("scoor"))
        //            {
        //                _frameworkMember.ApplyMemberChanges();
        //            }
        //#endif
        //        }

        public virtual void Clear()
        {
            Text = "";
        }

        public virtual void Copy()
        {
            if (_selectionStart > 0 && _selectionEnd >= _selectionStart)
                _frameworkMember.Copy(_selectedText);
        }

        public virtual void Cut()
        {
            if (_selectionStart > 0 && _selectionEnd >= _selectionStart)
            {
                _frameworkMember.Copy(_selectedText);
                Text = Text.Remove(_selectionStart - 1, _selectionEnd - _selectionStart);
                _selectedText = "";
                _selectionEnd = _selectionStart;
            }
        }

        public virtual void Paste()
        {
            var pasteText = _frameworkMember.PasteClipboard();
            InsertText(pasteText);
        }

        public virtual void InsertText(string text)
        {
            var caret = _selectionEnd;
            Text = Text.Insert(caret - 1, text);
            _selectionStart = caret + text.Length;
            _selectionEnd = _selectionStart;
        }

        public virtual void ReplaceSelection(string replacement)
        {
            if (_selectionStart > 0 && _selectionEnd >= _selectionStart)
            {
                Text = Text.Remove(_selectionStart - 1, _selectionEnd - _selectionStart)
                           .Insert(_selectionStart - 1, replacement);
                _selectionStart += replacement.Length;
                _selectionEnd = _selectionStart;
            }
        }

        public virtual void SetSelection(int start, int end)
        {
            _selectionStart = start;
            _selectionEnd = end;
            _selectedText = Text.Substring(start - 1, end - start);
        }


    }

}

