namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper around a framework tab container.
    /// </summary>
    public class AbstUIGfxTabContainer : AbstUIGfxNodeLayoutBase<IAbstUIFrameworkGfxTabContainer>
    {
        public string SelectedTabName { get => _framework.SelectedTabName; }

        public void AddTab(AbstUIGfxTabItem item) => _framework.AddTab(item.Framework<IAbstUIFrameworkGfxTabItem>());
        public void AddTab(IAbstUIFrameworkGfxTabItem node) => _framework.AddTab(node);
        public void RemoveTab(AbstUIGfxTabItem node) => _framework.RemoveTab(node.Framework<IAbstUIFrameworkGfxTabItem>());
        public void RemoveChild(IAbstUIFrameworkGfxTabItem node) => _framework.RemoveTab(node);
        public IEnumerable<IAbstUIFrameworkGfxTabItem> GetChildren() => _framework.GetTabs();
        public void ClearTabs() => _framework.ClearTabs();

        public void SelectTabByName(string tabName) => _framework.SelectTabByName(tabName);
    }
}
