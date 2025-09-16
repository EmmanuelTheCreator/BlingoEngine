using System.Linq;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace LingoEngine.Net.RNetTerminal.Dialogs
{
    internal class StartupDialog
    {
        private int _port;

        public int Port => _port;
        public StartupDialog(int port)
        {
            _port = port;
        }

        public void Show(System.Action<int> doConnect)
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

            
            var dialog = RUI.NewDialog("", 80, 24);
            dialog.SetScheme(
                new Scheme
                {
                    Normal = new Attribute(Color.White, Color.Black),
                    Focus = new Attribute(Color.Black, Color.Gray),
                    HotNormal = new Attribute(Color.BrightYellow, Color.Black),
                    HotFocus = new Attribute(Color.Black, Color.Gray),
                    Disabled = new Attribute(Color.Gray, Color.Black),
                }
            );

            var title = RUI.NewLabel("LingoEngine", Pos.Center(), 1);
            dialog.Add(title);

            var artLines = asciiArt.Split('\n');
            var artWidth = artLines.Max(l => l.Length);
            var ascii = RUI.NewLabel(asciiArt,12,Pos.Bottom(title),artWidth,artLines.Length);
            dialog.Add(ascii);

            var credit = RUI.NewLabel("By Emmanuel The Creator", Pos.Center(), Pos.AnchorEnd(7));
            dialog.Add(credit);

            var portLabel = RUI.NewLabel("Port:",Pos.AnchorEnd() - 29,Pos.AnchorEnd(4));
            var portField = RUI.NewTextField(_port.ToString(), Pos.AnchorEnd() - 18, Pos.AnchorEnd(4), 10);
            dialog.Add(portLabel, portField);
            var standalone = RUI.NewButton("Run Standalone", false, () => dialog.Running = false);
            var connect = RUI.NewButton("Connect",true,() =>
            {
                if (int.TryParse(portField.Text.ToString(), out var p))
                    _port = p;
                doConnect(_port);
                dialog.Running = false;
            });
            dialog.KeyDown += (_,e) =>
            {
                if (e.KeyCode == Key.Esc)
                {
                    dialog.Running = false;
                    e.Handled = true;
                }
            };
            dialog.AddButton(connect);
            dialog.AddButton(standalone);
            connect.SetFocus();
            var wind = new Window()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.None,
            };
            RNetTerminalStyle.SetForDialog(dialog);
            Application.Run(dialog);
        }
    }
}
