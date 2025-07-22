
namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper around a framework tab container.
    /// </summary>
    public class LingoGfxTabContainer : LingoGfxNodeLayoutBase<ILingoFrameworkGfxTabContainer>
    {
        public string SelectedTabName { get => _framework.SelectedTabName; }

        public void AddTab(LingoGfxTabItem item) => _framework.AddTab(item.Framework<ILingoFrameworkGfxTabItem>());
        public void AddTab(ILingoFrameworkGfxTabItem node) => _framework.AddTab(node);
        public void RemoveTab(LingoGfxTabItem node) => _framework.RemoveTab(node.Framework<ILingoFrameworkGfxTabItem>());
        public void RemoveChild(ILingoFrameworkGfxTabItem node) => _framework.RemoveTab(node);
        public IEnumerable<ILingoFrameworkGfxTabItem> GetChildren() => _framework.GetTabs();
        public void ClearTabs() => _framework.ClearTabs();

        public void SelectTabByName(string tabName) => _framework.SelectTabByName(tabName);
    }
}
