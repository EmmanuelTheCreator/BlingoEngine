using BlingoEngine.Core;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Net.RNetPipeServer;

/// <summary>
/// Extension methods to configure the pipe-based RNet host server.
/// </summary>
public static class BlingoRNetPipeHostSetup
{
    /// <summary>
    /// Registers the pipe host server used by the RNet tooling.
    /// </summary>
    /// <param name="reg">Engine registration.</param>
    /// <param name="port">Identifier used to derive the pipe endpoint.</param>
    /// <param name="autoStart">Automatically start the host after build.</param>
    /// <returns>The same registration instance for chaining.</returns>
    public static IBlingoEngineRegistration WithRNetPipeHostServer(this IBlingoEngineRegistration reg, int port = 61699, bool autoStart = false)
    {
        reg.ServicesMain(s => s
            .AddSingleton<IRNetConfiguration>(new RNetConfiguration { Port = port })
            .AddSingleton<IRNetPipeServer, RNetPipeServer>()
            .AddSingleton<IRNetPublisherEngineBridge, RNetPipePublisher>()
            .AddSingleton<IRNetPipeBus, RNetPipeBus>());

        reg.AddPostBuildAction(p =>
        {
            var config = p.GetRequiredService<IRNetConfiguration>();

            if (config.AutoStartRNetHostOnStartup || autoStart)
            {
                var server = p.GetRequiredService<IRNetPipeServer>();
                server.StartAsync().GetAwaiter().GetResult();
                var publisher = p.GetRequiredService<IRNetPublisherEngineBridge>();
                publisher.Enable(p.GetRequiredService<IBlingoPlayer>());
            }
        });

        return reg;
    }
}

