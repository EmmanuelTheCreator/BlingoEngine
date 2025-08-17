using System;
using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkInputText"/> using composition.
    /// </summary>
    public class AbstGodotInputText : IAbstFrameworkInputText, IDisposable
    {
        private readonly Action<string>? _onChange;
        private readonly IAbstFontManager _fontManager;
        private Control _control;
        private LineEdit? _lineEdit;
        private TextEdit? _textEdit;

        private string? _font;
        private AMargin _margin = AMargin.Zero;
        private event Action? _onValueChanged;

        private float _wantedWidth = 10;
        private float _wantedHeight = 10;
        private Color _fontColor = Colors.Black;
        private bool _isMultiLine;
        public object FrameworkNode => _control;

        public AbstGodotInputText(AbstInputText input, IAbstFontManager fontManager, Action<string>? onChange, bool multiLine = false)
        {
            _onChange = onChange;
            _fontManager = fontManager;
            IsMultiLine = multiLine;
            _control= InitControl(multiLine);

            input.Init(this);
        }

        private Control InitControl(bool multiLine)
        {
            if (_lineEdit != null)
                _lineEdit.TextChanged -= OnLineEditTextChanged;
            if (_textEdit != null)
                _textEdit.TextChanged -= OnTextEditTextChanged;
            if (_control != null)
                _control.Ready -= ControlReady;
            _lineEdit = null;
            _textEdit = null;
            if (multiLine)
            {
                _textEdit = new TextEdit();
                _control = _textEdit;
            }
            else
            {
                _lineEdit = new LineEdit();
                _control = _lineEdit;
            }

            _control.CustomMinimumSize = new Vector2(2, 2);
            _control.SizeFlagsHorizontal = 0;
            _control.SizeFlagsVertical = 0;

            if (_lineEdit != null)
                _lineEdit.TextChanged += OnLineEditTextChanged;
            if (_textEdit != null)
                _textEdit.TextChanged += OnTextEditTextChanged;

            _control.Ready += ControlReady;
            return _control;
        }

        private void ControlReady()
        {
            _control.CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
            _control.Size = new Vector2(_wantedWidth, _wantedHeight);
        }


        private void OnLineEditTextChanged(string _)
        {
            _text = _textEdit?.Text ?? _textEdit?.Text ?? string.Empty;
            _onValueChanged?.Invoke();
            _onChange?.Invoke(Text);
        }

        private void OnTextEditTextChanged()
        {
            _text = _textEdit?.Text ?? _textEdit?.Text ?? string.Empty;
            _onValueChanged?.Invoke();
            _onChange?.Invoke(Text);
        }

        public float X
        {
            get => _control.Position.X;
            set => _control.Position = new Vector2(value, _control.Position.Y);
        }

        public float Y
        {
            get => _control.Position.Y;
            set => _control.Position = new Vector2(_control.Position.X, value);
        }

        public float Width
        {
            get => _control.Size.X;
            set
            {
                _wantedWidth = value;
                _control.CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
                _control.Size = new Vector2(value, _wantedHeight);
            }
        }

        public float Height
        {
            get => _control.Size.Y;
            set
            {
                _wantedHeight = value;
                _control.CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
                _control.Size = new Vector2(_wantedWidth, value);
            }
        }

        public bool Visibility
        {
            get => _control.Visible;
            set => _control.Visible = value;
        }

        public bool Enabled
        {
            get => _lineEdit?.Editable ?? _textEdit?.Editable ?? true;
            set
            {
                if (_lineEdit != null) _lineEdit.Editable = value;
                if (_textEdit != null) _textEdit.Editable = value;
            }
        }

        private string _text;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                if (_lineEdit != null) _lineEdit.Text = value;
                if (_textEdit != null) _textEdit.Text = value;
            }
        }

        private int _maxLength;
        public int MaxLength
        {
            get => _maxLength;
            set
            {
                if (_lineEdit != null) _lineEdit.MaxLength = value;
                _maxLength = value;
            }
        }

        string IAbstFrameworkNode.Name
        {
            get => _control.Name;
            set => _control.Name = value;
        }

        public string? Font
        {
            get => _font;
            set
            {
                _font = value;
                if (string.IsNullOrEmpty(value))
                {
                    _control.RemoveThemeFontOverride("font");
                }
                else
                {
                    var font = _fontManager.Get<FontFile>(value);
                    if (font != null)
                        _control.AddThemeFontOverride("font", font);
                }
            }
        }

        private int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;

                Font? baseFont = string.IsNullOrEmpty(_font)
                    ? _fontManager.GetDefaultFont<Font>()
                    : _fontManager.Get<Font>(_font);
                if (baseFont == null)
                    return;

                var variation = new FontVariation
                {
                    BaseFont = baseFont
                };

                var theme = new Theme();
                var cls = _control.GetClass();
                theme.SetFont("font", cls, variation);
                theme.SetFontSize("font_size", cls, _fontSize);
                _control.Theme = theme;
            }
        }

      

        public Color FontColor
        {
            get => _fontColor;
            set
            {
                _fontColor = value;
                _control.AddThemeColorOverride("font_color", _fontColor);
            }
        }

        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                _control.AddThemeConstantOverride("margin_left", (int)_margin.Left);
                _control.AddThemeConstantOverride("margin_right", (int)_margin.Right);
                _control.AddThemeConstantOverride("margin_top", (int)_margin.Top);
                _control.AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }

       

        public bool IsMultiLine
        {
            get => _isMultiLine; set
            {
                if (_isMultiLine == value)
                    return;
                _isMultiLine = value;
                InitControl(_isMultiLine);
            }
        }

        event Action? IAbstFrameworkNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        public void Dispose()
        {
            if (_lineEdit != null)
                _lineEdit.TextChanged -= OnLineEditTextChanged;
            if (_textEdit != null)
                _textEdit.TextChanged -= OnTextEditTextChanged;

            _control.QueueFree();
        }
    }
}

