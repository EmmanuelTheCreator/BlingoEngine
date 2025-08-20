using AbstUI.Components.Menus;
using AbstUI.LGodot.Components;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkMenuItem"/>.
    /// </summary>
    public partial class AbstGodotMenuItem : IAbstFrameworkMenuItem, IDisposable
    {
        private readonly AbstMenuItem _abstMenuItem;
        private string _name;
        private bool _enabled = true;
        private bool _check;
        private string? _shortcut;
        private AbstGodotMenu? _menu;
        internal int Id { get; private set; }

        public string Name { get => _name; set { _name = value; Update(); } }
        public bool Enabled { get => _enabled; set { _enabled = value; Update(); } }
        public bool CheckMark { get => _check; set { _check = value; Update(); } }
        public string? Shortcut { get => _shortcut; set { _shortcut = value; Update(); } }
        public object FrameworkNode => _menu?.FrameworkNode!;



        public event Action? Activated;

        public AbstGodotMenuItem(AbstMenuItem item, string name, string? shortcut)
        {
            _abstMenuItem = item;
            _name = name;
            _shortcut = shortcut;
            item.Init(this);
        }


        internal void Attach(AbstGodotMenu menu, int id)
        {
            _menu = menu;
            Id = id;
            menu.RegisterItem(this);
        }

        internal void RaiseActivated() => Activated?.Invoke();

        internal void Update()
        {
            _menu?.UpdateItem(this);
        }

        internal string GetDisplayText() => string.IsNullOrEmpty(_shortcut) ? _name : $"{_name}\t{_shortcut}";

        public void Dispose() { }
    }
}
