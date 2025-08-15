using System;
using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Base class for all engine level graphics nodes exposing common properties.
    /// </summary>
    public abstract class AbstUIGfxNodeBase<TFramework> : IAbstUIGfxNode, IDisposable
        where TFramework : IAbstUIFrameworkGfxNode
    {
#pragma warning disable CS8618
        protected TFramework _framework;
#pragma warning restore CS8618

        public virtual bool Visibility { get => _framework.Visibility; set => _framework.Visibility = value; }
        public virtual string Name { get => _framework.Name; set => _framework.Name = value; }
        public virtual AMargin Margin { get => _framework.Margin; set => _framework.Margin = value; }

        public virtual float Width { get => _framework.Width; set => _framework.Width = value; }
        public virtual float Height { get => _framework.Height; set => _framework.Height = value; }


        public virtual T Framework<T>() where T : IAbstUIFrameworkGfxNode => (T)(object)_framework;
        public virtual IAbstUIFrameworkGfxNode FrameworkObj => _framework;

        public virtual void Init(TFramework framework) => _framework = framework;


        public virtual void Dispose() => (_framework as IDisposable)?.Dispose();
    }

    public abstract class AbstUIGfxNodeLayoutBase<TFramework> : AbstUIGfxNodeBase<TFramework>, IAbstUIGfxLayoutNode
       where TFramework : IAbstUIFrameworkGfxLayoutNode
    {


        public virtual float X { get => _framework.X; set => _framework.X = value; }
        public virtual float Y { get => _framework.Y; set => _framework.Y = value; }



    }
}
