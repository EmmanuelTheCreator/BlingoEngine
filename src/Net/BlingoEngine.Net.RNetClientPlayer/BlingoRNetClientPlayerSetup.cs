using System;
using BlingoEngine.Net.RNetProjectClient;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Net.RNetClientPlayer;

/// <summary>
/// Extension helpers to register the RNet client player.
/// </summary>
public static class BlingoRNetClientPlayerSetup
{
    /// <summary>
    /// Registers the RNet client player and connects to the specified host after the player is built.
    /// </summary>
    public static IBlingoEngineRegistration WithRNetClientPlayer(this IBlingoEngineRegistration reg, Uri hubUrl, HelloDto hello)
    {
        reg.ServicesMain(s =>
        {
            s.AddSingleton<IBlingoRNetProjectClient, BlingoRNetProjectClient>();
            s.AddSingleton<BlingoRNetClientPlayer>();
        });

        reg.AddBuildAction(sp =>
        {
            var player = sp.GetRequiredService<BlingoRNetClientPlayer>();
            player.ConnectAsync(hubUrl, hello).GetAwaiter().GetResult();
        });

        return reg;
    }
}

