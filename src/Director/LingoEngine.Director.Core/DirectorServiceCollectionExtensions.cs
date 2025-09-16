using LingoEngine.Director.Core.Remote;
using LingoEngine.Net.RNetHost.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LingoEngine.Director.Core;

internal static class DirectorServiceCollectionExtensions
{
    public static IServiceCollection AddDirectorDummyProjectServer(this IServiceCollection services)
    {
        services.TryAddSingleton<IRNetProjectServer, DummyRNetProjectServer>();
        return services;
    }
}
