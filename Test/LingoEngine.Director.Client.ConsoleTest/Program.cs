namespace LingoEngine.Director.Client.ConsoleTest;

internal static class Program
{
    private static async Task Main()
    {
        await using var app = new DirectorConsoleClient();
        await app.RunAsync();
    }
}
