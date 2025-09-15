using LingoEngine.IO;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Net.RNetProjectClient;
using LingoEngine.Net.RNetContracts;
using System.Net.WebSockets;
using Terminal.Gui;
using Timer = System.Timers.Timer;

namespace LingoEngine.Net.RNetTerminal;

public sealed class LingoRNetTerminal : IAsyncDisposable
{
    private LingoRNetProjectClient? _client;
    private readonly List<string> _logs = new();
    private CancellationTokenSource _cts = new();
    private Timer? _heartbeatTimer;
    private readonly RNetTerminalSettings _settings;
    private int _port;
    private bool _connected;
    private ListView? _logList;
    private Window? _scoreWindow;
    private Window? _stageWindow;
    private Window? _castWindow;
    private View? _mainArea;
    private Window? _propertyWindow;
    private Window? _logWindow;
    private PropertyInspector? _propertyInspector;
    private ScoreView? _scoreView;
    private StageView? _stageView;
    private CastView? _castView;
    private Label? _connectionStatusLabel;
    private Button? _logToggleButton;
    private bool _logsCollapsed;
    private StatusItem? _infoItem;

    private const int PropertyInspectorWidth = 22;
    private const int StageWindowHeight = 12;
    private const int CastWindowHeight = 12;
    private int _logExpandedWidth = 40;

    public LingoRNetTerminal()
    {
        _settings = RNetTerminalSettings.Load();
        _port = _settings.Port;
    }

    public async Task RunAsync()
    {
        Application.Init();
        SetMyTheme();
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
                new MenuItem("_Stage Mode", string.Empty, () => SwitchToStageMode()),
                new MenuItem("_Cast Mode", string.Empty, () => SwitchToCastMode())
            }),
            new MenuBarItem("_Help", Array.Empty<MenuItem>())
        });

        top.Add(menu);

        _connectionStatusLabel = new Label(string.Empty)
        {
            X = Pos.AnchorEnd(15),
            Y = 0,
            Width = 15,
            TextAlignment = TextAlignment.Right,
            ColorScheme = Colors.Menu
        };
        top.Add(_connectionStatusLabel);

        _mainArea = new View
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(PropertyInspectorWidth + _logExpandedWidth),
            Height = Dim.Fill() - 1
        };

        BuildScoreWindow();
        BuildStageWindow();
        BuildCastWindow();

        _mainArea.Add(_scoreWindow!);
        _mainArea.Add(_stageWindow!);
        _mainArea.Add(_castWindow!);
        top.Add(_mainArea);

        BuildRightPanel(top);

        _infoItem = new StatusItem(Key.Null, "Frame:0 Channel:0 Sprite:- Member:", null);
        var status = new StatusBar(new[] { _infoItem });
        top.Add(status);

        SwitchToStageMode();
        UpdateConnectionStatus();
        UpdateRightPanelLayout();
    }

    private void BuildScoreWindow()
    {
        _scoreWindow = new Window("Score")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _scoreView = new ScoreView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _scoreView.PlayFromHere += f => Log($"Play from {f}");
        _scoreView.InfoChanged += (f, ch, sp, mem) =>
        {
            UpdateInfo(f, ch, sp, mem);
            TerminalDataStore.Instance.SetFrame(f);
        };
        _scoreWindow.Add(_scoreView);
    }

    private void BuildStageWindow()
    {
        _stageWindow = new Window("Stage")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Sized(StageWindowHeight)
        };

        var collapse = new Button("[-]")
        {
            X = Pos.AnchorEnd(4),
            Y = 0,
            Width = 3,
            CanFocus = false
        };
        collapse.Clicked += () => SwitchToCastMode();

        _stageView = new StageView
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _stageWindow.Add(_stageView, collapse);
    }

    private void BuildCastWindow()
    {
        _castWindow = new Window("Cast")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Sized(CastWindowHeight),
            Visible = false
        };

        _castView = new CastView
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _castView.MemberSelected += m =>
        {
            Log($"memberSelected {m.Name}");
            _propertyInspector?.ShowMember(m);
        };

        _castWindow.Add(_castView);
    }

    private void BuildRightPanel(Toplevel top)
    {
        _propertyWindow = new Window("Properties")
        {
            X = Pos.AnchorEnd(PropertyInspectorWidth + _logExpandedWidth),
            Y = 1,
            Width = PropertyInspectorWidth,
            Height = Dim.Fill() - 1
        };

        _propertyInspector = new PropertyInspector
        {
            X = 0,
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
            if (args.KeyEvent.Key == Key.Tab)
            {
                if (_stageWindow?.Visible == true)
                {
                    _scoreView?.SetFocus();
                }
                else if (_castWindow?.Visible == true)
                {
                    _castView?.SetFocus();
                }
                else
                {
                    _scoreView?.SetFocus();
                }
                args.Handled = true;
            }
        };
        _propertyWindow.Add(_propertyInspector);
        top.Add(_propertyWindow);

        _logWindow = new Window("Logs")
        {
            X = Pos.AnchorEnd(_logExpandedWidth),
            Y = 1,
            Width = _logExpandedWidth,
            Height = Dim.Fill() - 1
        };

        _logToggleButton = new Button("<")
        {
            X = Pos.AnchorEnd(3),
            Y = 0,
            Width = 3,
            CanFocus = false
        };
        _logToggleButton.Clicked += ToggleLogs;

        _logList = new ListView(_logs)
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _logWindow.Add(_logToggleButton, _logList);
        top.Add(_logWindow);
    }

    private void SaveSettings()
    {
        _settings.Port = _port;
        _settings.Save();
    }

    private void ToggleLogs()
    {
        _logsCollapsed = !_logsCollapsed;
        UpdateRightPanelLayout();
    }

    private void UpdateRightPanelLayout()
    {
        var logWidth = _logsCollapsed ? 3 : _logExpandedWidth;

        if (_mainArea != null)
        {
            _mainArea.Width = Dim.Fill(PropertyInspectorWidth + logWidth);
        }

        if (_propertyWindow != null)
        {
            _propertyWindow.X = Pos.AnchorEnd(PropertyInspectorWidth + logWidth);
            _propertyWindow.Width = PropertyInspectorWidth;
        }

        if (_logWindow != null)
        {
            _logWindow.X = Pos.AnchorEnd(logWidth);
            _logWindow.Width = logWidth;
        }

        if (_logToggleButton != null)
        {
            _logToggleButton.Text = _logsCollapsed ? ">" : "<";
        }

        if (_logList != null)
        {
            _logList.Visible = !_logsCollapsed;
        }

        UpdateMainAreaLayout();
        _mainArea?.SetNeedsDisplay();
        _propertyWindow?.SetNeedsDisplay();
        _logWindow?.SetNeedsDisplay();
        _scoreWindow?.SetNeedsDisplay();
        _stageWindow?.SetNeedsDisplay();
        _castWindow?.SetNeedsDisplay();
    }

    private void UpdateMainAreaLayout()
    {
        if (_scoreWindow == null)
        {
            return;
        }

        var offset = 0;

        if (_stageWindow != null)
        {
            _stageWindow.Y = 0;
            if (_stageWindow.Visible)
            {
                _stageWindow.Height = Dim.Sized(StageWindowHeight);
                offset = StageWindowHeight;
            }
        }

        if (_castWindow != null)
        {
            _castWindow.Y = 0;
            if (_castWindow.Visible)
            {
                _castWindow.Height = Dim.Sized(CastWindowHeight);
                offset = CastWindowHeight;
            }
        }

        _scoreWindow.Y = offset;
        _scoreWindow.Height = Dim.Fill();
    }

    private void SwitchToStageMode()
    {
        if (_stageWindow == null || _scoreWindow == null)
        {
            return;
        }

        _stageWindow.Visible = true;
        if (_castWindow != null)
        {
            _castWindow.Visible = false;
        }

        UpdateMainAreaLayout();
        _scoreView?.SetFocus();
        _scoreView?.TriggerInfo();
    }

    private void SwitchToCastMode()
    {
        if (_castWindow == null || _scoreWindow == null)
        {
            return;
        }

        _castWindow.Visible = true;
        if (_stageWindow != null)
        {
            _stageWindow.Visible = false;
        }

        UpdateMainAreaLayout();
        if (_castView != null)
        {
            _castView.SetFocus();
        }
        _scoreView?.TriggerInfo();
    }

    private void UpdateConnectionStatus()
    {
        if (_connectionStatusLabel == null)
        {
            return;
        }

        _connectionStatusLabel.Text = _connected ? "Connected" : "Disconnected";
        _connectionStatusLabel.SetNeedsDisplay();
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
        UpdateConnectionStatus();

    }

    private async Task ConnectToHost()
    {
        _client = new LingoRNetProjectClient();
        // config.ClientName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Someone";
        var hubUrl = new Uri($"http://localhost:{_port}/director");
        try
        {
            await _client.ConnectAsync(hubUrl, new HelloDto("test-project", "console", "1.0", "RNetTerminal"));
            _connected = true;
            UpdateConnectionStatus();
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

