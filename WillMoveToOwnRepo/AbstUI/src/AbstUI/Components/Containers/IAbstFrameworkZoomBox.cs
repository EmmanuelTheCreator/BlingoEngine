using System;

namespace AbstUI.Components.Containers
{
    /// <summary>
    /// Framework-specific implementation for <see cref="AbstZoomBox"/> hosting a single child.
    /// </summary>
    public interface IAbstFrameworkZoomBox : IAbstFrameworkLayoutNode
    {
        /// <summary>Content displayed inside the zoom box.</summary>
        IAbstFrameworkLayoutNode? Content { get; set; }

        /// <summary>Horizontal zoom scale applied to the content.</summary>
        float ScaleH { get; set; }

        /// <summary>Vertical zoom scale applied to the content.</summary>
        float ScaleV { get; set; }
    }
}
