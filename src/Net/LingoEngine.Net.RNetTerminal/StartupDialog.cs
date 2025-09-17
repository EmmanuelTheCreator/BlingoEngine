using Terminal.Gui;

namespace LingoEngine.Net.RNetTerminal
{
    internal enum StartupSelectionMode
    {
        Standalone,
        Http,
        Pipe
    }

    internal readonly record struct StartupDialogResult(StartupSelectionMode Mode, int Port);

    internal sealed class StartupDialog
    {
        private int _port;
        private readonly RNetTerminalTransport _defaultTransport;

        public StartupDialog(int port, RNetTerminalTransport defaultTransport)
        {
            _port = port;
            _defaultTransport = defaultTransport;
        }

        public StartupDialogResult Show()
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

            var standalone = new Button("Run Standalone");
            var connectHttp = new Button("Connect via HTTP", _defaultTransport == RNetTerminalTransport.Http);
            var connectPipe = new Button("Connect via Pipe", _defaultTransport == RNetTerminalTransport.Pipe);
            var dialog = new Dialog("", 80, 24, standalone, connectHttp, connectPipe)
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

            var result = new StartupDialogResult(StartupSelectionMode.Standalone, _port);

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

            standalone.Clicked += () =>
            {
                if (int.TryParse(portField.Text.ToString(), out var p))
                {
                    _port = p;
                }

                result = new StartupDialogResult(StartupSelectionMode.Standalone, _port);
                dialog.Running = false;
            };
            connectHttp.Clicked += () =>
            {
                if (int.TryParse(portField.Text.ToString(), out var p))
                {
                    _port = p;
                }

                result = new StartupDialogResult(StartupSelectionMode.Http, _port);
                dialog.Running = false;
            };
            connectPipe.Clicked += () =>
            {
                if (int.TryParse(portField.Text.ToString(), out var p))
                {
                    _port = p;
                }

                result = new StartupDialogResult(StartupSelectionMode.Pipe, _port);
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

            if (_defaultTransport == RNetTerminalTransport.Pipe)
            {
                connectPipe.SetFocus();
            }
            else
            {
                connectHttp.SetFocus();
            }

            Application.Run(dialog);
            return result;
        }
    }
}
