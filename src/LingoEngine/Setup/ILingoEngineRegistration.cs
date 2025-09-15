using AbstUI;
using LingoEngine.Core;
using AbstUI.Styles;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace LingoEngine.Setup
{
    /// <summary>
    /// Lingo Engine Registration interface.
    /// </summary>
    public interface ILingoEngineRegistration
    {
        ILingoEngineRegistration ServicesMain(Action<IServiceCollection> services);
        ILingoEngineRegistration ServicesLingo(Action<IServiceCollection> services);
        ILingoEngineRegistration AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular);
        ILingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action);
        ILingoEngineRegistration WithFrameworkFactory<T>(Action<T>? setup = null) where T : class, ILingoFrameworkFactory;
        ILingoEngineRegistration WithProjectSettings(Action<LingoProjectSettings> setup);
        ILingoEngineRegistration WithGlobalVarsR<TGlobalVars>(Action<TGlobalVars>? setup = null) where TGlobalVars : LingoGlobalVars;
        ILingoEngineRegistration WithGlobalVars<TGlobalVars>(Action<TGlobalVars>? setup = null) where TGlobalVars : LingoGlobalVars, new();
        ILingoEngineRegistration WithGlobalVarsDefault();
        LingoPlayer Build();
        Task<LingoPlayer> BuildAsync();
        ILingoEngineRegistration BuildDelayed();
        LingoPlayer Build(IServiceProvider serviceProvider, bool allowInitializeProject = true);
        Task<LingoPlayer> BuildAsync(IServiceProvider serviceProvider, bool allowInitializeProject = true);
        void InitializeProject();
        Task InitializeProjectAsync();
        ILingoProjectFactory BuildAndRunProject(Action<IServiceProvider>? afterStart = null);
        ILingoProjectFactory RunProject(Action<IServiceProvider>? afterStart = null);
        ILingoEngineRegistration AddPreBuildAction(Action<IServiceProvider> buildAction);
        ILingoEngineRegistration AddBuildAction(Action<ILingoServiceProvider> buildAction);
        ILingoEngineRegistration SetProjectFactory<TLingoProjectFactory>() where TLingoProjectFactory : ILingoProjectFactory, new();
        ILingoEngineRegistration RegisterWindows(Action<IAbstFameworkWindowRegistrator>? registerWindows);
        ILingoEngineRegistration RegisterComponents(Action<IAbstFameworkComponentRegistrator>? componentWindows);
    }
}

