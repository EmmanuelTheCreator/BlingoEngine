using System;
using AbstUI.Components;
using AbstUI.SDL2.Windowing;
using AbstUI.SDL2.Components;
using AbstUI.FrameworkCommunication;
using LingoEngine.Director.Core.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.SDL2.Behaviors;

internal class DirSdlBehaviorLibraryWindow : AbstSdlWindow, IDirFrameworkBehaviorLibraryWindow, IFrameworkFor<DirectorBehaviorLibraryWindow>
{
    private readonly DirectorBehaviorLibraryWindow _directorWindow;

    public DirSdlBehaviorLibraryWindow(DirectorBehaviorLibraryWindow directorWindow, IServiceProvider services)
        : base((AbstSdlComponentFactory)services.GetRequiredService<IAbstComponentFactory>())
    {
        _directorWindow = directorWindow;
        Init(_directorWindow);
    }
}
