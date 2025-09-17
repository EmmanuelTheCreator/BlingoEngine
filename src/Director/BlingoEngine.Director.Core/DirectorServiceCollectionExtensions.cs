using BlingoEngine.Director.Core.Remote;
using BlingoEngine.Net.RNetHost.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlingoEngine.Director.Core;

internal static class DirectorServiceCollectionExtensions
{
    public static IServiceCollection AddDirectorDummyProjectServer(this IServiceCollection services)
    {
        services.TryAddSingleton<IRNetProjectServer, DummyRNetProjectServer>();
        return services;
    }
}

