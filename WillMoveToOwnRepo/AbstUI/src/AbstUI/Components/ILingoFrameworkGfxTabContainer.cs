namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific container organizing children into tabs.
    /// </summary>
    public interface IAbstUIFrameworkGfxTabContainer : IAbstUIFrameworkGfxLayoutNode
    {
        string SelectedTabName { get; }

        /// <summary>Adds a new tab containing the specified node.</summary>
        void AddTab(IAbstUIFrameworkGfxTabItem content);
        void RemoveTab(IAbstUIFrameworkGfxTabItem content);
        IEnumerable<IAbstUIFrameworkGfxTabItem> GetTabs();

        /// <summary>Removes all tabs and their content.</summary>
        void ClearTabs();
        void SelectTabByName(string tabName);
    }
    public interface IAbstUIFrameworkGfxTabItem : IAbstUIFrameworkGfxLayoutNode
    {
        public string Title { get; set; }
        public IAbstUIGfxNode? Content { get; set; }
        float TopHeight { get; set; }
    }
}
