using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Simple container that allows placing child nodes at arbitrary coordinates.
    /// </summary>
    public class AbstUIGfxPanel : AbstUIGfxNodeLayoutBase<IAbstUIFrameworkGfxPanel>
    {
        private readonly IAbstUIGfxFactory _factory;
        public IAbstUIGfxFactory Factory => _factory;
        public AbstUIGfxPanel(IAbstUIGfxFactory factory)
        {
            _factory = factory;
        }


        /// <summary>Adds a child to the panel and sets its position.</summary>
        public IAbstUIGfxNode AddItem(IAbstUIGfxNode node, float? x = null, float? y = null)
        {
            if (node is IAbstUIGfxLayoutNode layoutNode)
            {
                if (x != null) layoutNode.X = x.Value;
                if (y != null) layoutNode.Y = y.Value;
                _framework.AddItem(node.Framework<IAbstUIFrameworkGfxLayoutNode>());
                return node;
            }
            else
            {
                AbstUIGfxLayoutWrapper item = _factory.CreateLayoutWrapper(node, x, y);

                _framework.AddItem(item.FrameworkWrapper<IAbstUIFrameworkGfxLayoutWrapper>());
                return item;
            }

        }

        public void RemoveItem(IAbstUIGfxNode node) => _framework.RemoveItem(node.Framework<IAbstUIFrameworkGfxLayoutNode>());
        public void RemoveItem(IAbstUIFrameworkGfxLayoutNode node) => _framework.RemoveItem(node);
        public IEnumerable<IAbstUIFrameworkGfxLayoutNode> GetChildren() => _framework.GetItems();

        public AColor? BackgroundColor { get => _framework.BackgroundColor; set => _framework.BackgroundColor = value; }
        public AColor? BorderColor { get => _framework.BorderColor; set => _framework.BorderColor = value; }
        public float BorderWidth { get => _framework.BorderWidth; set => _framework.BorderWidth = value; }
    }





    public class AbstUIGfxLayoutWrapper : AbstUIGfxNodeLayoutBase<IAbstUIFrameworkGfxLayoutWrapper>
    {
        public IAbstUIGfxNode Content { get; set; }


        public override bool Visibility { get => Content.Visibility; set => Content.Visibility = value; }
        public override string Name { get => Content.Name; set => Content.Name = value; }
        public override AMargin Margin { get => Content.Margin; set => Content.Margin = value; }
        public override float Width { get => Content.Width; set => Content.Width = value; }
        public override float Height { get => Content.Height; set => Content.Height = value; }


        public virtual T FrameworkWrapper<T>() where T : IAbstUIFrameworkGfxLayoutWrapper => (T)(object)_framework;
        public virtual IAbstUIFrameworkGfxNode FrameworkObjWrapper => _framework;


        public override T Framework<T>() => Content.Framework<T>();
        //{
        //    if (typeof(T) == typeof(IAbstUIFrameworkGfxLayoutNode) || typeof(T) == typeof(IAbstUIFrameworkGfxLayoutWrapper))
        //        return (T)(object)_framework;
        //    return Content.Framework<T>();
        //}

        public override IAbstUIFrameworkGfxNode FrameworkObj => _framework;



        public AbstUIGfxLayoutWrapper(IAbstUIGfxNode content)
        {
            Content = content;

        }




    }
}

