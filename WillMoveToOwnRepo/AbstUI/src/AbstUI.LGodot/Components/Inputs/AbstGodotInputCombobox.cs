using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Components.Inputs;
using AbstUI.LGodot.Primitives;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkInputCombobox"/>.
    /// </summary>
    public partial class AbstGodotInputCombobox : OptionButton, IAbstFrameworkInputCombobox, IDisposable
    {
        private readonly List<KeyValuePair<string, string>> _items = new();
        private AMargin _margin = AMargin.Zero;
        private Action<string?>? _onChange;

        private event Action? _onValueChanged;

        public AbstGodotInputCombobox(AbstInputCombobox input, IAbstFontManager lingoFontManager, Action<string?>? onChange)
        {
            input.Init(this);
            ItemSelected += idx => _onValueChanged?.Invoke();
            _onChange = onChange;
            if (_onChange != null) ItemSelected += _ => _onChange(SelectedKey);
            UpdatePopupStyle();
        }


        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }
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
        public AColor ItemTextColor { get => _itemTextColor; set { _itemTextColor = value; UpdatePopupStyle(); } }
        private AColor _itemSelectedTextColor = AbstDefaultColors.InputSelectionText;
        public AColor ItemSelectedTextColor { get => _itemSelectedTextColor; set { _itemSelectedTextColor = value; UpdatePopupStyle(); } }
        private AColor _itemSelectedBackgroundColor = AbstDefaultColors.InputAccentColor;
        public AColor ItemSelectedBackgroundColor { get => _itemSelectedBackgroundColor; set { _itemSelectedBackgroundColor = value; UpdatePopupStyle(); } }
        private AColor _itemSelectedBorderColor = AbstDefaultColors.InputBorderColor;
        public AColor ItemSelectedBorderColor { get => _itemSelectedBorderColor; set { _itemSelectedBorderColor = value; UpdatePopupStyle(); } }
        private AColor _itemHoverTextColor = AbstDefaultColors.InputTextColor;
        public AColor ItemHoverTextColor { get => _itemHoverTextColor; set { _itemHoverTextColor = value; UpdatePopupStyle(); } }
        private AColor _itemHoverBackgroundColor = AbstDefaultColors.ListHoverColor;
        public AColor ItemHoverBackgroundColor { get => _itemHoverBackgroundColor; set { _itemHoverBackgroundColor = value; UpdatePopupStyle(); } }
        private AColor _itemHoverBorderColor = AbstDefaultColors.InputBorderColor;
        public AColor ItemHoverBorderColor { get => _itemHoverBorderColor; set { _itemHoverBorderColor = value; UpdatePopupStyle(); } }
        private AColor _itemPressedTextColor = AbstDefaultColors.InputSelectionText;
        public AColor ItemPressedTextColor { get => _itemPressedTextColor; set { _itemPressedTextColor = value; UpdatePopupStyle(); } }
        private AColor _itemPressedBackgroundColor = AbstDefaultColors.InputAccentColor;
        public AColor ItemPressedBackgroundColor { get => _itemPressedBackgroundColor; set { _itemPressedBackgroundColor = value; UpdatePopupStyle(); } }
        private AColor _itemPressedBorderColor = AbstDefaultColors.InputBorderColor;
        public AColor ItemPressedBorderColor { get => _itemPressedBorderColor; set { _itemPressedBorderColor = value; UpdatePopupStyle(); } }
        public int SelectedIndex { get => Selected; set => Selected = value; }
        public string? SelectedKey
        {
            get => Selected >= 0 ? (string?)GetItemMetadata(Selected) : null;
            set
            {
                if (value is null) { Selected = -1; return; }
                for (int i = 0; i < ItemCount; i++)
                {
                    if ((string?)GetItemMetadata(i) == value)
                    {
                        Selected = i;
                        break;
                    }
                }
            }
        }
        public string? SelectedValue
        {
            get => Selected >= 0 ? GetItemText(Selected) : null;
            set
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (GetItemText(i) == value)
                    {
                        Selected = i;
                        break;
                    }
                }
            }
        }

        private void UpdatePopupStyle()
        {
            var popup = GetPopup();
            popup.AddThemeColorOverride("font_color", _itemTextColor.ToGodotColor());
            popup.AddThemeColorOverride("font_color_hover", _itemHoverTextColor.ToGodotColor());
            popup.AddThemeColorOverride("font_color_pressed", _itemPressedTextColor.ToGodotColor());
            popup.AddThemeColorOverride("font_color_selected", _itemSelectedTextColor.ToGodotColor());

            var hover = new StyleBoxFlat
            {
                BgColor = _itemHoverBackgroundColor.ToGodotColor(),
                BorderColor = _itemHoverBorderColor.ToGodotColor()
            };
            hover.SetBorderWidthAll(1);
            popup.AddThemeStyleboxOverride("hover", hover);

            var pressed = new StyleBoxFlat
            {
                BgColor = _itemPressedBackgroundColor.ToGodotColor(),
                BorderColor = _itemPressedBorderColor.ToGodotColor()
            };
            pressed.SetBorderWidthAll(1);
            popup.AddThemeStyleboxOverride("pressed", pressed);

            var selected = new StyleBoxFlat
            {
                BgColor = _itemSelectedBackgroundColor.ToGodotColor(),
                BorderColor = _itemSelectedBorderColor.ToGodotColor()
            };
            selected.SetBorderWidthAll(1);
            popup.AddThemeStyleboxOverride("focus", selected);
        }

        event Action? IAbstFrameworkNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        public new void Dispose()
        {
            if (_onChange != null) ItemSelected -= _ => _onChange(SelectedKey);
            ItemSelected -= idx => _onValueChanged?.Invoke();
            QueueFree();
            base.Dispose();
        }
    }
}
