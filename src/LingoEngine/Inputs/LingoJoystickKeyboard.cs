using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Texts;
using System.Reflection.Emit;

namespace LingoEngine.Inputs
{
    /// <summary>
    /// On-screen keyboard navigated via joystick.
    /// Supports letters, digits, space and backspace.
    /// </summary>
    public class LingoJoystickKeyboard : ILingoKeyEventHandler, IDisposable
    {
        private readonly ILingoFrameworkFactory _factory;
        private readonly string[][] _layout;
        private readonly int _cols;
        private readonly LingoGfxWindow _window;
        private readonly LingoGfxCanvas _canvas;
        
        private readonly ILingoMouseSubscription _mouseDownSub;
        private readonly ILingoMouseSubscription _mouseMoveSub;
        private int _selectedRow;
        private int _selectedCol;
        private bool _enableMouse = true;
        private bool _enableKeyNumbers = true;
        private bool _enableKeyLetters = true;
        private bool _enableKeySpecial = true;

        public void SetWhiteTheme()
        {
            SelectedColor = LingoColorList.Black;
            SelectedBackgroundColor = LingoColorList.LightGray;
            BackgroundColor = LingoColorList.White;
            BorderColor = LingoColorList.LightGray;
            TextColor = LingoColorList.Black;
        }

        /// <summary>Color used for the border of the selected key.</summary>
        public LingoColor SelectedColor { get; set; } = LingoColorList.White;

        /// <summary>Optional fill color for the selected key.</summary>
        public LingoColor? SelectedBackgroundColor { get; set; } = LingoColorList.DarkGray;

        /// <summary>Background color of the keyboard.</summary>
        public LingoColor BackgroundColor { get; set; } = LingoColorList.Black;
        public LingoColor BorderColor { get; set; } = LingoColorList.DarkGray;
        public LingoColor TextColor { get; set; } = LingoColorList.White;

        /// <summary>Font name used for key labels.</summary>
        public string? FontName { get; set; }
        public int CellSpacing { get; set; } = 2;
        public int Margin { get; set; } = 5;
        public int FontSize { get; set; } = 12;
        public int CellSize { get; set; } = 32;

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

        private bool _showTitleBar = false;

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
        public event Action? EnterPressed;
        public event Action<string>? TextChanged;

        /// <summary>Raised when a key is selected.</summary>
        public event Action<string>? KeySelected;

        /// <summary>Supported keyboard layouts.</summary>
        public enum LingoKeyboardLayoutType
        {
            Azerty,
            Qwerty
        }

        public LingoJoystickKeyboard(ILingoFrameworkFactory factory, LingoKeyboardLayoutType layoutType = LingoKeyboardLayoutType.Azerty, bool showEscapeKey = false)
        {
            _factory = factory;
            _layout = layoutType == LingoKeyboardLayoutType.Azerty ? BuildAzertyLayout(showEscapeKey) : BuildQwertyLayout(showEscapeKey);
            _cols = _layout.Max(r => r.Length);
            var width = _cols * (CellSize + CellSpacing);
            var height = _layout.Length * (CellSize + CellSpacing);
            _window = _factory.CreateWindow("LingoJoystickKeyboard", _title);
            _canvas = _factory.CreateGfxCanvas("LingoJoystickKeyboardCanvas", width, height);
            _canvas.X = Margin;
            _canvas.Y = Margin;
            _window.AddItem(_canvas);
            _window.IsPopup = true;
            _window.OnWindowStateChanged += OnWindowStateChanged;
            _mouseDownSub = _window.Mouse.OnMouseDown(OnMouseDown);
            _mouseMoveSub = _window.Mouse.OnMouseMove(OnMouseMove);
            _window.Key.Subscribe(this);
            ApplyWindowChrome();
            DrawKeyboard();
            _window.Width = width + Margin*2;
            _window.Height = height+ Margin*2;
            _window.BackgroundColor = BackgroundColor;
        }
        public void Dispose()
        {
            _window.OnWindowStateChanged -= OnWindowStateChanged;
            _mouseDownSub.Release();
            _mouseMoveSub.Release();
            _window.Key.Unsubscribe(this);
            _window.Dispose();
        }
        public void UpdateStyle()
        {
            var width = _cols * (CellSize + CellSpacing);
            var height = _layout.Length * (CellSize + CellSpacing);
            _canvas.Width = width;
            _canvas.Height = height;
            _window.Width = width;
            _window.Height = height;
            _window.BackgroundColor = BackgroundColor;
            DrawKeyboard();
        }
        private void OnWindowStateChanged(bool state)
        {
            if (!state)
                Closed?.Invoke();
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
            string[][] layout =
            [
                ["1","2","3","4","5","6","7","8","9","0"],
                ["Q","W","E","R","T","Y","U","I","O","P","BACKSPACE"],
                ["A","S","D","F","G","H","J","K","L","ENTER"],
                ["Z","X","C","V","B","N","M","SPACE"]
            ];
            if (showEsc)
                layout[0] = layout[0].Concat(["ESC"]).ToArray();
            return layout;
        }

        private static string[][] BuildAzertyLayout(bool showEsc)
        {
            string[][] layout =
            [
                ["1","2","3","4","5","6","7","8","9","0"],
                ["A","Z","E","R","T","Y","U","I","O","P","BACKSPACE"],
                ["Q","S","D","F","G","H","J","K","L","M","ENTER"],
                ["W","X","C","V","B","N","SPACE"]
            ];
            if (showEsc)
                layout[0] = layout[0].Concat(new[] { "ESC" }).ToArray();
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
                    var x = c * (CellSize + CellSpacing) ;
                    var y = r * (CellSize + CellSpacing) ;
                    var key = _layout[r][c];
                    var selected = r == _selectedRow && c == _selectedCol;
                    if (selected && SelectedBackgroundColor.HasValue)
                        _canvas.DrawRect(LingoRect.New(x, y, CellSize, CellSize), SelectedBackgroundColor.Value, true);
                    var border = selected ? SelectedColor : BorderColor;
                    _canvas.DrawRect(LingoRect.New(x, y, CellSize, CellSize), border, false, 1);
                    var label = key switch { "SPACE" => "Space", "BACKSPACE" => "<-", "ENTER" => "OK", "ESC" => "Esc", _ => key };
                    _canvas.DrawText(new LingoPoint(x + 2, y + 20), label, FontName, TextColor, FontSize, CellSize - 4, LingoTextAlignment.Center);
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
                "ENTER" => string.Empty,
                "ESC" => string.Empty,
                _ => key
            };
        }

        /// <summary>Executes the currently selected key and updates <see cref="Text"/>.</summary>
        public string ExecuteSelectedKey()
        {
            var key = _layout[_selectedRow][_selectedCol];
            var result = GetSelectedKey();
            if (key == "ENTER")
            {
                EnterPressed?.Invoke();
            } 
            else if (key == "ESC")
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
                    Text = Text.Substring(0, Text.Length - 1);
            }
            else
            {
                if (Text.Length + value.Length <= MaxLength)
                    Text += value;
            }

            TextChanged?.Invoke(value);
            KeySelected?.Invoke(value);
            DrawKeyboard();
        }

        void ILingoKeyEventHandler.RaiseKeyUp(LingoKey key) { }
        void ILingoKeyEventHandler.RaiseKeyDown(LingoKey key)
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
            //Console.WriteLine($"Mouse moved: {e.MouseH}, {e.MouseV}");  
            if (!_window.Visibility) return;
            if (!_enableMouse) return;
            var localX = e.MouseH - Margin;
            var localY = e.MouseV - Margin;
            var row = (int)(localY / (CellSize + CellSpacing));
            var col = (int)(localX / (CellSize + CellSpacing));
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

       
    }
}

