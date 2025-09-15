using System;
using LingoEngine.Net.RNetProjectClient;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Net.RNetClientPlayer;

/// <summary>
/// Extension helpers to register the RNet client player.
/// </summary>
public static class LingoRNetClientPlayerSetup
{
    /// <summary>
    /// Registers the RNet client player and connects to the specified host after the player is built.
    /// </summary>
    public static ILingoEngineRegistration WithRNetClientPlayer(this ILingoEngineRegistration reg, Uri hubUrl, HelloDto hello)
    {
        reg.ServicesMain(s =>
        {
            s.AddSingleton<ILingoRNetProjectClient, LingoRNetProjectClient>();
            s.AddSingleton<LingoRNetClientPlayer>();
        });

        reg.AddBuildAction(sp =>
        {
            var player = sp.GetRequiredService<LingoRNetClientPlayer>();
            player.ConnectAsync(hubUrl, hello).GetAwaiter().GetResult();
        });

        return reg;
    }
}
