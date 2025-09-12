using System;
using AbstUI.Components;
using AbstUI.SDL2.Windowing;
using AbstUI.SDL2.Components;
using AbstUI.FrameworkCommunication;
using LingoEngine.Director.Core.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.SDL2.Behaviors;

internal class DirSdlBehaviorInspectorWindow : AbstSdlWindow, IDirFrameworkBehaviorInspectorWindow, IFrameworkFor<DirBehaviorInspectorWindow>
{
    private readonly DirBehaviorInspectorWindow _directorWindow;

    public DirSdlBehaviorInspectorWindow(DirBehaviorInspectorWindow directorWindow, IServiceProvider services)
        : base((AbstSdlComponentFactory)services.GetRequiredService<IAbstComponentFactory>())
    {
        _directorWindow = directorWindow;
        Init(_directorWindow);
    }
}
