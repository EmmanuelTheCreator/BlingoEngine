using AbstUI.Components;
using AbstUI.FrameworkCommunication;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Windowing;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Scores;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.SDL2.Inspector;

internal class DirSdlPropertyInspectorWindow : AbstSdlWindow, IDirFrameworkPropertyInspectorWindow, IFrameworkFor<DirectorPropertyInspectorWindow>
{
    private readonly DirectorPropertyInspectorWindow _directorInspectorWindow;
    private readonly AbstSdlPanel _headerPanel;
    private readonly AbstSdlTabContainer _tabs;
    private const int TitleBarHeight = 24;

    public DirSdlPropertyInspectorWindow(DirectorPropertyInspectorWindow directorInspectorWindow, IServiceProvider services)
        : base((AbstSdlComponentFactory)services.GetRequiredService<IAbstComponentFactory>())
    {
        _directorInspectorWindow = directorInspectorWindow;
        Init(_directorInspectorWindow);
        _directorInspectorWindow.Init(TitleBarHeight);

        _headerPanel = _directorInspectorWindow.HeaderPanel.Framework<AbstSdlPanel>();
        _tabs = _directorInspectorWindow.Tabs.Framework<AbstSdlTabContainer>();

        _headerPanel.Y = TitleBarHeight;
        _headerPanel.Width = Width;

        _tabs.Y = TitleBarHeight + _headerPanel.Height;
        _tabs.Width = Width;
        _tabs.Height = Height - TitleBarHeight - _headerPanel.Height;

        AddItem(_headerPanel);
        AddItem(_tabs);

        _directorInspectorWindow.ResizeFromFW(true, (int)Width, (int)Height - TitleBarHeight - (int)_headerPanel.Height);
    }
}

