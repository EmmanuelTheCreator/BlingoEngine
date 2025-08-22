using AbstUI.Primitives;
using AbstUI.SDL2.Core;
using AbstUI.Windowing;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Windowing;

internal class AbstSdlMainWindow : IAbstFrameworkMainWindow, IFrameworkFor<AbstMainWindow>
{
    private readonly ISdlRootComponentContext _context;
    private AbstMainWindow _window = null!;

    public AbstSdlMainWindow(ISdlRootComponentContext context)
    {
        _context = context;
    }

    public string Title { get; set; } = string.Empty;

    public void Init(AbstMainWindow instance)
    {
        _window = instance;
        _window.SetTheSizeFromFW(GetTheSize());
    }

    public APoint GetTheSize() => _context.GetWindowSize();

    public void SetTheSize(APoint size)
    {
        // SDL2 window resizing not implemented
    }
}

