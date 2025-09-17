using System;
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace LingoEngine.Net.RNetTerminal.Dialogs
{
    internal class PortDialog
    {
        public static Dialog Create(int port, Action<int> setPort)
        {
            var portField = RUI.NewTextField(port.ToString(), 1, 1, 10);
            var ok = RUI.NewButton("Ok", true, () =>
            {
                if (int.TryParse(portField.Text.ToString(), out var p))
                {
                    port = p;
                    setPort(p);
                }
                Application.RequestStop();
            });
            var dialog = RUI.NewDialog("Host Port", 25, 7, ok);
            dialog.Add(RUI.NewLabel("Port:", 1, 1), portField);
            portField.SetFocus();
            RNetTerminalStyle.SetForDialog(dialog);
            return dialog;
        }
    }
}
