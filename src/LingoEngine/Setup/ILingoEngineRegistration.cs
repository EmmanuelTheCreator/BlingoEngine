using AbstUI;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Setup
{
    /// <summary>
    /// Lingo Engine Registration interface.
    /// </summary>
    public interface ILingoEngineRegistration
    {
        ILingoEngineRegistration ServicesMain(Action<IServiceCollection> services);
        ILingoEngineRegistration ServicesLingo(Action<IServiceCollection> services);
        ILingoEngineRegistration AddFont(string name, string pathAndName);
        ILingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action);
        ILingoEngineRegistration WithFrameworkFactory<T>(Action<T>? setup = null) where T : class, ILingoFrameworkFactory;
        ILingoEngineRegistration WithProjectSettings(Action<LingoProjectSettings> setup);
        LingoPlayer Build();
        ILingoEngineRegistration BuildDelayed();
        LingoPlayer Build(IServiceProvider serviceProvider);
        ILingoProjectFactory BuildAndRunProject(Action<IServiceProvider>? afterStart = null);
        ILingoEngineRegistration AddPreBuildAction(Action<IServiceProvider> buildAction);
        ILingoEngineRegistration AddBuildAction(Action<ILingoServiceProvider> buildAction);
        ILingoEngineRegistration SetProjectFactory<TLingoProjectFactory>() where TLingoProjectFactory : ILingoProjectFactory, new();
        ILingoEngineRegistration RegisterWindows(Action<IAbstFameworkWindowRegistrator>? registerWindows);
        ILingoEngineRegistration RegisterComponents(Action<IAbstFameworkComponentRegistrator>? componentWindows);
    }
}

