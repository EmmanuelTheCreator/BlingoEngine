using System.Collections.Concurrent;
using LingoEngine.Director.Client;
using LingoEngine.Director.Contracts;
using Spectre.Console;

namespace LingoEngine.Director.Client.ConsoleTest;

public sealed class DirectorConsoleClient : IAsyncDisposable
{
    private readonly DirectorClient _client = new();
    private readonly ConcurrentQueue<string> _logs = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Layout _layout;
    private bool _running = true;
    private MenuState _menu = MenuState.Root;

    public DirectorConsoleClient()
    {
        _layout = new Layout("Root")
            .SplitColumns(
                new Layout("Menu").Ratio(1),
                new Layout("Logs").Ratio(2));
    }

    public async Task RunAsync()
    {
        var hubUrl = new Uri("http://localhost:61699/director");
        await _client.ConnectAsync(hubUrl, new HelloDto("test-project", "console", "1.0"));
        _ = Task.Run(ReceiveFramesAsync);

        AnsiConsole.Cursor.Hide();
        await AnsiConsole.Live(_layout).StartAsync(async ctx =>
        {
            while (_running)
            {
                Render();
                ctx.Refresh();
                await HandleInputAsync();
                await Task.Delay(100);
            }
        });
        AnsiConsole.Cursor.Show();
    }

    private async Task ReceiveFramesAsync()
    {
        try
        {
            await foreach (var frame in _client.StreamFramesAsync(_cts.Token))
            {
                Log($"Frame {frame.FrameId}");
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task HandleInputAsync()
    {
        if (!Console.KeyAvailable)
        {
            return;
        }

        var key = Console.ReadKey(true);
        switch (_menu)
        {
            case MenuState.Root:
                await HandleRootInputAsync(key.Key);
                break;
            case MenuState.MovieControl:
                await HandleMovieInputAsync(key.Key);
                break;
        }
    }

    private void Render()
    {
        var menuText = _menu switch
        {
            MenuState.Root =>
                "[bold]Director Client Test[/]\n\n" +
                "[1] Send ping\n" +
                "[2] Send pause command\n" +
                "[3] Movie controls\n" +
                "[Q] Quit\n",
            MenuState.MovieControl =>
                "[bold]Movie Controls[/]\n\n" +
                "[1] Play\n" +
                "[2] Stop\n" +
                "[3] Go to frame\n" +
                "[B] Back\n",
            _ => string.Empty
        };

        _layout["Menu"].Update(new Panel(menuText).Border(BoxBorder.Rounded).Header("Menu"));
        _layout["Logs"].Update(new Panel(string.Join('\n', _logs)).Border(BoxBorder.Rounded).Header("Logs"));
    }

    private async Task HandleRootInputAsync(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.D1:
            case ConsoleKey.NumPad1:
                await _client.SendHeartbeatAsync();
                Log("Sent heartbeat.");
                break;
            case ConsoleKey.D2:
            case ConsoleKey.NumPad2:
                await _client.SendCommandAsync(new PauseCmd());
                Log("Sent PauseCmd.");
                break;
            case ConsoleKey.D3:
            case ConsoleKey.NumPad3:
                _menu = MenuState.MovieControl;
                break;
            case ConsoleKey.Q:
            case ConsoleKey.Escape:
                _running = false;
                break;
        }
    }

    private async Task HandleMovieInputAsync(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.D1:
            case ConsoleKey.NumPad1:
                await _client.SendCommandAsync(new ResumeCmd());
                Log("Sent ResumeCmd.");
                break;
            case ConsoleKey.D2:
            case ConsoleKey.NumPad2:
                await _client.SendCommandAsync(new PauseCmd());
                Log("Sent PauseCmd.");
                break;
            case ConsoleKey.D3:
            case ConsoleKey.NumPad3:
                AnsiConsole.Cursor.Show();
                var frame = AnsiConsole.Ask<int>("Target frame:");
                AnsiConsole.Cursor.Hide();
                await _client.SendCommandAsync(new GoToFrameCmd(frame));
                Log($"Sent GoToFrameCmd {frame}.");
                break;
            case ConsoleKey.B:
            case ConsoleKey.Escape:
                _menu = MenuState.Root;
                break;
        }
    }

    private void Log(string message)
    {
        _logs.Enqueue($"[{DateTime.Now:HH:mm:ss}] {message}");
        while (_logs.Count > 20)
        {
            _logs.TryDequeue(out _);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        await _client.DisposeAsync();
    }

    private enum MenuState
    {
        Root,
        MovieControl
    }
}
