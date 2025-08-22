using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Windowing;

public class AbstBlazorMainWindow : IAbstFrameworkMainWindow, IFrameworkFor<AbstMainWindow>
{
    private AbstMainWindow _window = null!;
    private APoint _size;

    public string Title { get; set; } = string.Empty;

    public void Init(AbstMainWindow instance)
    {
        _window = instance;
        _window.SetTheSizeFromFW(_size);
    }

    public APoint GetTheSize() => _size;

    public void SetTheSize(APoint size)
    {
        _size = size;
    }
}

