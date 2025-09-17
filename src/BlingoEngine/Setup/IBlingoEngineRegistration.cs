using AbstUI;
using BlingoEngine.Core;
using AbstUI.Styles;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BlingoEngine.Setup
{
    /// <summary>
    /// Lingo Engine Registration interface.
    /// </summary>
    public interface IBlingoEngineRegistration
    {
        IBlingoEngineRegistration ServicesMain(Action<IServiceCollection> services);
        IBlingoEngineRegistration ServicesBlingo(Action<IServiceCollection> services);
        IBlingoEngineRegistration AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular);
        IBlingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action);
        IBlingoEngineRegistration WithFrameworkFactory<T>(Action<T>? setup = null) where T : class, IBlingoFrameworkFactory;
        IBlingoEngineRegistration WithProjectSettings(Action<BlingoProjectSettings> setup);
        IBlingoEngineRegistration WithGlobalVarsR<TGlobalVars>(Action<TGlobalVars>? setup = null) where TGlobalVars : BlingoGlobalVars;
        IBlingoEngineRegistration WithGlobalVars<TGlobalVars>(Action<TGlobalVars>? setup = null) where TGlobalVars : BlingoGlobalVars, new();
        IBlingoEngineRegistration WithGlobalVarsDefault();
        BlingoPlayer Build();
        Task<BlingoPlayer> BuildAsync();
        IBlingoEngineRegistration BuildDelayed();
        BlingoPlayer Build(IServiceProvider serviceProvider, bool allowInitializeProject = true);
        Task<BlingoPlayer> BuildAsync(IServiceProvider serviceProvider, bool allowInitializeProject = true);
        void InitializeProject();
        Task InitializeProjectAsync();
        IBlingoProjectFactory BuildAndRunProject(Action<IServiceProvider>? afterStart = null);
        IBlingoProjectFactory RunProject(Action<IServiceProvider>? afterStart = null);
        IBlingoEngineRegistration AddPreBuildAction(Action<IServiceProvider> buildAction);
        IBlingoEngineRegistration AddPostBuildAction(Action<IServiceProvider> buildAction);
        IBlingoEngineRegistration AddBuildAction(Action<IBlingoServiceProvider> buildAction);
        IBlingoEngineRegistration SetProjectFactory<TBlingoProjectFactory>() where TBlingoProjectFactory : IBlingoProjectFactory, new();
        IBlingoEngineRegistration RegisterWindows(Action<IAbstFameworkWindowRegistrator>? registerWindows);
        IBlingoEngineRegistration RegisterComponents(Action<IAbstFameworkComponentRegistrator>? componentWindows);
    }
}


