using AbstUI.Primitives;

namespace AbstUI.Components.Containers
{
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

