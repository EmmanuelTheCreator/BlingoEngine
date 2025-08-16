using Microsoft.AspNetCore.Components;
using AbstUI.Components;

namespace AbstUI.Blazor.Components;

public class AbstBlazorTabItem : AbstBlazorComponentBase, IAbstFrameworkTabItem
{
    private IAbstNode? _content;

    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public float TopHeight { get; set; }
    public IAbstNode? Content
    {
        get => _content;
        set
        {
            _content = value;
            if (value is AbstBlazorComponentBase comp)
                ContentFragment = comp.RenderFragment;
            else
                ContentFragment = null;
        }
    }

    internal RenderFragment? ContentFragment { get; private set; }
}
