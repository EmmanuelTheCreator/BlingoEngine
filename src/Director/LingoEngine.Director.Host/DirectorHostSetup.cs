using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.Host;

/// <summary>
/// Extension methods to configure the Director debug host server.
/// </summary>
public static class DirectorHostSetup
{
    /// <summary>
    /// Registers and starts the debug host server used by the Director tooling.
    /// </summary>
    /// <param name="reg">Engine registration.</param>
    /// <param name="url">URL on which the server will listen.</param>
    /// <returns>The same registration instance for chaining.</returns>
    public static ILingoEngineRegistration WithDirectorHostServer(this ILingoEngineRegistration reg, string url = "http://localhost:61699")
    {
        reg.ServicesMain(s =>
        {
            s.AddSingleton<IDebugServer, DebugServer>();
            s.AddSingleton<IDebugPublisher>(p => p.GetRequiredService<IDebugServer>().Publisher);
        });

        reg.AddPreBuildAction(p =>
        {
            var server = p.GetRequiredService<IDebugServer>();
            server.StartAsync(url).GetAwaiter().GetResult();
        });

        return reg;
    }
}
