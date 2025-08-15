using AbstUI.Texts;
using Godot;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.LGodot.Sprites;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using AbstUI.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;
using AbstUI.LGodot.Helpers;
using AbstUI.LGodot.Texts;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Primitives;

namespace LingoEngine.LGodot.Texts
{
    public abstract class LingoGodotMemberTextBase<TLingoText> : ILingoFrameworkMemberTextBase, IDisposable
         where TLingoText : ILingoMemberTextBase
    {
        protected TLingoText _lingoMemberText;
        protected string _text = "";
        protected IAbstFontManager _fontManager;
        protected readonly ILogger _logger;
        protected LabelSettings _LabelSettings = new LabelSettings();
        
        private GodotMemberTextNode _defaultTextNode;
        private List<GodotMemberTextNode> _usedNodes = new List<GodotMemberTextNode>();
        private LingoGodotTexture2D? _texture;
        public IAbstUITexture2D? TextureLingo => _texture;
        public LingoGodotTexture2D? TextureGodot => _texture;

        internal class GodotMemberTextNode
        {
            protected readonly CenterContainer _parentNode;
            protected readonly Label _labelNode;
            private readonly int _index;

            public Label LabelNode => _labelNode;
            public Node Node2D => _parentNode;

            public GodotMemberTextNode(int index, LabelSettings labelSettings)
            {
                _parentNode = new CenterContainer();
                _labelNode = new Label();
                _labelNode.LabelSettings = labelSettings;
                _parentNode.AddChild(_labelNode);
                _index = index;
            }
            public void Dispose()
            {
                _labelNode.Dispose();
                _parentNode.Dispose();
            }

            internal void SetName(string name)
            {
                _parentNode.Name = name+ _index;
            }
        }


        


        #region Properties

        public string Text { get => _text; set{  UpdateText(value);} }

        public bool WordWrap
        {
            get => _defaultTextNode.LabelNode.AutowrapMode != TextServer.AutowrapMode.Off;
            set { Apply(x => x.LabelNode.AutowrapMode = value ? TextServer.AutowrapMode.Word : TextServer.AutowrapMode.Off);  }
        }

        public int ScrollTop
        {
            get => _defaultTextNode.LabelNode.LinesSkipped;
            set { Apply(x => x.LabelNode.LinesSkipped = value);  }
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
                Apply(x =>
                {
                    x.LabelNode.AddThemeConstantOverride("margin_left", value);
                    x.LabelNode.AddThemeConstantOverride("margin_right", value);
                    x.LabelNode.AddThemeConstantOverride("margin_top", value);
                    x.LabelNode.AddThemeConstantOverride("margin_bottom", value);
                });
                
            }
        }
        public AbstTextAlignment Alignment
        {
            get
            {
                return _defaultTextNode.LabelNode.HorizontalAlignment switch
                {
                    HorizontalAlignment.Left => AbstTextAlignment.Left,
                    HorizontalAlignment.Center => AbstTextAlignment.Center,
                    HorizontalAlignment.Right => AbstTextAlignment.Right,
                    _ => AbstTextAlignment.Left // Default fallback
                };
            }
            set
            {
                var align = value switch
                {
                    AbstTextAlignment.Left => HorizontalAlignment.Left,
                    AbstTextAlignment.Center => HorizontalAlignment.Center,
                    AbstTextAlignment.Right => HorizontalAlignment.Right,
                    _ => HorizontalAlignment.Left // Default fallback
                };
                Apply(x => x.LabelNode.HorizontalAlignment = align);
            }
        }

        private AColor _lingoColor = AColor.FromRGB(0, 0, 0);
        public AColor TextColor
        {
            get => _lingoColor; 
            set
            {
                _lingoColor = value;
                _LabelSettings.SetLingoColor(value);
                
            }
        }
        public int FontSize
        {
            get => _LabelSettings.FontSize; 
            set
            {

                _LabelSettings.SetLingoFontSize(value);
                UpdateSize();
                
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
                
            }
        }


        public bool IsLoaded { get; private set; }
        public APoint Size { get; private set; }
        private bool _widthSet;
        public int Width { get => _widthSet? (int)_defaultTextNode.LabelNode.CustomMinimumSize.X: (int)Size.X; 
            set
            {
                Apply(x => x.LabelNode.CustomMinimumSize = new Vector2(value, x.LabelNode.CustomMinimumSize.Y));
                _widthSet = true;
            }
        }
        private bool _heightSet;
        public int Height { get => _heightSet? (int)_defaultTextNode.LabelNode.CustomMinimumSize.Y :(int)Size.Y;
            set
            {
                Apply(x => x.LabelNode.CustomMinimumSize = new Vector2(x.LabelNode.CustomMinimumSize.X, value));
                _heightSet = true;
            }
        }
        #endregion


        protected Node CreateForSpriteDraw(LingoGodotMemberTextBase<TLingoText> copiedNode)
        {
            var newNode = new GodotMemberTextNode(_usedNodes.Count+1, _LabelSettings);
            _usedNodes.Add(newNode);
            return newNode.Node2D;
        }
        public void ReleaseFromSprite(LingoSprite2D lingoSprite)
        {
            if (lingoSprite.Member == null) return;
            var godotNode = lingoSprite.Framework<LingoGodotSprite2D>().ChildMemberNode;
            if (godotNode == null) return;
            var nodeLocal = _usedNodes.FirstOrDefault(x => x.Node2D == godotNode);
            if (nodeLocal != null)
                _usedNodes.Remove(nodeLocal);
        }


#pragma warning disable CS8618
        public LingoGodotMemberTextBase(IAbstFontManager lingoFontManager, ILogger logger)
#pragma warning restore CS8618
        {
            _fontManager = lingoFontManager;
            _logger = logger;
            _defaultTextNode = new GodotMemberTextNode(1, _LabelSettings);
            _usedNodes.Add(_defaultTextNode);
        }
        public void Dispose()
        {
            foreach (var usedNode in _usedNodes)
                usedNode.Dispose();
            _usedNodes.Clear();
        }

        internal void Init(TLingoText lingoInstance)
        {
            _lingoMemberText = lingoInstance;
            if (!string.IsNullOrWhiteSpace(lingoInstance.Name))
            {
                _defaultTextNode.SetName(lingoInstance.Name);
                foreach (var usedNode in _usedNodes)
                    usedNode.SetName(lingoInstance.Name);
            }
        }
        public IAbstUITexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
        {
            int w = Width > 0 ? Width : (int)Size.X;
            int h = Height > 0 ? Height : (int)Size.Y;
            if (w <= 0 || h <= 0)
                w = h = 1;
            var viewport = new SubViewport
            {
                Size = new Vector2I(w, h),
                TransparentBg = true,
                RenderTargetUpdateMode = SubViewport.UpdateMode.Once
            };
            var label = new Label
            {
                LabelSettings = _LabelSettings,
                Text = Text,
                Size = new Vector2(w, h)
            };
            label.AutowrapMode = WordWrap ? TextServer.AutowrapMode.Word : TextServer.AutowrapMode.Off;
            label.HorizontalAlignment = Alignment switch
            {
                AbstTextAlignment.Center => HorizontalAlignment.Center,
                AbstTextAlignment.Right => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Left
            };
            viewport.AddChild(label);
            var img = viewport.GetTexture().GetImage();
            viewport.Dispose();
            var tex = ImageTexture.CreateFromImage(img);
            _texture =  new LingoGodotTexture2D(tex);
            return _texture;
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
            foreach (var item in _usedNodes)
                item.LabelNode.Text = value;
            UpdateSize();
            
        }
        private void Apply(Action<GodotMemberTextNode> action)
        {
            foreach (var item in _usedNodes)
                action(item);
        }

        private void UpdateSize()
        {
            Size = _defaultTextNode != null? _defaultTextNode.LabelNode.GetCombinedMinimumSize().ToLingoPoint() : (_LabelSettings.Font ?? _fontManager.GetDefaultFont<Font>()).GetMultilineStringSize(Text).ToLingoPoint();
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
