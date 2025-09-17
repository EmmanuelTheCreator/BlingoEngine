using System;
using System.Threading.Tasks;

namespace LingoEngine.Net.RNetTerminal;

internal static class Program
{
    private static async Task Main()
    {
        Console.Title = "LingoEngine Remote Net Terminal";
        await using var app = new LingoRNetTerminal();
        await app.RunAsync();
    }
}
