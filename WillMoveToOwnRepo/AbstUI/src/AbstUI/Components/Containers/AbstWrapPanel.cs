using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a panel that arranges children with wrapping.
    /// </summary>
    public class AbstWrapPanel : AbstNodeLayoutBase<IAbstFrameworkWrapPanel>
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
        public IAbstComponentFactory Factory { get; }

        public AbstWrapPanel(IAbstComponentFactory factory)
        {
            Factory = factory;
        }

        public AbstWrapPanel AddItem(IAbstNode node)
        {
            _framework.AddItem(node.Framework<IAbstFrameworkNode>());
            return this;
        }

        public AbstWrapPanel AddItem(IAbstFrameworkNode node)
        {
            _framework.AddItem(node);
            return this;
        }

        public void RemoveItem(IAbstNode node) => _framework.RemoveItem(node.Framework<IAbstFrameworkNode>());
        public void RemoveItem(IAbstFrameworkNode node) => _framework.RemoveItem(node);
        public IEnumerable<IAbstFrameworkNode> GetItems() => _framework.GetItems();

        public IAbstFrameworkNode? GetItem(int index) => _framework.GetItem(index);

        public void RemoveAll() => _framework.RemoveAll();
    }
}
