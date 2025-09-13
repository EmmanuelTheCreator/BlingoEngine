using LingoEngine.Director.Client;
using LingoEngine.Director.Contracts;
using Terminal.Gui;
using Timer = System.Timers.Timer;

namespace LingoEngine.Director.Client.ConsoleTest;

public sealed class DirectorConsoleClient : IAsyncDisposable
{
    private DirectorClient? _client;
    private readonly List<string> _logs = new();
    private CancellationTokenSource _cts = new();
    private Timer? _heartbeatTimer;
    private int _port = 61699;
    private bool _connected;
    private ListView? _logList;
    private readonly string[] _movieInputs = { "Greeting", "Info", "Box" };

    public DirectorConsoleClient()
    {
    }

    public Task RunAsync()
    {
        Application.Init();
        BuildUi();
        Application.Run();
        Application.Shutdown();
        return Task.CompletedTask;
    }

    private void BuildUi()
    {
        var top = Application.Top;

        var menu = new MenuBar(new[]
        {
            new MenuBarItem("_Host", new[]
            {
                new MenuItem("_Connect/Disconnect", string.Empty, async () => await ToggleConnectionAsync()),
                new MenuItem("_Host Port", string.Empty, SetPort),
                new MenuItem("_Quit", string.Empty, () => Application.RequestStop())
            }),
            new MenuBarItem("_Edit", Array.Empty<MenuItem>()),
            new MenuBarItem("_Window", new[]
            {
                new MenuItem("_Score", string.Empty, ShowScoreMenu),
                new MenuItem("_Stage", string.Empty, () => MessageBox.Query("Stage", "Not implemented.", "Ok")),
                new MenuItem("_Property Window", string.Empty, () => MessageBox.Query("Property", "Not implemented.", "Ok"))
            }),
            new MenuBarItem("_Help", Array.Empty<MenuItem>())
        });

        top.Add(menu);

        var uiWin = new Window("UI")
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(70),
            Height = Dim.Fill()
        };
        top.Add(uiWin);

        var logWin = new Window("Logs")
        {
            X = Pos.Percent(70),
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _logList = new ListView(_logs)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        logWin.Add(_logList);
        top.Add(logWin);
    }

    private void ShowScoreMenu()
    {
        var list = new ListView(_movieInputs)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        var close = new Button("Close");
        close.Clicked += () => Application.RequestStop();
        var dialog = new Dialog("Score", 40, 10, close);
        dialog.Add(list);
        Application.Run(dialog);
    }

    private async Task ToggleConnectionAsync()
    {
        if (!_connected)
        {
            _client = new DirectorClient();
            var hubUrl = new Uri($"http://localhost:{_port}/director");
            await _client.ConnectAsync(hubUrl, new HelloDto("test-project", "console", "1.0"));
            _connected = true;
            Log("Connected.");
            _cts = new CancellationTokenSource();
            _ = Task.Run(ReceiveFramesAsync);
            _heartbeatTimer = new Timer(1000);
            _heartbeatTimer.Elapsed += async (_, _) => await _client.SendHeartbeatAsync();
            _heartbeatTimer.AutoReset = true;
            _heartbeatTimer.Start();
        }
        else
        {
            _cts.Cancel();
            _heartbeatTimer?.Stop();
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
            if (_client != null)
            {
                await _client.DisposeAsync();
                _client = null;
            }
            _connected = false;
            Log("Disconnected.");
        }
    }

    private void SetPort()
    {
        var portField = new TextField(_port.ToString())
        {
            X = 1,
            Y = 1,
            Width = 10
        };
        var ok = new Button("Ok", is_default: true);
        ok.Clicked += () =>
        {
            if (int.TryParse(portField.Text.ToString(), out var p))
            {
                _port = p;
                Log($"Port set to {_port}.");
            }
            Application.RequestStop();
        };
        var dialog = new Dialog("Host Port", 25, 7, ok);
        dialog.Add(new Label("Port:") { X = 1, Y = 1 }, portField);
        Application.Run(dialog);
    }

    private async Task ReceiveFramesAsync()
    {
        try
        {
            await foreach (var frame in _client!.StreamFramesAsync(_cts.Token))
            {
                Log($"Frame {frame.FrameId}");
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void Log(string message)
    {
        void AddLog()
        {
            _logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            if (_logs.Count > 100)
            {
                _logs.RemoveAt(0);
            }
            _logList?.SetSource(_logs.ToList());
            _logList?.MoveEnd();
        }

        if (Application.MainLoop is { } loop)
        {
            loop.Invoke(AddLog);
        }
        else
        {
            AddLog();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _heartbeatTimer?.Dispose();
        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}

