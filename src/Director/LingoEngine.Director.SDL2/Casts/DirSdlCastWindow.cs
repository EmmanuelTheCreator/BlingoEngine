using LingoEngine.Director.Core.Casts;
using AbstUI.Components;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Containers;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.SDL2.Casts
{
    internal class DirSdlCastWindow : AbstSdlWindow, IDirFrameworkCastWindow
    {
        private readonly DirectorCastWindow _directorCastWindow;
        private readonly AbstSdlTabContainer _tabs;
        private const int TitleBarHeight = 24;

        public DirSdlCastWindow(DirectorCastWindow directorCastWindow, IServiceProvider services)
            : base((AbstSdlComponentFactory)services.GetRequiredService<IAbstComponentFactory>())
        {
            _directorCastWindow = directorCastWindow;
            Init(_directorCastWindow);
            _tabs = _directorCastWindow.TabContainer.Framework<AbstSdlTabContainer>();
            _tabs.Y = TitleBarHeight;
            _tabs.Width = Width;
            _tabs.Height = Height - TitleBarHeight;
            AddItem(_tabs);
            _directorCastWindow.ResizeFromFW(true, (int)Width, (int)Height - TitleBarHeight);
        }
    }
}
