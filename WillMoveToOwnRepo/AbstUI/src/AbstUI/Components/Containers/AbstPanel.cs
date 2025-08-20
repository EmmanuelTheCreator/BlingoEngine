using AbstUI.Primitives;

namespace AbstUI.Components.Containers
{
    /// <summary>
    /// Simple container that allows placing child nodes at arbitrary coordinates.
    /// </summary>
    public class AbstPanel : AbstNodeLayoutBase<IAbstFrameworkPanel>
    {
        private readonly IAbstComponentFactory _factory;
        public IAbstComponentFactory Factory => _factory;
        public AbstPanel(IAbstComponentFactory factory)
        {
            _factory = factory;
        }


        /// <summary>Adds a child to the panel and sets its position.</summary>
        public IAbstNode AddItem(IAbstNode node, float? x = null, float? y = null)
        {
            if (node is IAbstLayoutNode layoutNode)
            {
                if (x != null) layoutNode.X = x.Value;
                if (y != null) layoutNode.Y = y.Value;
                _framework.AddItem(node.Framework<IAbstFrameworkLayoutNode>());
                return node;
            }
            else
            {
                AbstLayoutWrapper item = _factory.CreateLayoutWrapper(node, x, y);

                _framework.AddItem(item.FrameworkWrapper<IAbstFrameworkLayoutWrapper>());
                return item;
            }

        }

        public void RemoveItem(IAbstNode node) => _framework.RemoveItem(node.Framework<IAbstFrameworkLayoutNode>());
        public void RemoveItem(IAbstFrameworkLayoutNode node) => _framework.RemoveItem(node);
        public IEnumerable<IAbstFrameworkLayoutNode> GetChildren() => _framework.GetItems();

        public AColor? BackgroundColor { get => _framework.BackgroundColor; set => _framework.BackgroundColor = value; }
        public AColor? BorderColor { get => _framework.BorderColor; set => _framework.BorderColor = value; }
        public float BorderWidth { get => _framework.BorderWidth; set => _framework.BorderWidth = value; }
    }





    public class AbstLayoutWrapper : AbstNodeLayoutBase<IAbstFrameworkLayoutWrapper>
    {
        public IAbstNode Content { get; set; }


        public override bool Visibility { get => Content.Visibility; set => Content.Visibility = value; }
        public override string Name { get => Content.Name; set => Content.Name = value; }
        public override AMargin Margin { get => Content.Margin; set => Content.Margin = value; }
        public override float Width
        {
            get => Content.Width;
            set
            {
                Content.Width = value;
                _framework.Width = (int)value;
            }
        }
        public override float Height
        {
            get => Content.Height;
            set
            {
                Content.Height = value;
                _framework.Height = (int)value;
            }
        }


        public virtual T FrameworkWrapper<T>() where T : IAbstFrameworkLayoutWrapper => (T)(object)_framework;
        public virtual IAbstFrameworkNode FrameworkObjWrapper => _framework;


        public override T Framework<T>() => Content.Framework<T>();
      

        public override IAbstFrameworkNode FrameworkObj => _framework;



        public AbstLayoutWrapper(IAbstNode content)
        {
            Content = content;

        }




    }
}

