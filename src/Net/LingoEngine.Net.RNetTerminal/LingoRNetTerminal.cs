using LingoEngine.IO.Data.DTO;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Net.RNetProjectClient;
using LingoEngine.Net.RNetTerminal.Datas;
using LingoEngine.Net.RNetTerminal.Dialogs;
using LingoEngine.Net.RNetTerminal.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
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
    private readonly RNetTerminalConnection _connection;
    private RNetTerminalConnectionOptions _connectionOptions;
    private readonly List<string> _logs = new();
    private readonly RNetTerminalSettings _settings;
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


    private bool IsRemoteMode => _connection.IsConnected;

    public LingoRNetTerminal()
    {
        _settings = RNetTerminalSettings.Load();
        _connectionOptions = new RNetTerminalConnectionOptions(_settings.Port, _settings.PreferredTransport);
        _connection = new RNetTerminalConnection();
        _connection.ConnectionStateChanged += OnConnectionStateChanged;
        _connection.PlayFrameReceived += OnPlayFrameReceived;
        _connection.LogMessage += Log;
    }

    public Task RunAsync()
    {
        Application.Init();
        SetMyTheme();
        var startupSelection = new StartupDialog(_connectionOptions.Port, _connectionOptions.Transport).Show();
        var transport = startupSelection.Mode switch
        {
            StartupSelectionMode.Http => RNetTerminalTransport.Http,
            StartupSelectionMode.Pipe => RNetTerminalTransport.Pipe,
            _ => _connectionOptions.Transport
        };
        _connectionOptions = _connectionOptions with { Port = startupSelection.Port, Transport = transport };
        SaveSettings();

        if (startupSelection.Mode is StartupSelectionMode.Http or StartupSelectionMode.Pipe)
        {
            var options = _connectionOptions;
            _ = Task.Run(async () =>
            {
#if DEBUG
                await Task.Delay(300).ConfigureAwait(false);
#else
                await Task.Delay(100).ConfigureAwait(false);
#endif
                try
                {
                    await _connection.ConnectAsync(options).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log($"Connection failed: {ex.Message}");
                }
            });
        }
        else
        {
            TerminalDataStore.Instance.LoadTestData();
        }
        Application.Begin(new Toplevel());
        BuildUi();
        Application.Run();
        Application.Shutdown();
        return Task.CompletedTask;
    }
    public Task SendCommandAsync(RNetCommand cmd, CancellationToken? ct = default)
    {
       if (_client == null) return Task.CompletedTask;
        return ct != null? _client.SendCommandAsync(cmd, ct.Value) : _client.SendCommandAsync(cmd);
                  
    }

    private void UpdateLocalChangeMode()
    {
        var remote = IsRemoteMode;
        var store = TerminalDataStore.Instance;
        store.ApplyLocalChanges = !remote;
        if (_propertyInspector != null)
        {
            _propertyInspector.DelayPropertyUpdates = remote;
        }
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
            var store = TerminalDataStore.Instance;
            var previous = store.GetFrame();
            UpdateInfo(f, ch, sp, mem);
            store.SetFrame(f);
            if (IsRemoteMode && f != previous)
            {
                _connection.QueueGoToFrameCommand(f);
            }
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
            var remote = IsRemoteMode;
            if (!remote)
            {
                store.PropertyHasChanged(target, n, v, _propertyInspector?.CurrentMember);
            }
            else
            {
                if (target == PropertyTarget.Sprite)
                {
                    var sel = store.GetSelectedSprite();
                    if (sel.HasValue)
                    {
                        _connection.QueueSpritePropertyChange(sel.Value, n, v);
                    }
                }
                else if (target == PropertyTarget.Member && _propertyInspector?.CurrentMember != null)
                {
                    var member = _propertyInspector.CurrentMember;
                    _connection.QueueMemberPropertyChange(member.CastLibNum, member.NumberInCast, n, v);
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

        UpdateLocalChangeMode();
    }
    private void SaveSettings()
    {
        _settings.Port = _connectionOptions.Port;
        _settings.PreferredTransport = _connectionOptions.Transport;
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

        var state = _connection.ConnectionState;
        var text = state switch
        {
            LingoNetConnectionState.Connected => "Connected",
            LingoNetConnectionState.Connecting => "Connecting",
            _ => "Disconnected"
        };

        _connectionStatusLabel.Text = text;
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
        try
        {
            if (!_connection.IsConnected)
            {
                await _connection.ConnectAsync(_connectionOptions).ConfigureAwait(false);
            }
            else
            {
                await _connection.DisconnectAsync().ConfigureAwait(false);
            }
        }
        catch (System.Exception ex)
        {
            Log($"Connection error: {ex.Message}");
        }
    }

    private void SetPort()
    {
        var portField = new TextField(_connectionOptions.Port.ToString())
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
                _connectionOptions = _connectionOptions with { Port = p };
                SaveSettings();
                Log($"Port set to {_connectionOptions.Port}.");
            }
            Application.RequestStop();
        };
        var dialog = new Dialog("Host Port", 25, 7, ok);
        dialog.Add(new Label("Port:") { X = 1, Y = 1 }, portField);
        portField.SetFocus();
        Application.Run(dialog);
    }

    private void OnConnectionStateChanged(LingoNetConnectionState state)
    {
        void Apply()
        {
            UpdateConnectionStatus();
            UpdateLocalChangeMode();
        }

        if (Application.MainLoop is { } loop)
        {
            loop.Invoke(Apply);
        }
        else
        {
            Apply();
        }
    }

    private void OnPlayFrameReceived(int frame)
    {
        void Apply() => _scoreView?.SetPlayFrame(frame);

        if (Application.MainLoop is { } loop)
        {
            loop.Invoke(Apply);
        }
        else
        {
            Apply();
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
        _connection.ConnectionStateChanged -= OnConnectionStateChanged;
        _connection.PlayFrameReceived -= OnPlayFrameReceived;
        _connection.LogMessage -= Log;
        await _connection.DisposeAsync().ConfigureAwait(false);
    }
}

