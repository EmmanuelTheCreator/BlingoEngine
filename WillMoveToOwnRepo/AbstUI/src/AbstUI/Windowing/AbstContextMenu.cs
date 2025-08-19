using AbstUI;
using AbstUI.Components;
using AbstUI.Inputs;

namespace AbstUI.Windowing
{
    /// <summary>
    /// Simple context menu linked to a window.
    /// </summary>
    public class AbstContextMenu : IDisposable
    {
        private readonly IAbstComponentFactory _factory;
        private readonly AbstMenu _menu;
        private readonly Func<(float X, float Y)> _positionProvider;
        private readonly Func<bool> _allowOpen;
        private readonly List<Item> _items = new();
        private readonly IAbstMouse? _mouse;

        private record Item(AbstMenuItem MenuItem, Func<bool> CanExecute, Action Execute);

        public AbstContextMenu(
            object window,
            IAbstComponentFactory factory,
            Func<(float X, float Y)> positionProvider,
            Func<bool> allowOpen,
            AbstMouse? mouse)
        {
            _factory = factory;
            _menu = factory.CreateContextMenu(window);
            _positionProvider = positionProvider;
            _allowOpen = allowOpen;
            _mouse = mouse;
        }

        /// <summary>Adds a menu entry.</summary>
        public AbstContextMenu AddItemFluent(string icon, string text, Func<bool> canExecute, Action execute)
        {
            AddItem(icon, text, canExecute, execute);
            return this;
        }
        public AbstMenuItem AddItem(string icon, string text, Func<bool> canExecute, Action execute)
        {
            AbstMenuItem item = _factory.CreateMenuItem(text);
            item.Activated += () => { if (canExecute()) execute(); };
            _menu.AddItem(item);
            _items.Add(new Item(item, canExecute, execute));
            return item;
        }

        /// <summary>Shows the menu at the mouse position.</summary>
        public void Popup()
        {
            if (!_allowOpen()) return;
            foreach (var i in _items)
                i.MenuItem.Enabled = i.CanExecute();
            var pos = _positionProvider();
            _menu.X = pos.X;
            _menu.Y = pos.Y;
            _menu.Popup();
            if (_mouse != null)
            {
                //_mouse.RightMouseDown = false;
                //_mouse.DoMouseUp();
            }
        }

        public void Dispose()
        {
            _menu.Dispose();
        }
    }
}
