using System;

namespace AbstUI.Components.Containers
{
    /// <summary>
    /// Container that hosts a single child and allows panning and scaling it.
    /// </summary>
    public class AbstZoomBox : AbstNodeLayoutBase<IAbstFrameworkZoomBox>
    {
        private IAbstLayoutNode? _content;

        /// <summary>Child displayed inside the zoom box.</summary>
        public IAbstLayoutNode? Content
        {
            get => _content;
            set
            {
                _content = value;
                _framework.Content = _content?.Framework<IAbstFrameworkLayoutNode>();
            }
        }

        /// <summary>Horizontal offset applied to the content.</summary>
        public float OffsetX
        {
            get => _content?.X ?? 0f;
            set { if (_content != null) _content.X = value; }
        }

        /// <summary>Vertical offset applied to the content.</summary>
        public float OffsetY
        {
            get => _content?.Y ?? 0f;
            set { if (_content != null) _content.Y = value; }
        }

        /// <summary>Horizontal zoom scale applied to the content. Scaling behavior is framework‑dependent.</summary>
        public float ScaleH
        {
            get => _framework.ScaleH;
            set => _framework.ScaleH = value;
        }

        /// <summary>Vertical zoom scale applied to the content. Scaling behavior is framework‑dependent.</summary>
        public float ScaleV
        {
            get => _framework.ScaleV;
            set => _framework.ScaleV = value;
        }
    }
}
