using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Net.RNetHost;

/// <summary>
/// Extension methods to configure the RNet host server.
/// </summary>
public static class LingoRNetHostSetup
{
    /// <summary>
    /// Registers and starts the host server used by the RNet tooling.
    /// </summary>
    /// <param name="reg">Engine registration.</param>
    /// <param name="url">URL on which the server will listen.</param>
    /// <returns>The same registration instance for chaining.</returns>
    public static ILingoEngineRegistration WithRNetHostServer(this ILingoEngineRegistration reg, string url = "http://localhost:61699")
    {
        reg.ServicesMain(s =>
        {
            s.AddSingleton<IRNetServer, RNetServer>();
            s.AddSingleton<IRNetPublisher>(p => p.GetRequiredService<IRNetServer>().Publisher);
        });

        reg.AddPreBuildAction(p =>
        {
            var server = p.GetRequiredService<IRNetServer>();
            server.StartAsync(url).GetAwaiter().GetResult();
        });

        return reg;
    }
}
