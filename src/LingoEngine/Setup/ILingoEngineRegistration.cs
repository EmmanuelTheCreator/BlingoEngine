using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine.Setup
{
    public interface ILingoEngineRegistration
    {
        ILingoEngineRegistration ServicesMain(Action<IServiceCollection> services);
        ILingoEngineRegistration ServicesLingo(Action<IServiceCollection> services);
        ILingoEngineRegistration AddFont(string name, string pathAndName);
        ILingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action);
        ILingoEngineRegistration WithFrameworkFactory<T>(Action<T>? setup = null) where T : class, ILingoFrameworkFactory;
        ILingoEngineRegistration WithProjectSettings(Action<LingoProjectSettings> setup);
        LingoPlayer Build();
        ILingoProjectFactory BuildAndRunProject();
        ILingoEngineRegistration AddBuildAction(Action<ILingoServiceProvider> buildAction);
        ILingoEngineRegistration SetProjectFactory<TLingoProjectFactory>() where TLingoProjectFactory : ILingoProjectFactory, new();
    }
}

