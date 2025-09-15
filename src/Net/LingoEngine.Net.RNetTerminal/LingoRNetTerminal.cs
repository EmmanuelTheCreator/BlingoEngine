using LingoEngine.IO;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Net.RNetClient;
using LingoEngine.Net.RNetContracts;
using System.Net.WebSockets;
using Terminal.Gui;
using Timer = System.Timers.Timer;

namespace LingoEngine.Net.RNetTerminal;

public sealed class LingoRNetTerminal : IAsyncDisposable
{
    private LingoRNetClient? _client;
    private readonly List<string> _logs = new();
    private CancellationTokenSource _cts = new();
    private Timer? _heartbeatTimer;
    private readonly RNetTerminalSettings _settings;
    private int _port;
    private bool _connected;
    private ListView? _logList;
    private Window? _uiWin;
    private View? _workspace;
    private PropertyInspector? _propertyInspector;
    private ScoreView? _scoreView;
    private StageView? _stageView;
    private CastView? _castView;
    private StatusItem? _infoItem;

    public LingoRNetTerminal()
    {
        _settings = RNetTerminalSettings.Load();
        _port = _settings.Port;
    }

    public async Task RunAsync()
    {
        Application.Init();
        SetMyTheme();
        var useConnection = true;
        //new StartupDialog(_port).Show(async p =>
        //{
        //    if (_port != p)
        //        SaveSettings();
        //    _port = p;
        //    useConnection = true;
        //});
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
        BuildUi();
        Application.Run();
        Application.Shutdown();
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
                new MenuItem("_Score", string.Empty, ShowScore),
                new MenuItem("_Cast", string.Empty, ShowCast),
                new MenuItem("_Stage", string.Empty, ShowStage),
                new MenuItem("_Property Window", string.Empty, ShowPropertyInspector)
            }),
            new MenuBarItem("_Help", Array.Empty<MenuItem>())
        });

        top.Add(menu);

        _uiWin = new Window("Score")
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(75),
            Height = Dim.Fill() - 1
        };
        _workspace = new View
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(70),
            Height = Dim.Fill()
        };
        _propertyInspector = new PropertyInspector
        {
            X = Pos.Percent(70),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _propertyInspector.PropertyChanged += (target, n, v) =>
        {
            Log($"propertyChanged {n}={v}");
            var store = TerminalDataStore.Instance;
            store.PropertyHasChanged(target, n, v, _propertyInspector?.CurrentMember);
            if (_client != null)
            {
                if (target == PropertyTarget.Sprite)
                {
                    var sel = store.GetSelectedSprite();
                    if (sel.HasValue)
                    {
                        _ = _client.SendCommandAsync(new SetSpritePropCmd(sel.Value.SpriteNum, sel.Value.BeginFrame, n, v));
                    }
                }
                else if (target == PropertyTarget.Member && _propertyInspector?.CurrentMember != null)
                {
                    var member = _propertyInspector.CurrentMember;
                    _ = _client.SendCommandAsync(new SetMemberPropCmd(member.CastLibNum, member.NumberInCast, n, v));
                }
            }
        };
        _propertyInspector.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Tab && _workspace?.Subviews.Count > 0)
            {
                _workspace.Subviews[0].SetFocus();
                args.Handled = true;
            }
        };
        _uiWin.Add(_workspace, _propertyInspector);
        top.Add(_uiWin);

        var logWin = new Window("Logs")
        {
            X = Pos.Percent(75),
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 1
        };
        _logList = new ListView(_logs)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        logWin.Add(_logList);
        top.Add(logWin);

        _infoItem = new StatusItem(Key.Null, "Frame:0 Channel:0 Sprite:- Member:", null);
        var status = new StatusBar(new[] { _infoItem });
        top.Add(status);

        ShowScore();
        ShowStage();
    }

    private void SaveSettings()
    {
        _settings.Port = _port;
        _settings.Save();
    }

    private void ShowPropertyInspector() => _propertyInspector?.SetFocus();

    private void ShowScore()
    {
        _uiWin!.Title = "Score";
        _workspace?.RemoveAll();
        _scoreView = new ScoreView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        _scoreView.PlayFromHere += f => Log($"Play from {f}");
        _scoreView.InfoChanged += (f, ch, sp, mem) =>
        {
            UpdateInfo(f, ch, sp, mem);
            TerminalDataStore.Instance.SetFrame(f);
        };
        _workspace?.Add(_scoreView);
        _scoreView.SetFocus();
        _scoreView.TriggerInfo();
    }

    private void ShowCast()
    {
        _uiWin!.Title = "Cast";
        _workspace?.RemoveAll();
        _castView = new CastView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _castView.MemberSelected += m =>
        {
            Log($"memberSelected {m.Name}");
            _propertyInspector?.ShowMember(m);
        };
        _workspace?.Add(_castView);
        _castView.SetFocus();
    }

    private void ShowStage()
    {
        _uiWin!.Title = "Stage";
        _workspace?.RemoveAll();

        _stageView = new StageView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(75)
        };

        _scoreView = new ScoreView
        {
            X = 0,
            Y = Pos.Percent(75),
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _scoreView.PlayFromHere += f => Log($"Play from {f}");
        _scoreView.InfoChanged += (f, ch, sp, mem) =>
        {
            UpdateInfo(f, ch, sp, mem);
            TerminalDataStore.Instance.SetFrame(f);
        };

        _workspace?.Add(_stageView, _scoreView);
        _scoreView.SetFocus();
        _scoreView.TriggerInfo();
    }

    private void UpdateInfo(int frame, int channel, SpriteRef? sprite, MemberRef? member)
    {
        if (_infoItem != null)
        {
            var store = TerminalDataStore.Instance;
            var memName = member.HasValue ? store.FindMember(member.Value.CastLibNum, member.Value.MemberNum)?.Name : null;
            _infoItem.Title = $"Frame:{frame} Channel:{channel} Sprite:{(sprite?.SpriteNum.ToString() ?? "-")} Member:{memName ?? string.Empty}";
        }
        _scoreView?.SetFocus();
    }

    private static void SetMyTheme()
    {
        var baseScheme = new ColorScheme
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.BrightBlue),
            Focus = Application.Driver.MakeAttribute(Color.Black, Color.White),
            HotNormal = Application.Driver.MakeAttribute(Color.BrightYellow, Color.BrightBlue),
            HotFocus = Application.Driver.MakeAttribute(Color.Black, Color.White),
            Disabled = Application.Driver.MakeAttribute(Color.Gray, Color.BrightBlue)
        };
        Colors.Base = baseScheme;
        Colors.Dialog = baseScheme;
        Colors.Error = baseScheme;
        var menuScheme = new ColorScheme
        {
            Normal = Application.Driver.MakeAttribute(Color.Black, Color.Gray),
            Focus = Application.Driver.MakeAttribute(Color.Black, Color.White),
            HotNormal = Application.Driver.MakeAttribute(Color.Green, Color.Gray),
            HotFocus = Application.Driver.MakeAttribute(Color.Green, Color.White),
            Disabled = Application.Driver.MakeAttribute(Color.DarkGray, Color.Gray)
        };
        Colors.Menu = menuScheme;
        Colors.TopLevel = baseScheme;
        if (Application.Top is { } top)
        {
            top.ColorScheme = baseScheme;
        }
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
        
    }

    private async Task ConnectToHost()
    {
        _client = new LingoRNetClient();
        // config.ClientName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Someone";
        var hubUrl = new Uri($"http://localhost:{_port}/director");
        try
        {
            await _client.ConnectAsync(hubUrl, new HelloDto("test-project", "console", "1.0", "RNetTerminal"));
            _connected = true;
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
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
        catch (Exception ex)
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
        catch (Exception ex)
        {
            Log($"Load movie failed: {ex.Message}");
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
                SaveSettings();
                Log($"Port set to {_port}.");
            }
            Application.RequestStop();
        };
        var dialog = new Dialog("Host Port", 25, 7, ok);
        dialog.Add(new Label("Port:") { X = 1, Y = 1 }, portField);
        portField.SetFocus();
        Application.Run(dialog);
    }

    private async Task ReceiveFramesAsync()
    {
        try
        {
            await foreach (var frame in _client!.StreamFramesAsync(_cts.Token))
            {
                Log($"Frame {frame.FrameId}");
                var f = (int)frame.FrameId;
                if (Application.MainLoop is { } loop)
                {
                    loop.Invoke(() => _scoreView?.SetPlayFrame(f));
                }
                else
                {
                    _scoreView?.SetPlayFrame(f);
                }
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

