using System.Collections.Generic;
using Terminal.Gui;

namespace LingoEngine.Director.Client.ConsoleTest;

internal sealed class PropertyInspector : Window
{
    private readonly TabView _tabs;
    private readonly List<string> _memberItems = new();
    private readonly ListView _memberList;
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

        _memberList = new ListView(_memberItems)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        _memberList.OpenSelectedItem += args =>
        {
            var line = _memberItems[args.Item];
            var idx = line.IndexOf(':');
            var name = idx >= 0 ? line[..idx] : line;
            var value = idx >= 0 ? line[(idx + 1)..].Trim() : string.Empty;
            var field = new TextField(value)
            {
                X = 12,
                Y = 1,
                Width = 20,
            };
            var ok = new Button("Ok", true);
            ok.Clicked += () => Application.RequestStop();
            var dialog = new Dialog($"Edit {name}", 40, 7, ok);
            dialog.Add(new Label(name + ":") { X = 1, Y = 1 }, field);
            Application.Run(dialog);
        };
        _memberTab = new TabView.Tab("Member", _memberList);
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
        var list = BuildPropertyList(props);
        _tabs.AddTab(new TabView.Tab(title, list), _tabs.Tabs.Count == 0);
    }

    private static ListView BuildPropertyList(string[] props)
    {
        var list = new ListView(props)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        list.OpenSelectedItem += args =>
        {
            var name = props[args.Item];
            var field = new TextField(string.Empty)
            {
                X = 12,
                Y = 1,
                Width = 20,
            };
            var ok = new Button("Ok", true);
            ok.Clicked += () => Application.RequestStop();
            var dialog = new Dialog($"Edit {name}", 40, 7, ok);
            dialog.Add(new Label(name + ":") { X = 1, Y = 1 }, field);
            Application.Run(dialog);
        };
        return list;
    }

    public void ShowMember(CastMemberInfo member)
    {
        _memberItems.Clear();
        _memberItems.Add($"Name: {member.Name}");
        _memberItems.Add($"Number: {member.Number}");
        _memberItems.Add($"Type: {member.Type}");
        _memberItems.Add($"Modified: {member.Modified}");
        _memberItems.Add($"Comment: {member.Comment}");
        _memberList.SetSource(_memberItems.ToList());
        _tabs.SelectedTab = _memberTab;
    }
}
