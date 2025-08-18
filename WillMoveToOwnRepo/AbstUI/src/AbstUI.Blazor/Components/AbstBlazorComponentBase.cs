using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public abstract class AbstBlazorComponentBase : ComponentBase, IAbstFrameworkLayoutNode
{
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public bool Visibility { get; set; } = true;
    [Parameter] public float Width { get; set; }
    [Parameter] public float Height { get; set; }
    [Parameter] public AMargin Margin { get; set; }
    [Parameter] public float X { get; set; }
    [Parameter] public float Y { get; set; }
    public object FrameworkNode => this;
    public virtual void Dispose() { }

    private bool _renderHandleAssigned;

    protected override void OnInitialized()
    {
        _renderHandleAssigned = true;
        base.OnInitialized();
    }

    /// <summary>
    /// Requests a re-render if the component has been initialized.
    /// This avoids calling <see cref="ComponentBase.StateHasChanged"/> before the
    /// render handle is assigned which would otherwise throw an exception when
    /// children are added during initial construction of the scene.
    /// </summary>
    protected void RequestRender()
    {
        if (_renderHandleAssigned)
            _ = InvokeAsync(StateHasChanged);
    }

    public RenderFragment RenderFragment => builder =>
    {
        BuildRenderTree(builder);
    };

    public static RenderFragment BuildFragment(Type componentType, IDictionary<string, object?>? parameters = null)
        => builder =>
        {
            builder.OpenComponent(0, componentType);
            if (parameters != null)
            {
                var seq = 1;
                foreach (var (key, value) in parameters)
                {
                    builder.AddAttribute(seq++, key, value);
                }
            }
            builder.CloseComponent();
        };

    protected virtual string BuildStyle()
    {
        var style = string.Empty;
        if (Width > 0) style += $"width:{Width}px;";
        if (Height > 0) style += $"height:{Height}px;";
        if (!Visibility) style += "display:none;";
        if (Margin.Left != 0 || Margin.Top != 0 || Margin.Right != 0 || Margin.Bottom != 0)
            style += $"margin:{Margin.Top}px {Margin.Right}px {Margin.Bottom}px {Margin.Left}px;";
        return style;
    }
}
