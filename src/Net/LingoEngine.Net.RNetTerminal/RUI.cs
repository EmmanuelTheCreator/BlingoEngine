using System.Data;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace LingoEngine.Net.RNetTerminal
{
    internal static class RUI
    {
        public static Label NewLabel(string text, Pos x, Pos y)
        {
            var label = new Label();
            label.Text = text;
            label.X = x;
            label.Y = y;
            return label;
        }
        public static CheckBox NewCheckBox(string text, bool isChecked = false)
        {
            var element = new CheckBox();
            element.Text = text;
            element.CheckedState = isChecked?CheckState.Checked: CheckState.UnChecked;
            return element;
        }
       
        public static bool Checked(this CheckBox checkBox)
            => checkBox.CheckedState == CheckState.Checked;
        public static CheckBox NewCheckBox(Pos x , Pos y , string text = "", bool isChecked = false)
        {
            var element = new CheckBox();
            element.Text = text;
            element.X = x;
            element.Y = y;
            element.CheckedState = isChecked?CheckState.Checked: CheckState.UnChecked;
            return element;
        }
        public static TextField NewTextField(string text, Pos x, Pos y, Dim? Width = null)
        {
            var element = new TextField();
            element.Text = text;
            element.X = x;
            element.Y = y;
            if (Width != null)
                element.Width = Width;
            return element;
        }
        public static ListView NewListView(TableView tableView)
        {
            var listView = new ListView();
            listView.Add(tableView);
            return listView;
        }
        public static ListView NewListView(IEnumerable<string> tableView)
        {
            var dt = new DataTable();
            foreach (var h in tableView)
                dt.Columns.Add(h);
            
            var tv = new TableView ();
            tv.Table = new DataTableSource(dt);
            var listView = new ListView();
            listView.Add(tv);
            return listView;
        }
        public static Tab NewTab(string text, TableView tableView)
        {
            var tab = new Tab();
            tab.Text = text;
            tab.Add(tableView);
            return tab;
        }
        public static Dialog NewDialog(string text, Dim width, Dim height, params Button[] buttons)
        {
            var dialog = new Dialog();
            dialog.Title = text;
            dialog.Width = width;
            dialog.Height = height;
            foreach (var btn in buttons)
                dialog.AddButton(btn);
            
            return dialog;
        }
        public static Button NewButton(string text, bool isDefault, Action click)
        {
            var btn = new Button();
            btn.Text = text;
            btn.IsDefault = isDefault;
            btn.KeyDown += (s, ev) =>
            {
                click();
            };
            btn.MouseClick += (s, ev) =>
            {
                click();
            };
            return btn;
        }

        public static Key KeyEventKey(this Key key)
        {
            return key;
        }
    }
}
