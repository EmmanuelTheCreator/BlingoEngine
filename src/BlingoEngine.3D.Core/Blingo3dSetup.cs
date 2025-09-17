using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.L3D.Core;

/// <summary>
/// Extension helpers to register the 3D engine services.
/// </summary>
public static class Blingo3dSetup
{
    public static IBlingoEngineRegistration WithBlingo3d(this IBlingoEngineRegistration reg)
    {
        //reg.ServicesMain(s => s.AddSingleton<IBlingoFrameworkFactory, Blingo3dFactory>());
        return reg;
    }
}

