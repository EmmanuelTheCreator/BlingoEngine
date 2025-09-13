using Terminal.Gui;

namespace LingoEngine.Director.Client.ConsoleTest;

internal sealed class PropertyInspector : Window
{
    private readonly TabView _tabs;

    public PropertyInspector() : base("Properties")
    {
        _tabs = new TabView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        var list = BuildPropertyList(new[] { "Start", "End", "LocH", "LocV", "MemberName" });
        _tabs.AddTab(new TabView.Tab("General", list), true);
        Add(_tabs);
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
                Width = 20
            };
            var ok = new Button("Ok", true);
            ok.Clicked += () => Application.RequestStop();
            var dialog = new Dialog($"Edit {name}", 40, 7, ok);
            dialog.Add(new Label(name + ":") { X = 1, Y = 1 }, field);
            Application.Run(dialog);
        };
        return list;
    }
}
