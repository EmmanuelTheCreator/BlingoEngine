using System.Collections.Generic;
using AbstUI.Blazor.Components.Texts;
using AbstUI.Components;
using LingoEngine.Blazor.Movies;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Blazor.Stages;

/// <summary>
/// Minimal debug overlay that renders lines of text using AbstUI labels.
/// </summary>
public class LingoBlazorDebugOverlay : ILingoFrameworkDebugOverlay
{
    private readonly LingoBlazorRootPanel _root;
    private readonly IAbstComponentFactory _factory;
    private readonly List<AbstBlazorLabelComponent> _labels = new();

    public LingoBlazorDebugOverlay(LingoBlazorRootPanel root, IAbstComponentFactory factory)
    {
        _root = root;
        _factory = factory;
    }

    public void ShowDebugger()
    {
        foreach (var l in _labels)
            l.Visibility = true;
    }

    public void HideDebugger()
    {
        foreach (var l in _labels)
            l.Visibility = false;
    }

    public void Begin() { }

    public int PrepareLine(int id, string text)
    {
        var label = _factory.CreateLabel($"Dbg{id}", text);
        var impl = label.Framework<AbstBlazorLabelComponent>();
        impl.X = 10;
        impl.Y = (id - 1) * 15;
        impl.Visibility = false;
        _root.Component.AddItem(impl);
        _labels.Add(impl);
        return id;
    }

    public void SetLineText(int id, string text)
    {
        var index = id - 1;
        if (index >= 0 && index < _labels.Count)
            _labels[index].Text = text;
    }

    public void End() { }
}

