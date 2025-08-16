using System.Reflection;
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

    public RenderFragment RenderFragment => builder =>
    {
        builder.OpenComponent(0, GetType());
        foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetCustomAttribute<ParameterAttribute>() != null)
            {
                builder.AddAttribute(1, prop.Name, prop.GetValue(this));
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
