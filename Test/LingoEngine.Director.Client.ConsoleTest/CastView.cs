using System;
using System.Collections.Generic;
using System.Data;
using Terminal.Gui;

namespace LingoEngine.Director.Client.ConsoleTest;

internal sealed class CastView : View
{
    private readonly TabView _tabs;

    public CastView()
    {
        CanFocus = true;
        _tabs = new TabView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        Add(_tabs);
        LoadData();
    }

    private void LoadData()
    {
        var casts = new Dictionary<string, DataTable>
        {
            ["TestCast"] = CreateSampleTable(new[]
            {
                ("Greeting", 1, "Text"),
                ("Info", 2, "Text"),
                ("Box", 3, "Shape")
            }),
            ["ExtraCast"] = CreateSampleTable(new[]
            {
                ("Note", 1, "Text")
            })
        };

        var first = true;
        foreach (var cast in casts)
        {
            var tableView = new TableView
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Table = cast.Value
            };
            _tabs.AddTab(new TabView.Tab(cast.Key, tableView), first);
            first = false;
        }
    }

    private static DataTable CreateSampleTable(IEnumerable<(string Name, int Number, string Type)> members)
    {
        var table = new DataTable();
        table.Columns.Add("Name");
        table.Columns.Add("Number", typeof(int));
        table.Columns.Add("Type");
        table.Columns.Add("Modified");
        table.Columns.Add("Comment");
        foreach (var member in members)
        {
            table.Rows.Add(member.Name, member.Number, member.Type, DateTime.Now.ToShortDateString(), string.Empty);
        }
        return table;
    }
}
