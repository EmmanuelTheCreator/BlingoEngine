using System;
using AbstUI.Primitives;

namespace AbstUI.Windowing;

public class AbstMainWindow
{
    private readonly IAbstFrameworkMainWindow _framework;
    private APoint _size;

    public AbstMainWindow(IAbstFrameworkMainWindow framework)
    {
        _framework = framework;
        _framework.Init(this);
        _size = _framework.GetTheSize();
    }

    public string Title
    {
        get => _framework.Title;
        set => _framework.Title = value;
    }

    public event Action<APoint>? SizeChanged;

    public APoint GetSize() => _size;

    public void SetTheSizeFromFW(APoint size)
    {
        _size = size;
        SizeChanged?.Invoke(size);
    }

    public void SetSize(APoint size)
    {
        _size = size;
        _framework.SetTheSize(size);
    }
}

