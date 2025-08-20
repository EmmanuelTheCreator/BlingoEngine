using System;
using AbstUI.Components;
using AbstUI.Components.Containers;

namespace AbstUI.Blazor.Components.Containers;

internal class AbstBlazorTabItemComponent : AbstBlazorComponentModelBase, IAbstFrameworkTabItem
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set { if (_title != value) { _title = value; RaiseChanged(); } }
    }

    private IAbstNode? _content;
    public IAbstNode? Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                ContentFrameworkNode = value?.FrameworkObj;
                RaiseChanged();
            }
        }
    }

    private float _topHeight;
    public float TopHeight
    {
        get => _topHeight;
        set { if (Math.Abs(_topHeight - value) > float.Epsilon) { _topHeight = value; RaiseChanged(); } }
    }

    internal object? ContentFrameworkNode { get; private set; }
}
