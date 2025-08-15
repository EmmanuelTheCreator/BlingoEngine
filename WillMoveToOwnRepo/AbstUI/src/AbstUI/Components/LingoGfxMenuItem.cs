namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a single menu item.
    /// </summary>
    public class AbstUIGfxMenuItem
    {
        private IAbstUIFrameworkGfxMenuItem _framework = null!;
        internal IAbstUIFrameworkGfxMenuItem Framework => _framework;

        public void Init(IAbstUIFrameworkGfxMenuItem framework) => _framework = framework;

        public string Name { get => _framework.Name; set => _framework.Name = value; }
        public bool Enabled { get => _framework.Enabled; set => _framework.Enabled = value; }
        public bool CheckMark { get => _framework.CheckMark; set => _framework.CheckMark = value; }
        public string? Shortcut { get => _framework.Shortcut; set => _framework.Shortcut = value; }
        public event Action? Activated
        {
            add { _framework.Activated += value; }
            remove { _framework.Activated -= value; }
        }
    }
}
