namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific container organizing children into tabs.
    /// </summary>
    public interface IAbstFrameworkTabContainer : IAbstFrameworkLayoutNode
    {
        string SelectedTabName { get; }

        /// <summary>Adds a new tab containing the specified node.</summary>
        void AddTab(IAbstFrameworkTabItem content);
        void RemoveTab(IAbstFrameworkTabItem content);
        IEnumerable<IAbstFrameworkTabItem> GetTabs();

        /// <summary>Removes all tabs and their content.</summary>
        void ClearTabs();
        void SelectTabByName(string tabName);
    }
    public interface IAbstFrameworkTabItem : IAbstFrameworkLayoutNode
    {
        public string Title { get; set; }
        public IAbstNode? Content { get; set; }
        float TopHeight { get; set; }
    }
}
