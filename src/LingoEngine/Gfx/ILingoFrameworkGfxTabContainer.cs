

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific container organizing children into tabs.
    /// </summary>
    public interface ILingoFrameworkGfxTabContainer : ILingoFrameworkGfxLayoutNode
    {
        string SelectedTabName { get; }

        /// <summary>Adds a new tab containing the specified node.</summary>
        void AddTab(ILingoFrameworkGfxTabItem content);
        void RemoveTab(ILingoFrameworkGfxTabItem content);
        IEnumerable<ILingoFrameworkGfxTabItem> GetTabs();

        /// <summary>Removes all tabs and their content.</summary>
        void ClearTabs();
        void SelectTabByName(string tabName);
    }
    public interface ILingoFrameworkGfxTabItem : ILingoFrameworkGfxLayoutNode
    {
        public string Title { get; set; }
        public ILingoGfxNode? Content { get; set; }
    }
}
