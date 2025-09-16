using LingoEngine.IO;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Movies.Commands;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Net.RNetProjectClient;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Timer = System.Timers.Timer;

namespace LingoEngine.Net.RNetTerminal;

public sealed class LingoRNetTerminal : System.IAsyncDisposable
{
    private readonly RNetTerminalSettings _settings;
    private static int _port;
    private static bool _connected;
    private Timer? _heartbeatTimer;
    private LingoRNetProjectClient? _client;
    public static bool IsConnected => _connected;
    public static int Port => _port;
    private CancellationTokenSource _cts = new();
    private RootWindow _rootWindow = null!;


    public LingoRNetTerminal()
    {
        _settings = RNetTerminalSettings.Load();
        _port = _settings.Port;
    }

    public async Task RunAsync()
    {
        Application.Init();
        RNetTerminalStyle.SetMyTheme();
        _rootWindow = new RootWindow();
        var useConnection = false;
        new StartupDialog(_port).Show(async p =>
        {
            if (_port != p)
                SaveSettings();
            _port = p;
            useConnection = true;
        });
        if (useConnection)
        {
            _ = Task.Run(async () =>
            {
#if DEBUG
                await Task.Delay(300);
#else
                await Task.Delay(100);
#endif
                await ConnectToHost().ConfigureAwait(false);
            });
        }
        else
            TerminalDataStore.Instance.LoadTestData();
        Application.Begin(new Toplevel());
        _rootWindow.BuildUi(SendCommandAsync,ToggleConnectionAsync, SetPort);
        
        Application.Run();
        Application.Shutdown();
    }
    public Task SendCommandAsync(RNetCommand cmd, CancellationToken? ct = default)
    {
       if (_client == null) return Task.CompletedTask;
        return ct != null? _client.SendCommandAsync(cmd, ct.Value) : _client.SendCommandAsync(cmd);
                  
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
    private void SetPort(int port)
    {
        _port = port;
        SaveSettings();
    }
    private void SaveSettings()
    {
        _settings.Port = _port;
        _settings.Save();
    }

    private async Task ToggleConnectionAsync()
    {
        if (!_connected)
            await ConnectToHost();
        else
            await DisconnectFromHost();
    }

    private async Task DisconnectFromHost()
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
        _rootWindow.UpdateConnectionStatus();

    }

    private async Task ConnectToHost()
    {
        _client = new LingoRNetProjectClient();
        // config.ClientName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Someone";
        var hubUrl = new System.Uri($"http://localhost:{_port}/director");
        try
        {
            await _client.ConnectAsync(hubUrl, new HelloDto("test-project", "console", "1.0", "RNetTerminal"));
            _connected = true;
            _rootWindow.UpdateConnectionStatus();
            Log("Connected.");
            _cts = new CancellationTokenSource();
            _ = Task.Run(ReceiveFramesAsync);
            _heartbeatTimer = new Timer(1000);
            System.Timers.ElapsedEventHandler value = async (_, _) => await DoHeartBeat();
            _heartbeatTimer.Elapsed += value;
            _heartbeatTimer.AutoReset = true;
            _heartbeatTimer.Start();
            await LoadProjectDataAsync();
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine(ex);
            throw;
        }

    }
    private async Task DoHeartBeat()
    {
        try
        {
            if (_client == null) return;
            await _client.SendHeartbeatAsync();
        }
        catch (WebSocketException ex)
        {
            if (ex.Message.Contains("The remote party closed the WebSocket connection"))
                await DisconnectFromHost();
            Log("Error sending heartbeat:" + ex.Message);
        }
        catch (System.Exception ex)
        {
            Log("Error sending heartbeat:" + ex.Message);
        }
    }
    private async Task LoadProjectDataAsync()
    {
        if (_client == null)
        {
            return;
        }
        try
        {
            var projectJson = await _client.GetCurrentProjectAsync();
            var repo = new JsonStateRepository();
            var project = repo.DeserializeProject(projectJson.json);
            TerminalDataStore.Instance.LoadFromProject(project);
        }
        catch (System.Exception ex)
        {
            Log($"Load movie failed: {ex.Message}");
        }
    }
    private void Log(string message)
    {
        _rootWindow.Log(message);   
    }

    private async Task ReceiveFramesAsync()
    {
        try
        {
            await foreach (var frame in _client!.StreamFramesAsync(_cts.Token))
            {
                //Log($"Frame {frame.FrameId}");
                var f = (int)frame.FrameId;

                Application.AddTimeout(System.TimeSpan.Zero, () =>
                {
                    _rootWindow.SetPlayFrame(f);
                    
                    return false; // do not repeat
                });
            }
        }
        catch (System.OperationCanceledException)
        {
        }
    }
}

