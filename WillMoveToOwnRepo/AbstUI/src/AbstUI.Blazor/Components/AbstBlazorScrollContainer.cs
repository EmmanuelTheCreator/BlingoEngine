using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorScrollContainer : AbstBlazorComponentBase, IAbstFrameworkScrollContainer
{
    private readonly List<IAbstFrameworkLayoutNode> _children = new();
    private readonly List<RenderFragment> _fragments = new();

    [Parameter] public bool ClipContents { get; set; }
    public float ScrollHorizontal { get; set; }
    public float ScrollVertical { get; set; }

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

    public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _children;

    protected override string BuildStyle()
    {
        var style = base.BuildStyle();
        style += $"position:absolute;left:{X}px;top:{Y}px;";
        style += ClipContents ? "overflow:hidden;" : "overflow:auto;";
        return style;
    }
}
