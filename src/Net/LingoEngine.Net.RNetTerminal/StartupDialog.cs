using Terminal.Gui;

namespace LingoEngine.Net.RNetTerminal
{
    internal class StartupDialog
    {
        private int _port;

        public int Port => _port;
        public StartupDialog(int port)
        {
            _port = port;
        }

        public void Show(Action<int> doConnect)
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
            connect.Clicked += () =>
            {
                if (int.TryParse(portField.Text.ToString(), out var p))
                    _port = p;
                doConnect(_port);
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
    }
}
