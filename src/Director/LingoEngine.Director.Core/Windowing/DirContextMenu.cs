using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;

namespace LingoEngine.Director.Core.Windowing
{
    /// <summary>
    /// Simple context menu linked to a window.
    /// </summary>
    public class DirContextMenu : IDisposable
    {
        private readonly ILingoFrameworkFactory _factory;
        private readonly LingoGfxMenu _menu;
        private readonly Func<(float X, float Y)> _positionProvider;
        private readonly Func<bool> _allowOpen;
        private readonly List<Item> _items = new();

        private record Item(LingoGfxMenuItem MenuItem, Func<bool> CanExecute, Action Execute);

        public DirContextMenu(
            object window,
            ILingoFrameworkFactory factory,
            Func<(float X, float Y)> positionProvider,
            Func<bool> allowOpen)
        {
            _factory = factory;
            _menu = factory.CreateContextMenu(window);
            _positionProvider = positionProvider;
            _allowOpen = allowOpen;
        }

        /// <summary>Adds a menu entry.</summary>
        public DirContextMenu AddItemFluent(string icon, string text, Func<bool> canExecute, Action execute)
        {
            AddItem(icon, text, canExecute, execute);
            return this;
        }
        public LingoGfxMenuItem AddItem(string icon, string text, Func<bool> canExecute, Action execute)
        {
            LingoGfxMenuItem item = _factory.CreateMenuItem(text);
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
        }

        public void Dispose()
        {
            _menu.Dispose();
        }
    }
}
