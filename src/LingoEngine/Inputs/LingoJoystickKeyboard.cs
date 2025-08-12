using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Texts;
using System;
using System.Linq;

namespace LingoEngine.Inputs
{
    /// <summary>
    /// On-screen keyboard navigated via joystick.
    /// Supports letters, digits, space and backspace.
    /// </summary>
    public class LingoJoystickKeyboard : IDisposable
    {
        private readonly ILingoFrameworkFactory _factory;
        private readonly string[][] _layout;
        private readonly int _cellWidth = 32;
        private readonly int _cellHeight = 32;
        private readonly LingoGfxWindow _window;
        private readonly LingoGfxCanvas _canvas;
        private readonly Action _onWindowClosed;
        private int _selectedRow;
        private int _selectedCol;
        private bool _enableMouse;
        private bool _enableKeyNumbers;
        private bool _enableKeyLetters;
        private bool _enableKeySpecial;

        /// <summary>Color used for the border of the selected key.</summary>
        public LingoColor SelectedColor { get; set; } = LingoColorList.Black;

        /// <summary>Optional fill color for the selected key.</summary>
        public LingoColor? SelectedBackgroundColor { get; set; }

        /// <summary>Background color of the keyboard.</summary>
        public LingoColor BackgroundColor { get; set; } = LingoColorList.White;

        /// <summary>Font name used for key labels.</summary>
        public string? FontName { get; set; }

        private string _title = "Keyboard";

        /// <summary>Window title.</summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                ApplyWindowChrome();
            }
        }

        private bool _showTitleBar = true;

        /// <summary>Whether to show the window title bar and close button.</summary>
        public bool ShowTitleBar
        {
            get => _showTitleBar;
            set
            {
                _showTitleBar = value;
                ApplyWindowChrome();
            }
        }

        /// <summary>Current text entered through the keyboard.</summary>
        public string Text { get; private set; } = string.Empty;

        /// <summary>Maximum allowed length of <see cref="Text"/>.</summary>
        public int MaxLength { get; set; } = int.MaxValue;

        /// <summary>Raised when the keyboard window is closed.</summary>
        public event Action? Closed;

        /// <summary>Raised when a key is selected.</summary>
        public event Action<string>? KeySelected;

        /// <summary>Supported keyboard layouts.</summary>
        public enum LingoKeyboardLayoutType
        {
            Azerty,
            Qwerty
        }

        public LingoJoystickKeyboard(ILingoFrameworkFactory factory, LingoKeyboardLayoutType layoutType = LingoKeyboardLayoutType.Qwerty, bool showEscapeKey = false)
        {
            _factory = factory;
            _layout = layoutType == LingoKeyboardLayoutType.Azerty ? BuildAzertyLayout(showEscapeKey) : BuildQwertyLayout(showEscapeKey);
            var cols = _layout.Max(r => r.Length);
            var width = cols * _cellWidth;
            var height = _layout.Length * _cellHeight;
            _window = _factory.CreateWindow("LingoJoystickKeyboard", _title);
            _window.IsPopup = true;
            _onWindowClosed = () => Closed?.Invoke();
            _window.OnClose += _onWindowClosed;
            _window.OnMouseDown += OnMouseDown;
            _window.OnMouseMove += OnMouseMove;
            _window.OnKeyDown += HandleKeyInput;
            _canvas = _factory.CreateGfxCanvas("LingoJoystickKeyboardCanvas", width, height);
            _window.AddItem(_canvas);
            ApplyWindowChrome();
            DrawKeyboard();
        }

        /// <summary>Enables hardware keyboard input.</summary>
        public void EnableKey(bool enableNumbers, bool enableLetters, bool enableSpecialKeys)
        {
            _enableKeyNumbers = enableNumbers;
            _enableKeyLetters = enableLetters;
            _enableKeySpecial = enableSpecialKeys;
        }

        /// <summary>Enables hardware mouse input.</summary>
        public void EnableMouse(bool state) => _enableMouse = state;

        private static string[][] BuildQwertyLayout(bool showEsc)
        {
            var layout = new[]
            {
                new[] { "1","2","3","4","5","6","7","8","9","0" },
                new[] { "Q","W","E","R","T","Y","U","I","O","P" },
                new[] { "A","S","D","F","G","H","J","K","L" },
                new[] { "Z","X","C","V","B","N","M","SPACE","BACKSPACE" }
            };
            if (showEsc)
                layout[^1] = layout[^1].Concat(new[] { "ESC" }).ToArray();
            return layout;
        }

        private static string[][] BuildAzertyLayout(bool showEsc)
        {
            var layout = new[]
            {
                new[] { "1","2","3","4","5","6","7","8","9","0" },
                new[] { "A","Z","E","R","T","Y","U","I","O","P" },
                new[] { "Q","S","D","F","G","H","J","K","L","M" },
                new[] { "W","X","C","V","B","N","SPACE","BACKSPACE" }
            };
            if (showEsc)
                layout[^1] = layout[^1].Concat(new[] { "ESC" }).ToArray();
            return layout;
        }

        private void ApplyWindowChrome()
        {
            if (ShowTitleBar)
                _window.Title = _title;
            else
                _window.Title = string.Empty;
            _window.Borderless = !ShowTitleBar;
        }

        private void DrawKeyboard()
        {
            _canvas.Clear(BackgroundColor);
            for (int r = 0; r < _layout.Length; r++)
            {
                for (int c = 0; c < _layout[r].Length; c++)
                {
                    var x = c * _cellWidth;
                    var y = r * _cellHeight;
                    var key = _layout[r][c];
                    var selected = r == _selectedRow && c == _selectedCol;
                    if (selected && SelectedBackgroundColor.HasValue)
                        _canvas.DrawRect(new LingoRect(x, y, _cellWidth, _cellHeight), SelectedBackgroundColor.Value, true);
                    var border = selected ? SelectedColor : LingoColorList.DarkGray;
                    _canvas.DrawRect(new LingoRect(x, y, _cellWidth, _cellHeight), border, false, 1);
                    var label = key switch { "SPACE" => "Space", "BACKSPACE" => "Bksp", "ESC" => "Esc", _ => key };
                    _canvas.DrawText(new LingoPoint(x + 2, y + 2), label, FontName, LingoColorList.Black, 12, _cellWidth - 4, LingoTextAlignment.Center);
                }
            }
        }

        /// <summary>Moves selection one cell up.</summary>
        public void MoveUp()
        {
            _selectedRow = (_selectedRow - 1 + _layout.Length) % _layout.Length;
            _selectedCol = Math.Min(_selectedCol, _layout[_selectedRow].Length - 1);
            DrawKeyboard();
        }

        /// <summary>Moves selection one cell down.</summary>
        public void MoveDown()
        {
            _selectedRow = (_selectedRow + 1) % _layout.Length;
            _selectedCol = Math.Min(_selectedCol, _layout[_selectedRow].Length - 1);
            DrawKeyboard();
        }

        /// <summary>Moves selection one cell left.</summary>
        public void MoveLeft()
        {
            var len = _layout[_selectedRow].Length;
            _selectedCol = (_selectedCol - 1 + len) % len;
            DrawKeyboard();
        }

        /// <summary>Moves selection one cell right.</summary>
        public void MoveRight()
        {
            var len = _layout[_selectedRow].Length;
            _selectedCol = (_selectedCol + 1) % len;
            DrawKeyboard();
        }

        /// <summary>Gets the string value of the currently selected key without executing it.</summary>
        public string GetSelectedKey()
        {
            var key = _layout[_selectedRow][_selectedCol];
            return key switch
            {
                "SPACE" => " ",
                "BACKSPACE" => "\b",
                "ESC" => string.Empty,
                _ => key
            };
        }

        /// <summary>Executes the currently selected key and updates <see cref="Text"/>.</summary>
        public string ExecuteSelectedKey()
        {
            var key = _layout[_selectedRow][_selectedCol];
            var result = GetSelectedKey();
            if (key == "ESC")
            {
                Close();
            }
            else
            {
                ProcessInput(result);
            }
            return result;
        }

        private void ProcessInput(string value)
        {
            if (value == "\b")
            {
                if (Text.Length > 0)
                    Text = Text[..^1];
            }
            else
            {
                if (Text.Length + value.Length <= MaxLength)
                    Text += value;
            }
            KeySelected?.Invoke(value);
            DrawKeyboard();
        }

        private void HandleKeyInput(LingoKey key)
        {
            if (!_window.Visibility) return;
            if (!(_enableKeyNumbers || _enableKeyLetters || _enableKeySpecial)) return;
            var name = key.Key.ToUpperInvariant();
            bool isDigit = name.Length == 1 && char.IsDigit(name[0]);
            bool isLetter = name.Length == 1 && char.IsLetter(name[0]);
            bool isSpecial = !isDigit && !isLetter;
            if ((isDigit && !_enableKeyNumbers) || (isLetter && !_enableKeyLetters) || (isSpecial && !_enableKeySpecial))
                return;
            switch (name)
            {
                case "UP":
                    MoveUp();
                    break;
                case "DOWN":
                    MoveDown();
                    break;
                case "LEFT":
                    MoveLeft();
                    break;
                case "RIGHT":
                    MoveRight();
                    break;
                case "ENTER":
                case "RETURN":
                    ExecuteSelectedKey();
                    break;
                case "SPACE":
                    ProcessInput(" ");
                    break;
                case "BACKSPACE":
                    ProcessInput("\b");
                    break;
                case "ESC":
                case "ESCAPE":
                    Close();
                    break;
                default:
                    if (name.Length == 1 && char.IsLetterOrDigit(name[0]))
                        ProcessInput(name);
                    break;
            }
        }

        private void OnMouseMove(LingoMouseEvent e)
        {
            if (!_window.Visibility) return;
            if (!_enableMouse) return;
            var localX = e.MouseH - _window.X;
            var localY = e.MouseV - _window.Y;
            if (localX < 0 || localY < 0 || localX >= _canvas.Width || localY >= _canvas.Height) return;
            var row = (int)(localY / _cellHeight);
            var col = (int)(localX / _cellWidth);
            if (row < 0 || row >= _layout.Length || col < 0 || col >= _layout[row].Length) return;
            _selectedRow = row;
            _selectedCol = col;
            DrawKeyboard();
        }

        private void OnMouseDown(LingoMouseEvent e)
        {
            if (!_window.Visibility) return;
            if (!_enableMouse) return;
            OnMouseMove(e);
            ExecuteSelectedKey();
            e.ContinuePropation = false;
        }

        /// <summary>Opens the keyboard popup window at the given position or centered if none.</summary>
        public void Open(LingoPoint? position = null)
        {
            ApplyWindowChrome();
            if (position.HasValue)
            {
                _window.X = position.Value.X;
                _window.Y = position.Value.Y;
                _window.Popup();
            }
            else
            {
                _window.PopupCentered();
            }
        }

        /// <summary>Closes the keyboard popup window.</summary>
        public void Close() => _window.Hide();

        public void Dispose()
        {
            _window.OnClose -= _onWindowClosed;
            _window.OnMouseDown -= OnMouseDown;
            _window.OnMouseMove -= OnMouseMove;
            _window.OnKeyDown -= HandleKeyInput;
            _window.Dispose();
        }
    }
}

