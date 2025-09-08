using System;
using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.LGodot.Primitives;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components.Inputs
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkInputText"/> using composition.
    /// </summary>
    public class AbstGodotInputText : IAbstFrameworkInputText, IHasTextBackgroundBorderColor, IDisposable, IFrameworkFor<AbstInputText>
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
        private AColor _textColor = AColors.Black;
        private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
        private AColor _borderColor = AbstDefaultColors.InputBorderColor;
        private bool _isMultiLine;
        public object FrameworkNode => _control;

        public AbstGodotInputText(AbstInputText input, IAbstFontManager fontManager, Action<string>? onChange, bool multiLine = false)
        {
            _onChange = onChange;
            _fontManager = fontManager;
            IsMultiLine = multiLine;
            _control = InitControl(multiLine);

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
            _text = _lineEdit?.Text ?? string.Empty;
            _onValueChanged?.Invoke();
            _onChange?.Invoke(Text);
        }

        private void OnTextEditTextChanged()
        {
            _text = _textEdit?.Text ?? string.Empty;
            _onValueChanged?.Invoke();
            _onChange?.Invoke(Text);
        }

        private (int line, int column) GetLineColumn(int index)
        {
            int line = 0;
            int column = 0;
            for (int i = 0; i < index && i < _text.Length; i++)
            {
                if (_text[i] == '\n')
                {
                    line++;
                    column = 0;
                }
                else
                {
                    column++;
                }
            }
            return (line, column);
        }

        private int GetIndexFromLineColumn(int line, int column)
        {
            int idx = 0;
            int currentLine = 0;
            while (idx < _text.Length && currentLine < line)
            {
                if (_text[idx++] == '\n')
                    currentLine++;
            }
            return Math.Min(idx + column, _text.Length);
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

        private string _text = string.Empty;

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



        public AColor TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                _control.AddThemeColorOverride("font_color", _textColor.ToGodotColor());
            }
        }

        public AColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                _control.AddThemeColorOverride("background_color", _backgroundColor.ToGodotColor());
            }
        }

        public AColor BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                _control.AddThemeColorOverride("border_color", _borderColor.ToGodotColor());
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

        public bool HasSelection
        {
            get
            {
                if (_lineEdit != null)
                    return _lineEdit.HasSelection();
                if (_textEdit != null)
                    return _textEdit.HasSelection();
                return false;
            }
        }

        public void DeleteSelection()
        {
            if (_lineEdit != null)
            {
                if (!_lineEdit.HasSelection()) return;
                int start = _lineEdit.GetSelectionFromColumn();
                int end = _lineEdit.GetSelectionToColumn();
                _text = _text.Remove(start, end - start);
                _lineEdit.Text = _text;
                _lineEdit.CaretColumn = start;
                _lineEdit.Deselect();
            }
            else if (_textEdit != null)
            {
                if (!_textEdit.HasSelection()) return;
                int startLine = _textEdit.GetSelectionFromLine();
                int startCol = _textEdit.GetSelectionFromColumn();
                int endLine = _textEdit.GetSelectionToLine();
                int endCol = _textEdit.GetSelectionToColumn();
                int start = GetIndexFromLineColumn(startLine, startCol);
                int end = GetIndexFromLineColumn(endLine, endCol);
                _text = _text.Remove(start, end - start);
                _textEdit.Text = _text;
                _textEdit.Call("set_caret_line", startLine);
                _textEdit.Call("set_caret_column", startCol);
                _textEdit.Call("deselect");
            }
            else
            {
                return;
            }

            _onValueChanged?.Invoke();
            _onChange?.Invoke(Text);
        }

        public void SetCaretPosition(int position)
        {
            int pos = Math.Clamp(position, 0, _text.Length);
            if (_lineEdit != null)
            {
                _lineEdit.CaretColumn = pos;
                _lineEdit.Deselect();
            }
            else if (_textEdit != null)
            {
                var (line, column) = GetLineColumn(pos);
                _textEdit.Call("set_caret_line", line);
                _textEdit.Call("set_caret_column", column);
                _textEdit.Call("deselect");
            }
        }

        public int GetCaretPosition()
        {
            if (_lineEdit != null)
                return _lineEdit.CaretColumn;
            if (_textEdit != null)
            {
                int line = _textEdit.GetCaretLine();
                int column = _textEdit.GetCaretColumn();
                return GetIndexFromLineColumn(line, column);
            }
            return 0;
        }

        public void SetSelection(int start, int end)
        {
            start = Math.Clamp(start, 0, _text.Length);
            end = Math.Clamp(end, 0, _text.Length);

            if (_lineEdit != null)
            {
                if (start != end)
                    _lineEdit.Select(start, end);
                else
                    _lineEdit.Deselect();
                _lineEdit.CaretColumn = end;
            }
            else if (_textEdit != null)
            {
                if (start != end)
                {
                    var (startLine, startCol) = GetLineColumn(start);
                    var (endLine, endCol) = GetLineColumn(end);
                    _textEdit.Call("select", startLine, startCol, endLine, endCol);
                }
                else
                {
                    _textEdit.Call("deselect");
                }
                var (line, column) = GetLineColumn(end);
                _textEdit.Call("set_caret_line", line);
                _textEdit.Call("set_caret_column", column);
            }
        }

        public void SetSelection(System.Range range)
        {
            SetSelection(range.Start.GetOffset(_text.Length), range.End.GetOffset(_text.Length));
        }

        public void InsertText(string text)
        {
            if (HasSelection)
                DeleteSelection();
            int caret = GetCaretPosition();
            _text = _text.Insert(caret, text);
            if (_lineEdit != null)
            {
                _lineEdit.Text = _text;
                _lineEdit.CaretColumn = caret + text.Length;
                _lineEdit.Deselect();
            }
            else if (_textEdit != null)
            {
                _textEdit.Text = _text;
                var (line, column) = GetLineColumn(caret + text.Length);
                _textEdit.Call("set_caret_line", line);
                _textEdit.Call("set_caret_column", column);
                _textEdit.Call("deselect");
            }
            _onValueChanged?.Invoke();
            _onChange?.Invoke(Text);
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

