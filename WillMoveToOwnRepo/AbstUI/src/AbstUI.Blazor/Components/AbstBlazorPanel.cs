using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorPanel : AbstBlazorComponentBase, IAbstFrameworkPanel
{
    private readonly List<IAbstFrameworkLayoutNode> _children = new();
    private readonly List<RenderFragment> _fragments = new();

    [Parameter] public AColor? BackgroundColor { get; set; }
    [Parameter] public AColor? BorderColor { get; set; }
    [Parameter] public float BorderWidth { get; set; }

    public void AddItem(IAbstFrameworkLayoutNode child)
    {
        if (!_children.Contains(child))
        {
            _children.Add(child);
            if (child is AbstBlazorComponentBase comp)
                _fragments.Add(comp.RenderFragment);
            StateHasChanged();
        }
    }

    public void RemoveItem(IAbstFrameworkLayoutNode child)
    {
        var index = _children.IndexOf(child);
        if (index >= 0)
        {
            _children.RemoveAt(index);
            _fragments.RemoveAt(index);
            StateHasChanged();
        }
    }

    public void RemoveAll()
    {
        _children.Clear();
        _fragments.Clear();
        StateHasChanged();
    }

    public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _children;

    private string BuildPanelStyle()
    {
        var style = string.Empty;
        if (BackgroundColor.HasValue)
            style += $"background-color:rgba({BackgroundColor.Value.R},{BackgroundColor.Value.G},{BackgroundColor.Value.B},{BackgroundColor.Value.A / 255f});";
        if (BorderColor.HasValue && BorderWidth > 0)
            style += $"border:{BorderWidth}px solid rgba({BorderColor.Value.R},{BorderColor.Value.G},{BorderColor.Value.B},{BorderColor.Value.A / 255f});";
        return style;
    }
}
