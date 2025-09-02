using System;
using AbstUI.Components;
using AbstUI.FrameworkCommunication;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Windowing;
using LingoEngine.Director.Core.Inspector;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.SDL2.Inspector;

internal class DirSdlPropertyInspectorWindow : AbstSdlWindow, IDirFrameworkPropertyInspectorWindow, IFrameworkFor<DirectorPropertyInspectorWindow>
{
    private readonly DirectorPropertyInspectorWindow _directorInspectorWindow;
    private const int TitleBarHeight = 24;

    public DirSdlPropertyInspectorWindow(DirectorPropertyInspectorWindow directorInspectorWindow, IServiceProvider services)
        : base((AbstSdlComponentFactory)services.GetRequiredService<IAbstComponentFactory>())
    {
        _directorInspectorWindow = directorInspectorWindow;
        Init(_directorInspectorWindow);
        _directorInspectorWindow.Init(TitleBarHeight);

        _directorInspectorWindow.ResizingContentFromFW(true, (int)Width, (int)Height - TitleBarHeight);
    }
}

