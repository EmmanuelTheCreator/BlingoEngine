namespace AbstUI.Components.Containers
{
    /// <summary>
    /// Engine level wrapper around a framework scroll container.
    /// </summary>
    public class AbstScrollContainer : AbstNodeLayoutBase<IAbstFrameworkScrollContainer>
    {
        public void AddItem(IAbstNode node) => _framework.AddItem(node.Framework<IAbstFrameworkLayoutNode>());
        public void AddItem(IAbstFrameworkLayoutNode node) => _framework.AddItem(node);
        public void RemoveItem(IAbstNode node) => _framework.RemoveItem(node.Framework<IAbstFrameworkLayoutNode>());
        public void RemoveItem(IAbstFrameworkLayoutNode node) => _framework.RemoveItem(node);
        public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _framework.GetItems();

        public float ScrollHorizontal { get => _framework.ScrollHorizontal; set => _framework.ScrollHorizontal = value; }
        public float ScrollVertical { get => _framework.ScrollVertical; set => _framework.ScrollVertical = value; }
        public bool ClipContents { get => _framework.ClipContents; set => _framework.ClipContents = value; }
    }
}
