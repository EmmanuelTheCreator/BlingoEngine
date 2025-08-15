namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper around a framework scroll container.
    /// </summary>
    public class AbstUIGfxScrollContainer : AbstUIGfxNodeLayoutBase<IAbstUIFrameworkGfxScrollContainer>
    {
        public void AddItem(IAbstUIGfxNode node) => _framework.AddItem(node.Framework<IAbstUIFrameworkGfxLayoutNode>());
        public void AddItem(IAbstUIFrameworkGfxLayoutNode node) => _framework.AddItem(node);
        public void RemoveItem(IAbstUIGfxNode node) => _framework.RemoveItem(node.Framework<IAbstUIFrameworkGfxLayoutNode>());
        public void RemoveItem(IAbstUIFrameworkGfxLayoutNode node) => _framework.RemoveItem(node);
        public IEnumerable<IAbstUIFrameworkGfxLayoutNode> GetItems() => _framework.GetItems();

        public float ScrollHorizontal { get => _framework.ScrollHorizontal; set => _framework.ScrollHorizontal = value; }
        public float ScrollVertical { get => _framework.ScrollVertical; set => _framework.ScrollVertical = value; }
        public bool ClipContents { get => _framework.ClipContents; set => _framework.ClipContents = value; }
    }
}
