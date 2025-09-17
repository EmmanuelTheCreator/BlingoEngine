using System;
using System.Threading.Tasks;

namespace BlingoEngine.Net.RNetTerminal;

internal static class Program
{
    private static async Task Main()
    {
        Console.Title = "BlingoEngine Remote Net Terminal";
        await using var app = new BlingoRNetTerminal();
        await app.RunAsync();
    }
}

