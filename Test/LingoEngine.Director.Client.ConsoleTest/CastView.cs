using System;
using System.Data;
using Terminal.Gui;

namespace LingoEngine.Director.Client.ConsoleTest;

internal sealed class CastView : View
{
    private readonly TableView _table;

    public CastView()
    {
        CanFocus = true;
        _table = new TableView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        Add(_table);
        LoadData();
    }

    private void LoadData()
    {
        var table = new DataTable();
        table.Columns.Add("Name");
        table.Columns.Add("Number", typeof(int));
        table.Columns.Add("Type");
        table.Columns.Add("Modified");
        table.Columns.Add("Comment");
        table.Rows.Add("Greeting", 1, "Text", DateTime.Now.ToShortDateString(), string.Empty);
        table.Rows.Add("Info", 2, "Text", DateTime.Now.ToShortDateString(), string.Empty);
        table.Rows.Add("Box", 3, "Shape", DateTime.Now.ToShortDateString(), string.Empty);
        _table.Table = table;
    }
}
