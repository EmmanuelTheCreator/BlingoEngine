using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Components.Inputs;
using AbstUI.Styles;
using AbstUI.LGodot.Primitives;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkItemList"/>.
    /// </summary>
    public partial class AbstGodotItemList : ItemList, IAbstFrameworkItemList, IDisposable
    {
        private readonly List<KeyValuePair<string, string>> _items = new();
        private AMargin _margin = AMargin.Zero;
        private Action<string?>? _onChange;
        private event Action? _onValueChanged;
        private ItemSelectedEventHandler? _onItemSelected;

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); CustomMinimumSize = new Vector2(value, Size.Y); } }
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(Size.X, value); } }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get; set; } // { get => !Disabled; set => Disabled = !value; }
        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }

        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                AddThemeConstantOverride("margin_left", (int)_margin.Left);
                AddThemeConstantOverride("margin_right", (int)_margin.Right);
                AddThemeConstantOverride("margin_top", (int)_margin.Top);
                AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }

        public object FrameworkNode => this;
        public IReadOnlyList<KeyValuePair<string, string>> Items => _items;

        public AbstGodotItemList(AbstItemList list, Action<string?>? onChange)
        {
            _onChange = onChange;
            list.Init(this);
            _onItemSelected = idx =>
            {
                _onValueChanged?.Invoke();
                if (idx >= 0 && idx < _items.Count)
                    _onChange?.Invoke(_items[(int)idx].Key);
            };
            ItemSelected += _onItemSelected;
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
            SizeFlagsVertical = SizeFlags.ExpandFill;
            CustomMinimumSize = new Vector2(100, 50);
            UpdateStyle();
        }


        public void AddItem(string key, string value)
        {
            _items.Add(new KeyValuePair<string, string>(key, value));
            int idx = ItemCount;
            base.AddItem(value);
            SetItemMetadata(idx, key);
        }
        public void ClearItems()
        {
            _items.Clear();
            Clear();
        }

        public string? ItemFont { get; set; }
        public int ItemFontSize { get; set; } = 11;
        private AColor _itemTextColor = AbstDefaultColors.InputTextColor;
        public AColor ItemTextColor { get => _itemTextColor; set { _itemTextColor = value; UpdateStyle(); } }
        private AColor _itemSelectedTextColor = AbstDefaultColors.InputSelectionText;
        public AColor ItemSelectedTextColor { get => _itemSelectedTextColor; set { _itemSelectedTextColor = value; UpdateStyle(); } }
        private AColor _itemSelectedBackgroundColor = AbstDefaultColors.InputAccentColor;
        public AColor ItemSelectedBackgroundColor { get => _itemSelectedBackgroundColor; set { _itemSelectedBackgroundColor = value; UpdateStyle(); } }
        private AColor _itemSelectedBorderColor = AbstDefaultColors.InputBorderColor;
        public AColor ItemSelectedBorderColor { get => _itemSelectedBorderColor; set { _itemSelectedBorderColor = value; UpdateStyle(); } }
        private AColor _itemHoverTextColor = AbstDefaultColors.InputTextColor;
        public AColor ItemHoverTextColor { get => _itemHoverTextColor; set { _itemHoverTextColor = value; UpdateStyle(); } }
        private AColor _itemHoverBackgroundColor = AbstDefaultColors.ListHoverColor;
        public AColor ItemHoverBackgroundColor { get => _itemHoverBackgroundColor; set { _itemHoverBackgroundColor = value; UpdateStyle(); } }
        private AColor _itemHoverBorderColor = AbstDefaultColors.InputBorderColor;
        public AColor ItemHoverBorderColor { get => _itemHoverBorderColor; set { _itemHoverBorderColor = value; UpdateStyle(); } }
        private AColor _itemPressedTextColor = AbstDefaultColors.InputSelectionText;
        public AColor ItemPressedTextColor { get => _itemPressedTextColor; set { _itemPressedTextColor = value; UpdateStyle(); } }
        private AColor _itemPressedBackgroundColor = AbstDefaultColors.InputAccentColor;
        public AColor ItemPressedBackgroundColor { get => _itemPressedBackgroundColor; set { _itemPressedBackgroundColor = value; UpdateStyle(); } }
        private AColor _itemPressedBorderColor = AbstDefaultColors.InputBorderColor;
        public AColor ItemPressedBorderColor { get => _itemPressedBorderColor; set { _itemPressedBorderColor = value; UpdateStyle(); } }
        public int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value < 0 || value >= ItemCount)
                {
                    DeselectAll();
                }
                else
                {
                    _selectedIndex = value;
                    Select(value);
                }
            }
        }
        public string? SelectedKey
        {
            get => SelectedIndex >= 0 ? (string?)GetItemMetadata(SelectedIndex) : null;
            set
            {
                if (value is null) { DeselectAll(); return; }
                for (int i = 0; i < ItemCount; i++)
                {
                    if ((string?)GetItemMetadata(i) == value)
                    {
                        Select(i);
                        break;
                    }
                }
            }
        }
        public string? SelectedValue
        {
            get => SelectedIndex >= 0 ? GetItemText(SelectedIndex) : null;
            set
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (GetItemText(i) == value)
                    {
                        Select(i);
                        break;
                    }
                }
            }
        }

        private void UpdateStyle()
        {
            AddThemeColorOverride("font_color", _itemTextColor.ToGodotColor());
            AddThemeColorOverride("font_color_hovered", _itemHoverTextColor.ToGodotColor());
            AddThemeColorOverride("font_color_selected", _itemSelectedTextColor.ToGodotColor());

            var hover = new StyleBoxFlat
            {
                BgColor = _itemHoverBackgroundColor.ToGodotColor(),
                BorderColor = _itemHoverBorderColor.ToGodotColor()
            };
            hover.SetBorderWidthAll(1);
            AddThemeStyleboxOverride("hover", hover);

            var pressed = new StyleBoxFlat
            {
                BgColor = _itemPressedBackgroundColor.ToGodotColor(),
                BorderColor = _itemPressedBorderColor.ToGodotColor()
            };
            pressed.SetBorderWidthAll(1);
            AddThemeStyleboxOverride("pressed", pressed);

            var selected = new StyleBoxFlat
            {
                BgColor = _itemSelectedBackgroundColor.ToGodotColor(),
                BorderColor = _itemSelectedBorderColor.ToGodotColor()
            };
            selected.SetBorderWidthAll(1);
            AddThemeStyleboxOverride("selected", selected);
        }

        event Action? IAbstFrameworkNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        public new void Dispose()
        {
            if (_onItemSelected != null) ItemSelected -= _onItemSelected;
            QueueFree();
            base.Dispose();
        }
    }
}
