using System;
using System.Collections.Generic;
using System.Data;
using LingoEngine.IO.Data.DTO;
using Terminal.Gui;

namespace LingoEngine.Director.Client.ConsoleTest;

internal sealed class PropertyInspector : Window
{
    private readonly TabView _tabs;
    private readonly DataTable _memberTable = new();
    private readonly List<PropertySpec> _memberSpecs = new();
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
            new PropertySpec("Lock", typeof(bool)),
            new PropertySpec("FlipH", typeof(bool)),
            new PropertySpec("FlipV", typeof(bool)),
            new PropertySpec("Name", typeof(string)),
            new PropertySpec("X", typeof(int)),
            new PropertySpec("Y", typeof(int)),
            new PropertySpec("Z", typeof(int)),
            new PropertySpec("Left", typeof(int)),
            new PropertySpec("Top", typeof(int)),
            new PropertySpec("Right", typeof(int)),
            new PropertySpec("Bottom", typeof(int)),
            new PropertySpec("Width", typeof(int)),
            new PropertySpec("Height", typeof(int)),
            new PropertySpec("Ink", typeof(int)),
            new PropertySpec("Blend", typeof(float)),
            new PropertySpec("StartFrame", typeof(int)),
            new PropertySpec("EndFrame", typeof(int)),
            new PropertySpec("Rotation", typeof(float)),
            new PropertySpec("Skew", typeof(float)),
            new PropertySpec("ForeColor", typeof(Color)),
            new PropertySpec("BackColor", typeof(Color)),
            new PropertySpec("Behaviors", typeof(string))
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
            var spec = _memberSpecs[args.Row];
            if (spec.ReadOnly)
            {
                return;
            }
            var value = _memberTable.Rows[args.Row][1]?.ToString() ?? string.Empty;
            var newValue = EditValue(spec.Type, spec.Name, value);
            if (newValue != null)
            {
                _memberTable.Rows[args.Row][1] = newValue;
            }
            _memberTableView.SetNeedsDisplay();
        };
        _memberTab = new TabView.Tab("Member", _memberTableView);
        _tabs.AddTab(_memberTab, false);

        AddTab("Bitmap", new[]
        {
            new PropertySpec("Dimensions", typeof(string), true),
            new PropertySpec("Highlight", typeof(bool)),
            new PropertySpec("RegPointX", typeof(int)),
            new PropertySpec("RegPointY", typeof(int))
        });
        AddTab("Sound", new[]
        {
            new PropertySpec("Loop", typeof(bool)),
            new PropertySpec("Duration", typeof(float), true),
            new PropertySpec("SampleRate", typeof(int), true),
            new PropertySpec("BitDepth", typeof(int), true),
            new PropertySpec("Channels", typeof(int), true),
            new PropertySpec("Play", typeof(bool)),
            new PropertySpec("Stop", typeof(bool))
        });
        AddTab("Movie", new[]
        {
            new PropertySpec("StageWidth", typeof(int)),
            new PropertySpec("StageHeight", typeof(int)),
            new PropertySpec("Resolution", typeof(float)),
            new PropertySpec("Channels", typeof(int)),
            new PropertySpec("BackgroundColor", typeof(Color)),
            new PropertySpec("About", typeof(string)),
            new PropertySpec("Copyright", typeof(string))
        });
        AddTab("Cast", new[]
        {
            new PropertySpec("Number", typeof(int)),
            new PropertySpec("Name", typeof(string))
        });
        AddTab("Text", new[]
        {
            new PropertySpec("Width", typeof(int)),
            new PropertySpec("Height", typeof(int)),
            new PropertySpec("Edit", typeof(bool))
        });
        AddTab("Shape", new[]
        {
            new PropertySpec("Shape", typeof(string)),
            new PropertySpec("Filled", typeof(bool)),
            new PropertySpec("Width", typeof(int)),
            new PropertySpec("Height", typeof(int)),
            new PropertySpec("Edit", typeof(bool))
        });
        AddTab("Guides", new[]
        {
            new PropertySpec("GuidesColor", typeof(Color)),
            new PropertySpec("GuidesVisible", typeof(bool)),
            new PropertySpec("GuidesSnap", typeof(bool)),
            new PropertySpec("GuidesLock", typeof(bool)),
            new PropertySpec("GridColor", typeof(Color)),
            new PropertySpec("GridVisible", typeof(bool)),
            new PropertySpec("GridSnap", typeof(bool)),
            new PropertySpec("AddVerticalGuide", typeof(bool)),
            new PropertySpec("AddHorizontalGuide", typeof(bool)),
            new PropertySpec("RemoveGuides", typeof(bool)),
            new PropertySpec("GridWidth", typeof(int)),
            new PropertySpec("GridHeight", typeof(int))
        });
        AddTab("Behavior", new[] { new PropertySpec("Behaviors", typeof(string)) });
        AddTab("FilmLoop", new[]
        {
            new PropertySpec("Framing", typeof(string)),
            new PropertySpec("Loop", typeof(bool)),
            new PropertySpec("FrameCount", typeof(int))
        });

        Add(_tabs);
    }

    private void AddTab(string title, PropertySpec[] props)
    {
        var view = BuildPropertyTableView(props);
        _tabs.AddTab(new TabView.Tab(title, view), _tabs.Tabs.Count == 0);
    }

    private static TableView BuildPropertyTableView(PropertySpec[] props)
    {
        var table = new DataTable();
        table.Columns.Add("Property");
        table.Columns.Add("Value");
        for (var i = 0; i < props.Length; i++)
        {
            table.Rows.Add(props[i].Name, string.Empty);
        }
        var view = new TableView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Table = table,
        };
        view.CellActivated += args =>
        {
            var spec = props[args.Row];
            if (spec.ReadOnly)
            {
                return;
            }
            var value = table.Rows[args.Row][1]?.ToString() ?? string.Empty;
            var newValue = EditValue(spec.Type, spec.Name, value);
            if (newValue != null)
            {
                table.Rows[args.Row][1] = newValue;
            }
            view.SetNeedsDisplay();
        };
        return view;
    }

    private static string? EditValue(Type type, string name, string value)
    {
        string? result = null;
        if (type == typeof(bool))
        {
            var check = new CheckBox(12, 1, string.Empty, bool.TryParse(value, out var b) && b);
            var ok = new Button("Ok", true);
            ok.Clicked += () =>
            {
                result = check.Checked.ToString();
                Application.RequestStop();
            };
            var dialog = new Dialog($"Edit {name}", 30, 7, ok);
            dialog.Add(new Label(name + ":") { X = 1, Y = 1 }, check);
            Application.Run(dialog);
        }
        else if (type == typeof(int))
        {
            var field = new TextField(value) { X = 12, Y = 1, Width = 20 };
            var ok = new Button("Ok", true);
            ok.Clicked += () =>
            {
                if (int.TryParse(field.Text.ToString(), out var v))
                {
                    result = v.ToString();
                }
                Application.RequestStop();
            };
            var dialog = new Dialog($"Edit {name}", 40, 7, ok);
            dialog.Add(new Label(name + ":") { X = 1, Y = 1 }, field);
            field.SetFocus();
            Application.Run(dialog);
        }
        else if (type == typeof(float))
        {
            var field = new TextField(value) { X = 12, Y = 1, Width = 20 };
            var ok = new Button("Ok", true);
            ok.Clicked += () =>
            {
                if (float.TryParse(field.Text.ToString(), out var v))
                {
                    result = v.ToString();
                }
                Application.RequestStop();
            };
            var dialog = new Dialog($"Edit {name}", 40, 7, ok);
            dialog.Add(new Label(name + ":") { X = 1, Y = 1 }, field);
            field.SetFocus();
            Application.Run(dialog);
        }
        else if (type == typeof(Color))
        {
            var colors = Enum.GetNames<Color>();
            var list = new ListView(colors)
            {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            var ok = new Button("Ok", true);
            ok.Clicked += () =>
            {
                result = colors[list.SelectedItem];
                Application.RequestStop();
            };
            var dialog = new Dialog($"Edit {name}", 30, 15, ok);
            dialog.Add(list);
            Application.Run(dialog);
        }
        else
        {
            var field = new TextField(value) { X = 12, Y = 1, Width = 20 };
            var ok = new Button("Ok", true);
            ok.Clicked += () =>
            {
                result = field.Text.ToString();
                Application.RequestStop();
            };
            var dialog = new Dialog($"Edit {name}", 40, 7, ok);
            dialog.Add(new Label(name + ":") { X = 1, Y = 1 }, field);
            field.SetFocus();
            Application.Run(dialog);
        }
        return result;
    }

    public void ShowMember(LingoMemberDTO member)
    {
        _memberTable.Rows.Clear();
        _memberSpecs.Clear();
        AddMember("Name", member.Name, typeof(string));
        AddMember("Number", member.Number.ToString(), typeof(int), true);
        AddMember("Type", member.Type.ToString(), typeof(string), true);
        AddMember("Comment", member.Comments, typeof(string));
        _memberTableView.SetNeedsDisplay();
        _tabs.SelectedTab = _memberTab;
    }

    private void AddMember(string name, string value, Type type, bool readOnly = false)
    {
        _memberTable.Rows.Add(name, value);
        _memberSpecs.Add(new PropertySpec(name, type, readOnly));
    }

    private sealed class PropertySpec
    {
        public string Name { get; }
        public Type Type { get; }
        public bool ReadOnly { get; }

        public PropertySpec(string name, Type type, bool readOnly = false)
        {
            Name = name;
            Type = type;
            ReadOnly = readOnly;
        }
    }
}

