using LingoEngine.IO.Data.DTO;
using LingoEngine.Net.RNetClient;
using LingoEngine.Net.RNetContracts;
using Terminal.Gui;
using System.Linq;
using Timer = System.Timers.Timer;
using LingoEngine.IO;

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
    private int? _selectedSprite;

    public LingoRNetTerminal()
    {
        _settings = RNetTerminalSettings.Load();
        _port = _settings.Port;
    }

    public Task RunAsync()
    {
        Application.Init();
        SetNortonTheme();
        ShowStartupDialog();
        Application.Begin(new Toplevel());
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
        _propertyInspector.PropertyChanged += (n, v) =>
        {
            Log($"propertyChanged {n}={v}");
            if (_selectedSprite.HasValue && _client != null)
            {
                _ = _client.SendCommandAsync(new SetSpritePropCmd(_selectedSprite.Value, n, v));
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
    }

    private void ShowStartupDialog()
    {
        const string asciiArt = @" ____                      _         _   _      _
|  _ \ ___ _ __ ___   ___ | |_ ___  | \ | | ___| |_
| |_) / _ \ '_ ` _ \ / _ \| __/ _ \ |  \| |/ _ \ __|
|  _ <  __/ | | | | | (_) | ||  __/ | |\  |  __/ |_
|_| \_\___|_| |_| |_|\___/ \__\___| |_| \_|\___|\__|
 _____                   _             _
|_   _|__ _ __ _ __ ___ (_)_ __   __ _| |
  | |/ _ \ '__| '_ ` _ \| | '_ \ / _` | |
  | |  __/ |  | | | | | | | | | | (_| | |
  |_|\___|_|  |_| |_| |_|_|_| |_|\__,_|_|";

        var standalone = new Button("Run Standalone", true);
        var connect = new Button("Connect");
        var dialog = new Dialog("LingoEngine Remote Net Terminal", 80, 24, standalone, connect)
        {
            ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.Black, Color.Gray),
                HotNormal = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
                HotFocus = Application.Driver.MakeAttribute(Color.Black, Color.Gray),
                Disabled = Application.Driver.MakeAttribute(Color.Gray, Color.Black)
            }
        };

        var title = new Label("LingoEngine")
        {
            X = Pos.Center(),
            Y = 1
        };
        dialog.Add(title);

        var artLines = asciiArt.Split('\n');
        var artWidth = artLines.Max(l => l.Length);
        var ascii = new Label(asciiArt)
        {
            X = Pos.Center() - artWidth / 2,
            Y = Pos.Bottom(title),
            Width = artWidth,
            Height = artLines.Length
        };
        dialog.Add(ascii);

        var credit = new Label("By Emmanuel The Creator")
        {
            X = Pos.Center(),
            Y = Pos.AnchorEnd(7)
        };
        dialog.Add(credit);

        var portLabel = new Label("Port:")
        {
            X = Pos.Center() - 14,
            Y = Pos.AnchorEnd(4)
        };
        var portField = new TextField(_port.ToString())
        {
            X = Pos.Center() - 9,
            Y = Pos.AnchorEnd(4),
            Width = 10
        };
        dialog.Add(portLabel, portField);

        standalone.Clicked += () => dialog.Running = false;
        connect.Clicked += async () =>
        {
            if (int.TryParse(portField.Text.ToString(), out var p))
            {
                _port = p;
                SaveSettings();
            }
            await ToggleConnectionAsync();
            dialog.Running = false;
        };
        dialog.KeyPress += e =>
        {
            if (e.KeyEvent.Key == Key.Esc)
            {
                dialog.Running = false;
                e.Handled = true;
            }
        };
        portField.SetFocus();
        Application.Run(dialog);
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
        _scoreView.SpriteSelected += n =>
        {
            Log($"spriteSelected {n}");
            _selectedSprite = n;
        };
        _scoreView.PlayFromHere += f => Log($"Play from {f}");
        _scoreView.InfoChanged += UpdateInfo;
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
        _stageView.SpriteSelected += n =>
        {
            Log($"spriteSelected {n}");
            _selectedSprite = n;
            _stageView.SetSelectedSprite(n);
            _scoreView.SelectSprite(n);
        };
        _scoreView.SpriteSelected += n =>
        {
            Log($"spriteSelected {n}");
            _selectedSprite = n;
            _stageView.SetSelectedSprite(n);
            _scoreView.SelectSprite(n);
        };
        _scoreView.PlayFromHere += f => Log($"Play from {f}");
        _scoreView.InfoChanged += (f, ch, sp, mem) =>
        {
            UpdateInfo(f, ch, sp, mem);
            _stageView.SetFrame(f);
            _stageView.RequestRedraw();
        };

        _workspace?.Add(_stageView, _scoreView);
        _scoreView.SetFocus();
        _scoreView.TriggerInfo();
    }

    private void UpdateInfo(int frame, int channel, int? sprite, string? member)
    {
        if (_infoItem != null)
        {
            _infoItem.Title = $"Frame:{frame} Channel:{channel} Sprite:{(sprite?.ToString() ?? "-")} Member:{member ?? string.Empty}";
        }

        if (_propertyInspector != null)
        {
            _propertyInspector.ShowMember(member != null ? FindMember(member) : null);
        }
    }

    private LingoMemberDTO? FindMember(string name)
    {
        foreach (var cast in TerminalDataStore.Instance.Casts.Values)
        {
            foreach (var m in cast)
            {
                if (m.Name == name)
                {
                    return m;
                }
            }
        }
        return null;
    }

    private static void SetNortonTheme()
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
        {
            _client = new LingoRNetClient();
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
            await LoadMovieDataAsync();
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
            TerminalDataStore.Instance.LoadTestData();
            _scoreView?.ReloadData();
            _stageView?.ReloadData();
            _castView?.ReloadData();
        }
    }

    private async Task LoadMovieDataAsync()
    {
        if (_client == null)
        {
            return;
        }
        try
        {
            var movieJson = await _client.GetMovieAsync();
            var repo = new JsonStateRepository();
            var project = repo.Deserialize(movieJson.json);
            TerminalDataStore.Instance.LoadFromProject(project);
            _scoreView?.ReloadData();
            _stageView?.ReloadData();
            _castView?.ReloadData();
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

