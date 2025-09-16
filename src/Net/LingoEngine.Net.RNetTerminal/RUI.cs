using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace LingoEngine.Net.RNetTerminal
{
    internal static class RUI
    {

        
        internal static Window NewWindow(string name, Pos x, Pos y, Dim? width = null, Dim? height = null)
        {
            var element = new Window();
            element.Text = name;
            element.X = x;
            element.Y = y;
            if (width != null) element.Width = width;
            if (height != null) element.Height = height;
            return element;
        }
        internal static View NewView(Pos x, Pos y, Dim? width = null, Dim? height = null)
        {
            var element = new View();
            element.X = x;
            element.Y = y;
            if (width != null) element.Width = width;
            if (height != null) element.Height = height;
            return element;
        }
        public static Label NewLabel(string text, Pos x, Pos y, Dim? width = null, Dim? height = null)
        {
            var element = new Label();
            element.Text = text;
            element.X = x;
            element.Y = y;
            if (width != null) element.Width = width;
            if (height != null) element.Height = height;
            return element;
        }
        public static CheckBox NewCheckBox(string text, bool isChecked = false)
        {
            var element = new CheckBox();
            element.Text = text;
            element.CheckedState = isChecked?CheckState.Checked: CheckState.UnChecked;
            return element;
        }
       
        public static bool Checked(this CheckBox checkBox) => checkBox.CheckedState == CheckState.Checked;
        public static bool ToBool(this CheckState checkState) => checkState == CheckState.Checked;
        public static CheckState ToCheckedSTate(this bool state) => state? CheckState.Checked: CheckState.UnChecked;


        public static CheckBox NewCheckBox(Pos x , Pos y , string text = "", bool isChecked = false)
        {
            var element = new CheckBox();
            element.Text = text;
            element.X = x;
            element.Y = y;
            element.CheckedState = isChecked?CheckState.Checked: CheckState.UnChecked;
            return element;
        }
        public static TextField NewTextField(string text, Pos x, Pos y, Dim? width = null)
        {
            var element = new TextField();
            element.Text = text;
            element.X = x;
            element.Y = y;
            if (width != null)
                element.Width = width;
            RNetTerminalStyle.SetForTextField(element);
            return element;
        }
        public static ListView NewListView(TableView tableView)
        {
            var listView = new ListView();
            listView.Add(tableView);
            return listView;
        }
        public static ListView NewListView(IEnumerable<string> data)
        {
            var dt = new DataTable();
            var column = dt.Columns.Add("Data");
            //column.MaxLength = 1000;
            foreach (var h in data)
                dt.Rows.Add(h);
            
            var tv = new TableView ();
            tv.Table = new DataTableSource(dt);
            var listView = new ListView();
            listView.Add(tv);
            return listView;
        }
        internal static ListView? NewListView(List<string> data, Pos x, Pos y, Dim? width, Dim? height)
        {
            var element = NewListView(data);
            element.X = x;
            element.Y = y;
            if (width != null) element.Width = width;
            if (height != null) element.Height = height;
            return element;
        }
        public static Tab NewTab(string text, View tableView)
        {
            var tab = new Tab();
            tab.DisplayText = text;
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
            RNetTerminalStyle.SetForDialog(dialog);
            return dialog;
        }
        public static Button NewButton(string text, bool isDefault = false, System.Action? click = null)
        {
            var btn = new Button();
            btn.Text = text;
            btn.IsDefault = isDefault;
            if (click != null)
            {
                btn.Accepting += (s, ev) =>
                {
                        click();
                };
                //btn.MouseClick += (s, ev) =>
                //{
                //    click();
                //};
            }
            return btn;
        }

        public static Key KeyEventKey(this Key key)
        {
            return key;
        }
        public static DataTableSource ToDataTableSource(this IEnumerable<string> data)
        {
            var dt = new DataTable();
            foreach (var h in data)
                dt.Columns.Add(h);

            var dts = new DataTableSource(dt);
            return dts;
        }
    }
}
