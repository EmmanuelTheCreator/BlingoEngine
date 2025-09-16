using LingoEngine.Net.RNetContracts;
using LingoEngine.Net.RNetTerminal.Datas;
using LingoEngine.Net.RNetTerminal.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace LingoEngine.Net.RNetTerminal.Views
{
    internal class RootWindow
    {
       
        private readonly List<string> _logs = new();
       
        private bool _connected => LingoRNetTerminal.IsConnected;
        private ListView? _logList = new ListView();
        private View? _mainArea;
        private View? _logWindow;
        private PropertyInspector? _propertyInspector;
        private Label? _connectionStatusLabel;
        private Button? _logToggleButton;
        private bool _logsCollapsed;
        private Label? _infoItem;

        private const int _propertyInspectorWidth = 28;
        private const int _stageHeight = 12;
        private const int _castHeight = 12;
        private int _logExpandedWidth = 40;
        private Action<int> _setPort = p => { };
        //private View _leftZone;
        private Tile _leftTopZone;
        private ScoreView _score;
        private StageView _stage;
        private CastView _cast;
        private PropertyInspector _propInsp;
        private MenuItemv2 _stageBtn;
        private MenuItemv2 _castBtn;

        private int _port => LingoRNetTerminal.Port;
        public RootWindow()
        {
           // _leftZone = new View();
        }


        public void BuildUi(Func<RNetCommand, CancellationToken?, Task> sendCommandAsync, Func<Task> toggleConnectionAsync, Action<int> setPort)
        {
            _setPort = setPort;
            var top = new Window
            {
                BorderStyle = LineStyle.None,
            }; // Application.Top;
            top.Border!.Width = 0;
            var menu = new MenuBarv2(new[]
            {
                new MenuBarItemv2("_Host", new[]
                {
                    NewMenuItemv2("_Connect/Disconnect", string.Empty, async () => await toggleConnectionAsync()),
                    NewMenuItemv2("_Host Port", string.Empty, SetPort),
                    NewMenuItemv2("_Quit", string.Empty, () => Application.RequestStop())
                }),
                //new MenuBarItemv2("_Edit", System.Array.Empty<MenuItemv2>()),
                new MenuBarItemv2("_Help", Array.Empty<MenuItemv2>())
            });
            _stageBtn = NewMenuItemv2("_Stage", string.Empty, () => SwitchToStageMode());
            _castBtn = NewMenuItemv2("_Cast", string.Empty, () => SwitchToCastMode());
            _stageBtn.X = Pos.Absolute(15);
            _castBtn.X = Pos.Absolute(15);
            top.Add(menu);
            top.Add(_stageBtn);
            top.Add(_castBtn);
            RNetTerminalStyle.SetMenuSchema(menu);
            

            _connectionStatusLabel = RUI.NewLabel(string.Empty, Pos.AnchorEnd(15), 0, 15);
            _connectionStatusLabel.TextAlignment = Alignment.End;
            _connectionStatusLabel.SetScheme(RNetTerminalStyle.MenuScheme);

            top.Add(_connectionStatusLabel);

            var tv = new TileView
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()!-1,
                Orientation = Orientation.Vertical,
                BorderStyle = LineStyle.None
            };
            RNetTerminalStyle.SetTileViewSchema(tv);
            tv.Border!.Width = 0;
            tv.Border.Visible = false;
            tv.TrySplitTile(0, 2, out var tvLeft);
            tv.TrySplitTile(1, 2, out var tv2);

            _score = BuildScoreWindow();
            _stage = BuildStageWindow();
            _cast = BuildCastWindow();
            _propInsp = CreatePropertyInspector(sendCommandAsync);
            var logs = CreateLog();


            _stage.Visible = true;
            _cast.Visible = false;

            tv.LineStyle = LineStyle.Single;
            tv2.LineStyle = LineStyle.Single;
            tv2.Tiles.ElementAt(0).Title = "Property Inspector";
            tv2.Tiles.ElementAt(0).ContentView!.Add(_propInsp);
            tv2.Tiles.ElementAt(1).Title = "Log";
            tv2.Tiles.ElementAt(1).ContentView!.Add(logs);

            tvLeft.Orientation = Orientation.Horizontal;
            _leftTopZone = tvLeft.Tiles.ElementAt(0);
            _leftTopZone.Title = "Stage";
            _leftTopZone.ContentView!.Add(_stage);
            tvLeft.Tiles.ElementAt(1).Title = "Score";
            tvLeft.Tiles.ElementAt(1).ContentView!.Add(_score);


            RNetTerminalStyle.SetTileViewSchema(tv);
            top.Add(tv);

            _infoItem = new Label { Text = "Frame:0 Channel:0 Sprite:- Member:" , Y= Pos.Align(Alignment.End)};
            RNetTerminalStyle.SetStatusBar(_infoItem);
            top.Add(_infoItem);

            UpdateConnectionStatus();
            Application.Run(top);
            top.Dispose();
        }

        private MenuItemv2 NewMenuItemv2(string text, string helperText, Action action)
        {
            var menuItem = new MenuItemv2(text, helperText, action);
            menuItem.SetScheme(RNetTerminalStyle.MenuScheme);
            return menuItem;
        }

        private ScoreView BuildScoreWindow()
        {
            var scoreView = new ScoreView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            scoreView.PlayFromHere += f => Log($"Play from {f}");
            scoreView.InfoChanged += (f, ch, sp, mem) =>
            {
                UpdateInfo(f, ch, sp, mem);
                TerminalDataStore.Instance.SetFrame(f);
            };
            return scoreView;
        }

        private StageView BuildStageWindow()
        {
            var stageView = new StageView();
            stageView.X = 0;
            stageView.Y = 0;
            stageView.Width = Dim.Fill();
            stageView.Height = Dim.Fill();
            return stageView;
        }

        private CastView BuildCastWindow()
        {
            var castView = new CastView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            castView.MemberSelected += m =>
            {
                Log($"memberSelected {m.Name}");
                _propertyInspector?.ShowMember(m);
            };
            return castView;
        }

        private PropertyInspector CreatePropertyInspector(Func<RNetCommand, CancellationToken?, Task> sendCommandAsync)
        {
            //_propertyWindow = RUI.NewWindow("Properties", Pos.AnchorEnd(_propertyInspectorWidth + _logExpandedWidth), 1, _propertyInspectorWidth, Dim.Fill() - 1);

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
                if (target == PropertyTarget.Sprite)
                {
                    var sel = store.GetSelectedSprite();
                    if (sel.HasValue)
                    {
                        _ = sendCommandAsync(new SetSpritePropCmd(sel.Value.SpriteNum, sel.Value.BeginFrame, n, v), null);
                    }
                }
                else if (target == PropertyTarget.Member && _propertyInspector?.CurrentMember != null)
                {
                    var member = _propertyInspector.CurrentMember;
                    _ = sendCommandAsync(new SetMemberPropCmd(member.CastLibNum, member.NumberInCast, n, v), null);
                }
            };
            //_propertyInspector.KeyDown += (_, args) =>
            //{
            //    if (args.KeyCode == Key.Tab)
            //    {
            //        if (_stage?.Visible == true)
            //        {
            //            _score?.SetFocus();
            //        }
            //        else if (_cast?.Visible == true)
            //        {
            //            _cast?.SetFocus();
            //        }
            //        else
            //        {
            //            _score?.SetFocus();
            //        }
            //        args.Handled = true;
            //    }
            //};
            return _propertyInspector;
        }
        private View CreateLog()
{

            _logWindow = new View { X = Pos.AnchorEnd(_logExpandedWidth), Y = 1, Width = _logExpandedWidth, Height = Dim.Fill() };

            //_logToggleButton = RUI.NewButton("<", false, () => ToggleLogs());
            //_logToggleButton.X = Pos.AnchorEnd(3);
            //_logToggleButton.Y = 0;
            //_logToggleButton.CanFocus = false;
            _logList = RUI.NewListView(_logs, 0, 1, Dim.Fill(), Dim.Fill());
            //_logWindow.Add(_logToggleButton, _logList);
            return _logWindow;
        }

      

        private void ToggleLogs()
        {
            _logsCollapsed = !_logsCollapsed;
            _logWindow.Width = _logsCollapsed ? 3 : _logExpandedWidth;
        }

        private void SwitchToStageMode()
        {
            if (_stage == null || _score == null)
                return;
            if (_stage.Visible) return;

            _stage.Visible = true;
            _cast.Visible = false;
            _stageBtn.Visible = false;
            _castBtn.Visible = true;
            _leftTopZone.ContentView!.Remove(_cast);
            _leftTopZone.ContentView!.Add(_stage);
            _score.SetFocus();
            _score.TriggerInfo();
            _leftTopZone.Title = _stage.Visible ? "Stage" : "Cast";
            _stage.Draw();
          
        }

        private void SwitchToCastMode()
        {
            if (_cast == null || _score == null)
                return;
            if (_cast.Visible) return;
            _cast.Visible = true;
            _stage.Visible = false;
            _stageBtn.Visible = true;
            _castBtn.Visible = false;
            _leftTopZone.ContentView!.Remove(_stage);
            _leftTopZone.ContentView!.Add(_cast);
            _cast.SetFocus();
            _score.TriggerInfo();
            _leftTopZone.Title = _stage.Visible ? "Stage" : "Cast";
        }

        public void UpdateConnectionStatus()
        {
            if (_connectionStatusLabel == null)
                return;
            
            _connectionStatusLabel.Text = _connected ? "Connected" : "Disconnected";
            _connectionStatusLabel.SetNeedsDraw();
        }

        public void UpdateInfo(int frame, int channel, SpriteRef? sprite, MemberRef? member)
        {
            if (_infoItem != null)
            {
                var store = TerminalDataStore.Instance;
                var memName = member.HasValue ? store.FindMember(member.Value.CastLibNum, member.Value.MemberNum)?.Name : null;
                _infoItem.Title = $"Frame:{frame} Channel:{channel} Sprite:{sprite?.SpriteNum.ToString() ?? "-"} Member:{memName ?? string.Empty}";
            }
            _score?.SetFocus();
        }

      

        private void SetPort()
        {
            var dialog = PortDialog.Create(_port, p =>
            {
                Log($"Port set to {p}.");
                _setPort(p);
            });
            Application.Run(dialog);
        }



        public void Log(string message)
        {
            void AddLog()
            {
                _logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
                if (_logs.Count > 100)
                {
                    _logs.RemoveAt(0);
                }
                if (_logList != null)
                {
                    _logList.SetSource(new System.Collections.ObjectModel.ObservableCollection<string>(_logs));
                    _logList.MoveEnd();
                }
            }

            Application.AddTimeout(TimeSpan.Zero, () =>
            {
                AddLog();
                return false; // do not repeat
            });
        }

        internal void SetPlayFrame(int f)
        {
            _score?.SetPlayFrame(f);
        }
    }
}
