using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using LingoEngine.IO.Data.DTO;
using Terminal.Gui;

namespace LingoEngine.Net.RNetTerminal;

public enum PropertyTarget
{
    Sprite,
    Member
}

internal sealed class PropertyInspector : Window
{
    private readonly TabView _tabs;
    private readonly DataTable _memberTable = new();
    private readonly List<PropertySpec> _memberSpecs = new();
    private readonly TableView _memberTableView;
    private readonly DataTable _spriteTable = new();
    private readonly List<PropertySpec> _spriteSpecs = new();
    private readonly TableView _spriteTableView;
    private readonly TabView.Tab _memberTab;
    private readonly TabView.Tab _spriteTab;
    private readonly TabView.Tab _bitmapTab;
    private readonly TabView.Tab _soundTab;
    private readonly TabView.Tab _movieTab;
    private readonly TabView.Tab _castTab;
    private readonly TabView.Tab _textTab;
    private readonly TabView.Tab _shapeTab;
    private readonly TabView.Tab _guidesTab;
    private readonly TabView.Tab _behaviorTab;
    private readonly TabView.Tab _filmLoopTab;
    private Lingo2DSpriteDTO? _sprite;
    private LingoMemberDTO? _member;
    private string _lastTab = "Sprite";

    public LingoMemberDTO? CurrentMember => _member;

    public event Action<PropertyTarget, string, string>? PropertyChanged;

    public PropertyInspector() : base("Properties")
    {
        _tabs = new TabView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _tabs.SelectedTabChanged += (_, e) =>
        {
            if (e.NewTab != null)
            {
                _lastTab = e.NewTab.Text?.ToString() ?? _lastTab;
            }
        };

        _spriteSpecs.AddRange(new[]
        {
            new PropertySpec("Lock", typeof(bool)),
            new PropertySpec("FlipH", typeof(bool)),
            new PropertySpec("FlipV", typeof(bool)),
            new PropertySpec("Name", typeof(string)),
            new PropertySpec("LocH", typeof(int)),
            new PropertySpec("LocV", typeof(int)),
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

        _spriteTable.Columns.Add("\u200B");
        _spriteTable.Columns.Add("\u200B\u200B");
        foreach (var spec in _spriteSpecs)
        {
            _spriteTable.Rows.Add(spec.Name, GetDefaultValue(spec.Type));
        }
        _spriteTableView = new TableView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Table = _spriteTable,
            FullRowSelect = true
        };
        _spriteTableView.Style.AlwaysShowHeaders = false;
        _spriteTableView.Style.ShowHorizontalHeaderUnderline = false;
        _spriteTableView.Style.ShowHorizontalHeaderOverline = false;
        _spriteTableView.Style.ShowVerticalHeaderLines = false;
        _spriteTableView.Style.ShowVerticalCellLines = false;
        _spriteTableView.SelectedColumn = 1;
        _spriteTableView.SelectedCellChanged += _ => _spriteTableView.SelectedColumn = 1;
        _spriteTableView.KeyPress += e =>
        {
            if (e.KeyEvent.Key == Key.CursorLeft || e.KeyEvent.Key == Key.CursorRight)
            {
                e.Handled = true;
            }
        };
        _spriteTableView.Style.ColumnStyles.Add(_spriteTable.Columns[1], new TableView.ColumnStyle { Alignment = TextAlignment.Right });
        _spriteTableView.CellActivated += args =>
        {
            var spec = _spriteSpecs[args.Row];
            if (spec.ReadOnly)
            {
                return;
            }
            var value = _spriteTable.Rows[args.Row][1]?.ToString() ?? string.Empty;
            var newValue = EditValue(spec.Type, spec.Name, value);
            if (newValue != null)
            {
                _spriteTable.Rows[args.Row][1] = newValue;
                PropertyChanged?.Invoke(PropertyTarget.Sprite, spec.Name, newValue);
            }
            _spriteTableView.SetNeedsDisplay();
        };
        _spriteTab = new TabView.Tab("Sprite", _spriteTableView);

        _memberTable.Columns.Add("\u200B");
        _memberTable.Columns.Add("\u200B\u200B");
        _memberTableView = new TableView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Table = _memberTable,
            FullRowSelect = true
        };
        _memberTableView.Style.AlwaysShowHeaders = false;
        _memberTableView.Style.ShowHorizontalHeaderUnderline = false;
        _memberTableView.Style.ShowHorizontalHeaderOverline = false;
        _memberTableView.Style.ShowVerticalHeaderLines = false;
        _memberTableView.Style.ShowVerticalCellLines = false;
        _memberTableView.SelectedColumn = 1;
        _memberTableView.SelectedCellChanged += _ => _memberTableView.SelectedColumn = 1;
        _memberTableView.KeyPress += e =>
        {
            if (e.KeyEvent.Key == Key.CursorLeft || e.KeyEvent.Key == Key.CursorRight)
            {
                e.Handled = true;
            }
        };
        _memberTableView.Style.ColumnStyles.Add(_memberTable.Columns[1], new TableView.ColumnStyle { Alignment = TextAlignment.Right });
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
                PropertyChanged?.Invoke(PropertyTarget.Member, spec.Name, newValue);
            }
            _memberTableView.SetNeedsDisplay();
        };
        _memberTab = new TabView.Tab("Member", _memberTableView);

        _bitmapTab = CreateTab("Bitmap", new[]
        {
            new PropertySpec("Dimensions", typeof(string), true),
            new PropertySpec("Highlight", typeof(bool)),
            new PropertySpec("RegPointX", typeof(int)),
            new PropertySpec("RegPointY", typeof(int))
        });
        _soundTab = CreateTab("Sound", new[]
        {
            new PropertySpec("Loop", typeof(bool)),
            new PropertySpec("Duration", typeof(float), true),
            new PropertySpec("SampleRate", typeof(int), true),
            new PropertySpec("BitDepth", typeof(int), true),
            new PropertySpec("Channels", typeof(int), true),
            new PropertySpec("Play", typeof(bool)),
            new PropertySpec("Stop", typeof(bool))
        });
        _movieTab = CreateTab("Movie", new[]
        {
            new PropertySpec("StageWidth", typeof(int)),
            new PropertySpec("StageHeight", typeof(int)),
            new PropertySpec("Resolution", typeof(float)),
            new PropertySpec("Channels", typeof(int)),
            new PropertySpec("BackgroundColor", typeof(Color)),
            new PropertySpec("About", typeof(string)),
            new PropertySpec("Copyright", typeof(string))
        });
        _castTab = CreateTab("Cast", new[]
        {
            new PropertySpec("Number", typeof(int)),
            new PropertySpec("Name", typeof(string))
        });
        _textTab = CreateTab("Text", new[]
        {
            new PropertySpec("Width", typeof(int)),
            new PropertySpec("Height", typeof(int)),
            new PropertySpec("Edit", typeof(bool))
        });
        _shapeTab = CreateTab("Shape", new[]
        {
            new PropertySpec("Shape", typeof(string)),
            new PropertySpec("Filled", typeof(bool)),
            new PropertySpec("Width", typeof(int)),
            new PropertySpec("Height", typeof(int)),
            new PropertySpec("Edit", typeof(bool))
        });
        _guidesTab = CreateTab("Guides", new[]
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
        _behaviorTab = CreateTab("Behavior", new[] { new PropertySpec("Behaviors", typeof(string)) });
        _filmLoopTab = CreateTab("FilmLoop", new[]
        {
            new PropertySpec("Framing", typeof(string)),
            new PropertySpec("Loop", typeof(bool)),
            new PropertySpec("FrameCount", typeof(int))
        });

        Add(_tabs);
        SetTabs(_spriteTab, _memberTab);
        var initial = _tabs.Tabs.FirstOrDefault(t => t.Text.ToString() == _lastTab) ?? _spriteTab;
        _tabs.SelectedTab = initial;

        var store = TerminalDataStore.Instance;
        UpdateSelection(store.GetSelectedSprite());
        store.SelectedSpriteChanged += UpdateSelection;
        store.SpriteChanged += s =>
        {
            var sel = store.GetSelectedSprite();
            if (sel.HasValue && sel.Value.SpriteNum == s.SpriteNum && sel.Value.BeginFrame == s.BeginFrame)
            {
                ShowSprite(s);
            }
        };
        store.MemberChanged += m =>
        {
            var sel = store.GetSelectedSprite();
            if (sel.HasValue)
            {
                var sprite = store.FindSprite(sel.Value);
                if (sprite != null && sprite.CastLibNum == m.CastLibNum && sprite.MemberNum == m.NumberInCast)
                {
                    ShowMember(m);
                    return;
                }
            }
            if (_member != null && _member.CastLibNum == m.CastLibNum && _member.NumberInCast == m.NumberInCast)
            {
                ShowMember(m);
            }
        };
    }

    private void UpdateSelection(SpriteRef? sel)
    {
        var store = TerminalDataStore.Instance;
        var sprite = sel.HasValue ? store.FindSprite(sel.Value) : null;
        ShowSprite(sprite);
        if (sprite != null)
        {
            ShowMember(store.FindMember(sprite.CastLibNum, sprite.MemberNum));
        }
        else
        {
            ShowMember(null);
        }
    }

    public void ShowSprite(Lingo2DSpriteDTO? sprite)
    {
        _sprite = sprite;
        for (var i = 0; i < _spriteSpecs.Count; i++)
        {
            var spec = _spriteSpecs[i];
            string value;
            if (sprite == null)
            {
                value = GetDefaultValue(spec.Type);
            }
            else
            {
                value = spec.Name switch
                {
                    "Lock" => sprite.Lock.ToString(),
                    "FlipH" => sprite.FlipH.ToString(),
                    "FlipV" => sprite.FlipV.ToString(),
                    "Name" => sprite.Name,
                    "LocH" => ((int)sprite.LocH).ToString(CultureInfo.InvariantCulture),
                    "LocV" => ((int)sprite.LocV).ToString(CultureInfo.InvariantCulture),
                    "Z" => sprite.LocZ.ToString(CultureInfo.InvariantCulture),
                    "Width" => ((int)sprite.Width).ToString(CultureInfo.InvariantCulture),
                    "Height" => ((int)sprite.Height).ToString(CultureInfo.InvariantCulture),
                    "Ink" => sprite.Ink.ToString(CultureInfo.InvariantCulture),
                    "Blend" => sprite.Blend.ToString(CultureInfo.InvariantCulture),
                    "StartFrame" => sprite.BeginFrame.ToString(CultureInfo.InvariantCulture),
                    "EndFrame" => sprite.EndFrame.ToString(CultureInfo.InvariantCulture),
                    "Rotation" => sprite.Rotation.ToString(CultureInfo.InvariantCulture),
                    "Skew" => sprite.Skew.ToString(CultureInfo.InvariantCulture),
                    _ => _spriteTable.Rows[i][1]?.ToString() ?? GetDefaultValue(spec.Type)
                };
            }
            _spriteTable.Rows[i][1] = value;
        }
        _spriteTableView.SetNeedsDisplay();
    }

    private TabView.Tab CreateTab(string title, PropertySpec[] props)
    {
        var view = BuildPropertyTableView(props);
        return new TabView.Tab(title, view);
    }

    private void SetTabs(params TabView.Tab[] tabs)
    {
        foreach (var existing in _tabs.Tabs.ToList())
        {
            _tabs.RemoveTab(existing);
        }

        foreach (var tab in tabs)
        {
            _tabs.AddTab(tab, _tabs.Tabs.Count == 0);
        }
    }

    private TableView BuildPropertyTableView(PropertySpec[] props)
    {
        var table = new DataTable();
        table.Columns.Add("\u200B");
        table.Columns.Add("\u200B\u200B");
        for (var i = 0; i < props.Length; i++)
        {
            table.Rows.Add(props[i].Name, GetDefaultValue(props[i].Type));
        }
        var view = new TableView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Table = table,
            FullRowSelect = true
        };
        view.Style.AlwaysShowHeaders = false;
        view.Style.ShowHorizontalHeaderUnderline = false;
        view.Style.ShowHorizontalHeaderOverline = false;
        view.Style.ShowVerticalHeaderLines = false;
        view.Style.ShowVerticalCellLines = false;
        view.SelectedColumn = 1;
        view.SelectedCellChanged += _ => view.SelectedColumn = 1;
        view.KeyPress += e =>
        {
            if (e.KeyEvent.Key == Key.CursorLeft || e.KeyEvent.Key == Key.CursorRight)
            {
                e.Handled = true;
            }
        };
        view.Style.ColumnStyles.Add(table.Columns[1], new TableView.ColumnStyle { Alignment = TextAlignment.Right });
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
                PropertyChanged?.Invoke(PropertyTarget.Member, spec.Name, newValue);
            }
            view.SetNeedsDisplay();
        };
        return view;
    }

    private static string GetDefaultValue(Type type)
    {
        if (type == typeof(bool))
        {
            return bool.FalseString;
        }
        if (type == typeof(int) || type == typeof(float))
        {
            return "0";
        }
        return string.Empty;
    }

    private static string? EditValue(Type type, string name, string value)
    {
        string? result = null;
        if (type == typeof(bool))
        {
            var check = new CheckBox(12, 1, string.Empty, bool.TryParse(value, out var b) && b);
            check.KeyPress += e =>
            {
                if (e.KeyEvent.Key == Key.Space)
                {
                    check.Checked = !check.Checked;
                    result = check.Checked.ToString();
                    Application.RequestStop();
                    e.Handled = true;
                }
            };
            var ok = new Button("Ok", true);
            ok.Clicked += () =>
            {
                result = check.Checked.ToString();
                Application.RequestStop();
            };
            var dialog = new Dialog($"Edit {name}", 30, 7, ok);
            dialog.Add(new Label(name + ":") { X = 1, Y = 1 }, check);
            check.SetFocus();
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

    public void ShowMember(LingoMemberDTO? member)
    {
        _member = member;
        _memberTable.Rows.Clear();
        _memberSpecs.Clear();
        if (member == null)
        {
            _memberTableView.SetNeedsDisplay();
            var tabsEmpty = new List<TabView.Tab>();
            if (_sprite != null)
            {
                tabsEmpty.Add(_spriteTab);
            }
            tabsEmpty.Add(_memberTab);
            SetTabs(tabsEmpty.ToArray());
            var target = _tabs.Tabs.FirstOrDefault(t => t.Text.ToString() == _lastTab);
            _tabs.SelectedTab = target ?? (_sprite != null ? _spriteTab : _memberTab);
            return;
        }

        AddMember("Name", member.Name, typeof(string));
        AddMember("Number", member.Number.ToString(), typeof(int), true);
        AddMember("CastLibNum", member.CastLibNum.ToString(), typeof(int), true);
        AddMember("NumberInCast", member.NumberInCast.ToString(), typeof(int), true);
        AddMember("Type", member.Type.ToString(), typeof(string), true);
        AddMember("RegPointX", member.RegPoint.X.ToString(), typeof(float), true);
        AddMember("RegPointY", member.RegPoint.Y.ToString(), typeof(float), true);
        AddMember("Width", member.Width.ToString(), typeof(int), true);
        AddMember("Height", member.Height.ToString(), typeof(int), true);
        AddMember("Size", member.Size.ToString(), typeof(int), true);
        AddMember("Comment", member.Comments, typeof(string));
        AddMember("FileName", member.FileName, typeof(string), true);
        AddMember("PurgePriority", member.PurgePriority.ToString(), typeof(int), true);

        _memberTableView.SetNeedsDisplay();

        var tabs = new List<TabView.Tab>();
        if (_sprite != null)
        {
            tabs.Add(_spriteTab);
        }
        tabs.Add(_memberTab);
        tabs.Add(_castTab);

        switch (member.Type)
        {
            case LingoMemberTypeDTO.Bitmap:
            case LingoMemberTypeDTO.Picture:
                tabs.Add(_bitmapTab);
                break;
            case LingoMemberTypeDTO.Sound:
                tabs.Add(_soundTab);
                break;
            case LingoMemberTypeDTO.Text:
            case LingoMemberTypeDTO.Field:
                tabs.Add(_textTab);
                break;
            case LingoMemberTypeDTO.Shape:
                tabs.Add(_shapeTab);
                break;
            case LingoMemberTypeDTO.FilmLoop:
                tabs.Add(_filmLoopTab);
                break;
            case LingoMemberTypeDTO.Movie:
                tabs.Add(_movieTab);
                break;
        }

        SetTabs(tabs.ToArray());
        var desired = _tabs.Tabs.FirstOrDefault(t => t.Text.ToString() == _lastTab);
        _tabs.SelectedTab = desired ?? (_sprite != null ? _spriteTab : _memberTab);
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

