using LingoEngine.Director.Core.Casts;
using AbstUI.Components;
using AbstUI.SDL2.Components;
using Microsoft.Extensions.DependencyInjection;
using AbstUI.SDL2.Windowing;
using AbstUI.FrameworkCommunication;

namespace LingoEngine.Director.SDL2.Casts
{
    internal class DirSdlCastWindow : AbstSdlWindow, IDirFrameworkCastWindow , IFrameworkFor<DirectorCastWindow>
    {
        private readonly DirectorCastWindow _directorCastWindow;

        public DirSdlCastWindow(DirectorCastWindow directorCastWindow, IServiceProvider services)
            : base((AbstSdlComponentFactory)services.GetRequiredService<IAbstComponentFactory>())
        {
            _directorCastWindow = directorCastWindow;
            Init(_directorCastWindow);
        }
    }
}
