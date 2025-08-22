using Godot;
using AbstUI.Primitives;
using AbstUI.Components.Buttons;
using AbstUI.Components.Menus;
using AbstUI.LGodot.Components.Menus;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkMenu"/>.
    /// </summary>
    public partial class AbstGodotMenu : PopupMenu, IAbstFrameworkMenu, IDisposable, IFrameworkFor<AbstMenu>
    {
        private readonly Dictionary<int, AbstGodotMenuItem> _items = new();
        private string _name;
        private AMargin _margin;

        public AbstGodotMenu(AbstMenu menu, string name)
        {
            _name = name;
            _margin = AMargin.Zero;
            menu.Init(this);
            IdPressed += OnIdPressed;

        }

        public new string Name { get => _name; set => _name = value; }

        public float X { get => Position.X; set => Position = new Vector2I((int)value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2I(Position.X, (int)value); }
        public float Width { get => Size.X; set => Size = new Vector2I((int)value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2I(Size.X, (int)value); }
        public bool Visibility { get => Visible; set => Visible = value; }

        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                AddThemeConstantOverride("margin_left", (int)value.Left);
                AddThemeConstantOverride("margin_right", (int)value.Right);
                AddThemeConstantOverride("margin_top", (int)value.Top);
                AddThemeConstantOverride("margin_bottom", (int)value.Bottom);
            }
        }

        public object FrameworkNode => this;

        public void AddItem(IAbstFrameworkMenuItem item)
        {
            if (item is not AbstGodotMenuItem godotItem)
                return;
            int id = ItemCount;
            godotItem.Attach(this, id);
            _items[id] = godotItem;
            base.AddItem(godotItem.GetDisplayText(), id);
            UpdateItem(godotItem);
        }

        public void ClearItems()
        {
            Clear();
            _items.Clear();
        }


        public void PositionPopup(IAbstFrameworkButton button)
        {
            if (button is not Button btn)
                return;
            Position = new Vector2I((int)btn.GlobalPosition.X, (int)(btn.GlobalPosition.Y + btn.Size.Y));
        }

        public void Popup() => base.Popup();

        internal void RegisterItem(AbstGodotMenuItem item)
        {
            UpdateItem(item);
        }

        internal void UpdateItem(AbstGodotMenuItem item)
        {
            if (!_items.ContainsKey(item.Id))
                return;
            int idx = GetItemIndex(item.Id);
            SetItemText(idx, item.GetDisplayText());
            SetItemDisabled(idx, !item.Enabled);
            SetItemChecked(idx, item.CheckMark);
        }

        private void OnIdPressed(long id)
        {
            if (_items.TryGetValue((int)id, out var item))
                item.RaiseActivated();
        }

        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }
    }
}
