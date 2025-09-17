using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace BlingoEngine.Net.RNetTerminal.Dialogs
{
    internal class AskForIntDialog
    {
        public static int? Prompt(string title, string prompt, int startValue = 0)
        {
            int? result = null;
            var field = new TextField() { Text = startValue.ToString(), X = 12, Y = 1, Width = 10 };
            var ok = RUI.NewButton("Ok", true, () =>
            {
                if (int.TryParse(field.Text.ToString(), out var v))
                {
                    result = v;
                }
                Application.RequestStop();
            });
            var dialog = RUI.NewDialog(title, 30, 7, ok);
            dialog.Add(new Label { Text = prompt, X = 1, Y = 1 }, field);
            field.SetFocus();
            Application.Run(dialog);
            return result;
        }
    }
}

