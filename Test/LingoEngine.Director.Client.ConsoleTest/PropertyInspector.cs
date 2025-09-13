using System.Collections.Generic;
using System.Data;
using LingoEngine.IO.Data.DTO;
using Terminal.Gui;

namespace LingoEngine.Director.Client.ConsoleTest;

internal sealed class PropertyInspector : Window
{
    private readonly TabView _tabs;
    private readonly DataTable _memberTable = new();
    private readonly TableView _memberTableView;
    private readonly TabView.Tab _memberTab;

    public PropertyInspector() : base("Properties")
    {
        _tabs = new TabView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        AddTab("Sprite", new[]
        {
            "Lock", "FlipH", "FlipV", "Name", "X", "Y", "Z", "Left", "Top", "Right", "Bottom", "Width", "Height",
            "Ink", "Blend", "StartFrame", "EndFrame", "Rotation", "Skew", "ForeColor", "BackColor", "Behaviors",
        });

        _memberTable.Columns.Add("Property");
        _memberTable.Columns.Add("Value");
        _memberTableView = new TableView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Table = _memberTable,
        };
        _memberTableView.CellActivated += args =>
        {
            var name = _memberTable.Rows[args.Row][0]?.ToString() ?? string.Empty;
            var value = _memberTable.Rows[args.Row][1]?.ToString() ?? string.Empty;
            var field = new TextField(value)
            {
                X = 12,
                Y = 1,
                Width = 20,
            };
            var ok = new Button("Ok", true);
            ok.Clicked += () =>
            {
                _memberTable.Rows[args.Row][1] = field.Text.ToString();
                Application.RequestStop();
            };
            var dialog = new Dialog($"Edit {name}", 40, 7, ok);
            dialog.Add(new Label(name + ":") { X = 1, Y = 1 }, field);
            field.SetFocus();
            Application.Run(dialog);
            _memberTableView.SetNeedsDisplay();
        };
        _memberTab = new TabView.Tab("Member", _memberTableView);
        _tabs.AddTab(_memberTab, false);

        AddTab("Bitmap", new[] { "Dimensions", "Highlight", "RegPointX", "RegPointY" });
        AddTab("Sound", new[] { "Loop", "Duration", "SampleRate", "BitDepth", "Channels", "Play", "Stop" });
        AddTab("Movie", new[]
        {
            "StageWidth", "StageHeight", "Resolution", "Channels", "BackgroundColor", "About", "Copyright",
        });
        AddTab("Cast", new[] { "Number", "Name" });
        AddTab("Text", new[] { "Width", "Height", "Edit" });
        AddTab("Shape", new[] { "Shape", "Filled", "Width", "Height", "Edit" });
        AddTab("Guides", new[]
        {
            "GuidesColor", "GuidesVisible", "GuidesSnap", "GuidesLock", "GridColor", "GridVisible", "GridSnap",
            "AddVerticalGuide", "AddHorizontalGuide", "RemoveGuides", "GridWidth", "GridHeight",
        });
        AddTab("Behavior", new[] { "Behaviors" });
        AddTab("FilmLoop", new[] { "Framing", "Loop", "FrameCount" });

        Add(_tabs);
    }

    private void AddTab(string title, string[] props)
    {
        var view = BuildPropertyTableView(props);
        _tabs.AddTab(new TabView.Tab(title, view), _tabs.Tabs.Count == 0);
    }

    private static TableView BuildPropertyTableView(string[] props)
    {
        var table = new DataTable();
        table.Columns.Add("Property");
        table.Columns.Add("Value");
        foreach (var prop in props)
        {
            table.Rows.Add(prop, string.Empty);
        }
        var view = new TableView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Table = table,
        };
        view.CellActivated += args =>
        {
            var name = table.Rows[args.Row][0]?.ToString() ?? string.Empty;
            var value = table.Rows[args.Row][1]?.ToString() ?? string.Empty;
            var field = new TextField(value)
            {
                X = 12,
                Y = 1,
                Width = 20,
            };
            var ok = new Button("Ok", true);
            ok.Clicked += () =>
            {
                table.Rows[args.Row][1] = field.Text.ToString();
                Application.RequestStop();
            };
            var dialog = new Dialog($"Edit {name}", 40, 7, ok);
            dialog.Add(new Label(name + ":") { X = 1, Y = 1 }, field);
            field.SetFocus();
            Application.Run(dialog);
            view.SetNeedsDisplay();
        };
        return view;
    }

    public void ShowMember(LingoMemberDTO member)
    {
        _memberTable.Rows.Clear();
        _memberTable.Rows.Add("Name", member.Name);
        _memberTable.Rows.Add("Number", member.Number.ToString());
        _memberTable.Rows.Add("Type", member.Type.ToString());
        _memberTable.Rows.Add("Comment", member.Comments);
        _memberTableView.SetNeedsDisplay();
        _tabs.SelectedTab = _memberTab;
    }
}
