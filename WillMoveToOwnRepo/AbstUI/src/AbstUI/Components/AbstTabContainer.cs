namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper around a framework tab container.
    /// </summary>
    public class AbstTabContainer : AbstNodeLayoutBase<IAbstFrameworkTabContainer>
    {
        public string SelectedTabName { get => _framework.SelectedTabName; }

        public void AddTab(AbstTabItem item) => _framework.AddTab(item.Framework<IAbstFrameworkTabItem>());
        public void AddTab(IAbstFrameworkTabItem node) => _framework.AddTab(node);
        public void RemoveTab(AbstTabItem node) => _framework.RemoveTab(node.Framework<IAbstFrameworkTabItem>());
        public void RemoveChild(IAbstFrameworkTabItem node) => _framework.RemoveTab(node);
        public IEnumerable<IAbstFrameworkTabItem> GetChildren() => _framework.GetTabs();
        public void ClearTabs() => _framework.ClearTabs();

        public void SelectTabByName(string tabName) => _framework.SelectTabByName(tabName);
    }
}
