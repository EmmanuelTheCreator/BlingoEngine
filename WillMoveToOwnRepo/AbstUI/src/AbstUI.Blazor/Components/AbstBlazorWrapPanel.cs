using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorWrapPanel : AbstBlazorComponentBase, IAbstFrameworkWrapPanel
{
    private readonly List<IAbstFrameworkNode> _children = new();
    private readonly List<RenderFragment> _fragments = new();

    [Parameter] public AOrientation Orientation { get; set; }
    [Parameter] public APoint ItemMargin { get; set; }

    public void AddItem(IAbstFrameworkNode child)
    {
        if (!_children.Contains(child))
        {
            _children.Add(child);
            if (child is AbstBlazorComponentBase comp)
                _fragments.Add(comp.RenderFragment);
            StateHasChanged();
        }
    }

    public void RemoveItem(IAbstFrameworkNode child)
    {
        var index = _children.IndexOf(child);
        if (index >= 0)
        {
            _children.RemoveAt(index);
            _fragments.RemoveAt(index);
            StateHasChanged();
        }
    }

    public IEnumerable<IAbstFrameworkNode> GetItems() => _children;

    public IAbstFrameworkNode? GetItem(int index)
        => index >= 0 && index < _children.Count ? _children[index] : null;

    public void RemoveAll()
    {
        _children.Clear();
        _fragments.Clear();
        StateHasChanged();
    }

    private string BuildWrapStyle() =>
        $"{BuildStyle()}display:flex;flex-wrap:wrap;flex-direction:{(Orientation == AOrientation.Horizontal ? "row" : "column")};";

    private string BuildItemStyle() => $"margin:{ItemMargin.Y}px {ItemMargin.X}px;";
}
