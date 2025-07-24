using Godot;
using LingoEngine.LGodot.Helpers;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Primitives;
using LingoEngine.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;
using System.Resources;

namespace LingoEngine.LGodot.Texts
{
    public abstract class LingoGodotMemberTextBase<TLingoText> : ILingoFrameworkMemberTextBase, IDisposable
         where TLingoText : ILingoMemberTextBase
    {
        protected TLingoText _lingoMemberText;
        protected string _text = "";
        protected ILingoFontManager _fontManager;
        protected readonly ILogger _logger;
        protected LabelSettings _LabelSettings = new LabelSettings();
        protected readonly Label _labelNode;
        protected readonly CenterContainer _parentNode;
        private bool _isCloning = false;

        
        public Node Node2D => _parentNode;


        #region Properties

        public string Text { get => _text; set{  UpdateText(value);IsLoaded = false;} }

        public bool WordWrap
        {
            get => _labelNode.AutowrapMode != TextServer.AutowrapMode.Off;
            set { _labelNode.AutowrapMode = value ? TextServer.AutowrapMode.Word : TextServer.AutowrapMode.Off; IsLoaded = false; }
        }

        public int ScrollTop
        {
            get => _labelNode.LinesSkipped;
            set { _labelNode.LinesSkipped = value; IsLoaded = false; }
        }

        private LingoTextStyle _textStyle = LingoTextStyle.None;

        public LingoTextStyle FontStyle
        {
            get => _textStyle;
            set
            {
                _textStyle = value;
                if (_textStyle != LingoTextStyle.None)
                {
                    // todo : implement a way for rtf

                    var rtl = new RichTextLabel();
                    // Bold/Italic handled via FontStyle
                    TextServer.FontStyle style = 0; // _LabelSettings.Font.GetFontStyle();

                    if (value.HasFlag(LingoTextStyle.Bold))
                        style |= TextServer.FontStyle.Bold;

                    if (value.HasFlag(LingoTextStyle.Italic))
                        style |= TextServer.FontStyle.Italic;
                    //_LabelSettings.Font.Styl();


                    //rtl.FontStyle = style;

                    //// Underline handled separately
                    //_LabelSettings.UnderlineMode = value.HasFlag(LingoTextStyle.Underline)
                    //    ? UnderlineMode.Always
                    //    : UnderlineMode.Disabled;
                    IsLoaded = false;
                }
                
                    UpdateSize();
            }
        }

        private int _margin = 0;
        public int Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                _labelNode.AddThemeConstantOverride("margin_left", value);
                _labelNode.AddThemeConstantOverride("margin_right", value);
                _labelNode.AddThemeConstantOverride("margin_top", value);
                _labelNode.AddThemeConstantOverride("margin_bottom", value);
                IsLoaded = false;
            }
        }
        public LingoTextAlignment Alignment
        {
            get
            {
                return _labelNode.HorizontalAlignment switch
                {
                    HorizontalAlignment.Left => LingoTextAlignment.Left,
                    HorizontalAlignment.Center => LingoTextAlignment.Center,
                    HorizontalAlignment.Right => LingoTextAlignment.Right,
                    _ => LingoTextAlignment.Left // Default fallback
                };
            }
            set
            {
                _labelNode.HorizontalAlignment = value switch
                {
                    LingoTextAlignment.Left => HorizontalAlignment.Left,
                    LingoTextAlignment.Center => HorizontalAlignment.Center,
                    LingoTextAlignment.Right => HorizontalAlignment.Right,
                    _ => HorizontalAlignment.Left // Default fallback
                };
                IsLoaded = false;
            }
        }

        private LingoColor _lingoColor = LingoColor.FromRGB(0, 0, 0);
        public LingoColor TextColor
        {
            get => _lingoColor; 
            set
            {
                _lingoColor = value;
                _LabelSettings.SetLingoColor(value);
                IsLoaded = false;
            }
        }
        public int FontSize
        {
            get => _LabelSettings.FontSize; 
            set
            {

                _LabelSettings.SetLingoFontSize(value);
                UpdateSize();
                IsLoaded = false;
            }
        }

        private string _fontName = "";
        public string FontName
        {
            get => _fontName;
            set
            {
                _fontName = value;
                _LabelSettings.SetLingoFont(_fontManager, value);
                UpdateSize();
                IsLoaded = false;
            }
        }


        public bool IsLoaded { get; private set; }
        public LingoPoint Size { get; private set; }
        private bool _widthSet;
        public int Width { get => _widthSet? (int)_labelNode.CustomMinimumSize.X: (int)Size.X; 
            set
            {
                _labelNode.CustomMinimumSize = new Vector2(value, _labelNode.CustomMinimumSize.Y);
                _widthSet = true;
            }
        }
        private bool _heightSet;
        public int Height { get => _heightSet? (int)_labelNode.CustomMinimumSize.Y :(int)Size.Y;
            set
            {
                _labelNode.CustomMinimumSize = new Vector2(_labelNode.CustomMinimumSize.X, value);
                _heightSet = true;
            }
        }
        #endregion


        protected Node CloneForSpriteDraw(LingoGodotMemberTextBase<TLingoText> copiedNode)
        {
            _isCloning = true;
            copiedNode._isCloning = true;

            copiedNode._lingoMemberText = _lingoMemberText;
            // Parse properties
            copiedNode.WordWrap = WordWrap;
            copiedNode.ScrollTop = ScrollTop;
            copiedNode.FontStyle = FontStyle;
            copiedNode.Margin = Margin;
            copiedNode.Alignment = Alignment;
            copiedNode.TextColor = TextColor;
            copiedNode.FontSize = FontSize;
            copiedNode.FontName = FontName;
            copiedNode.Width = Width;
            copiedNode.Height = Height;

            _isCloning = false;
            copiedNode._isCloning = false;
            // Set latest
            copiedNode.Text = Text;
            return copiedNode.Node2D;
        }



#pragma warning disable CS8618
        public LingoGodotMemberTextBase(ILingoFontManager lingoFontManager, ILogger logger)
#pragma warning restore CS8618
        {
            _fontManager = lingoFontManager;
            _logger = logger;
            _parentNode = new CenterContainer();
            _labelNode = new Label();
            _parentNode.AddChild(_labelNode);
            //var labelSettings = new LabelSettings
            //{
            //    Font = _fontManager.Get<FontFile>("Earth"),
            //    FontColor = new Color(1, 0, 0),
            //    FontSize = 40,
            //};
            // these are needed in the styling:
            //theme.SetConstant("minimum_height", controlType, 10);
            //theme.SetConstant("minimum_width", controlType, 5);
            //theme.SetConstant("minimum_spaces", controlType, 1);
            //theme.SetConstant("minimum_character_width", controlType, 0);
            _labelNode.LabelSettings = _LabelSettings;
        }
        public void Dispose()
        {
            _labelNode.Dispose();
            _parentNode.Dispose();
        }

        internal void Init(TLingoText lingoInstance)
        {
            _lingoMemberText = lingoInstance;
            if (!string.IsNullOrWhiteSpace(lingoInstance.Name))
                _parentNode.Name = lingoInstance.Name;
        }

        public string ReadText()
        {
            var file = GodotHelper.ReadFile(_lingoMemberText.FileName);
            if (file == null)
            {
                _logger.LogWarning("File not found for Text :" + _lingoMemberText.FileName);
                return "";
            }
            return file;
        }
        public string ReadTextRtf()
        {
            var rtfVersion = GodotHelper.ReadFile(_lingoMemberText.FileName.Replace(".txt", ".rtf"));
            if (rtfVersion == null)
                return "";
            return rtfVersion;
        }
        private void UpdateText(string value)
        {
            if (_text == value) return;
            _text = value;
            _labelNode.Text = value;
            UpdateSize();
            
        }

        
        private void UpdateSize()
        {
            
            if (_isCloning) return;
            Size = _labelNode != null? _labelNode.GetCombinedMinimumSize().ToLingoPoint() : (_LabelSettings.Font ?? _fontManager.GetDefaultFont<Font>()).GetMultilineStringSize(Text).ToLingoPoint();
            _lingoMemberText.Width = (int)Size.X;
            _lingoMemberText.Height = (int)Size.Y;
        }

        public void Erase()
        {
            Unload();
            IsLoaded = false;
        }


        public void Preload()
        {
            IsLoaded = true;
        }

        public void Unload()
        {
            IsLoaded = false;
        }



        #region Clipboard
        public void ImportFileInto()
        {
        }

        public void CopyToClipboard()
        {
            DisplayServer.ClipboardSet(Text);
        }
        public void PasteClipboardInto()
        {
            var _RAWTextData = DisplayServer.ClipboardGet();
            _lingoMemberText.Text = _RAWTextData;
        }
        public void Copy(string text) => DisplayServer.ClipboardSet(text);
        public string PasteClipboard() => DisplayServer.ClipboardGet();
        #endregion
    }
}
