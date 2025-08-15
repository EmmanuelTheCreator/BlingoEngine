using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a panel that arranges children with wrapping.
    /// </summary>
    public class AbstUIGfxWrapPanel : AbstUIGfxNodeLayoutBase<IAbstUIFrameworkGfxWrapPanel>
    {

        public AOrientation Orientation
        {
            get => _framework.Orientation;
            set => _framework.Orientation = value;
        }

        public APoint ItemMargin
        {
            get => _framework.ItemMargin;
            set => _framework.ItemMargin = value;
        }
        public IAbstUIGfxFactory Factory { get; }

        public AbstUIGfxWrapPanel(IAbstUIGfxFactory factory)
        {
            Factory = factory;
        }

        public AbstUIGfxWrapPanel AddItem(IAbstUIGfxNode node)
        {
            _framework.AddItem(node.Framework<IAbstUIFrameworkGfxNode>());
            return this;
        }

        public AbstUIGfxWrapPanel AddItem(IAbstUIFrameworkGfxNode node)
        {
            _framework.AddItem(node);
            return this;
        }

        public void RemoveItem(IAbstUIGfxNode node) => _framework.RemoveItem(node.Framework<IAbstUIFrameworkGfxNode>());
        public void RemoveItem(IAbstUIFrameworkGfxNode node) => _framework.RemoveItem(node);
        public IEnumerable<IAbstUIFrameworkGfxNode> GetItems() => _framework.GetItems();

        public IAbstUIFrameworkGfxNode? GetItem(int index) => _framework.GetItem(index);

        public void RemoveAll() => _framework.RemoveAll();
    }
}
