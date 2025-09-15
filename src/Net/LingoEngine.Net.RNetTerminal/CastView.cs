using System.Collections.Generic;
using System.Data;
using LingoEngine.IO.Data.DTO;
using Terminal.Gui;

namespace LingoEngine.Net.RNetTerminal;

internal sealed class CastView : View
{
    private TabView _tabs;
    public event Action<LingoMemberDTO>? MemberSelected;

    public CastView()
    {
        CanFocus = true;
        _tabs = new TabView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        Add(_tabs);
        var store = TerminalDataStore.Instance;
        store.CastsChanged += ReloadData;
        store.MemberChanged += _ => ReloadData();
        ReloadData();
    }

    public void ReloadData()
    {
        var casts = TerminalDataStore.Instance.GetCasts();
        _tabs.RemoveAll();
        Remove(_tabs);
        _tabs = new TabView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        Add(_tabs);
        var first = true;
        foreach (var cast in casts)
        {
            var members = cast.Value;
            var tableView = new TableView
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Table = CreateTable(members),
                FullRowSelect = true
            };
            tableView.SelectedColumn = 0;
            tableView.SelectedCellChanged += _ => tableView.SelectedColumn = 0;
            tableView.KeyPress += e =>
            {
                if (e.KeyEvent.Key == Key.CursorLeft || e.KeyEvent.Key == Key.CursorRight)
                {
                    e.Handled = true;
                }
            };
            tableView.CellActivated += e =>
            {
                if (e.Row >= 0 && e.Row < members.Count)
                {
                    var member = members[e.Row];
                    MemberSelected?.Invoke(member);
                    if (member.Type == LingoMemberTypeDTO.Text)
                    {
                        var text = new TextView
                        {
                            Width = Dim.Fill(),
                            Height = Dim.Fill(),
                            Text = member.Comments
                        };
                        var ok = new Button("Ok", true);
                        ok.Clicked += () =>
                        {
                            member.Comments = text.Text.ToString() ?? string.Empty;
                            TerminalDataStore.Instance.UpdateMember(member);
                            Application.RequestStop();
                        };
                        var dialog = new Dialog($"Edit {member.Name}", 60, 20, ok);
                        dialog.Add(text);
                        text.SetFocus();
                        Application.Run(dialog);
                    }
                }
            };
            _tabs.AddTab(new TabView.Tab(cast.Key, tableView), first);
            first = false;
        }
        _tabs.SetNeedsDisplay();
    }

    private static DataTable CreateTable(IEnumerable<LingoMemberDTO> members)
    {
        var table = new DataTable();
        table.Columns.Add("Name");
        table.Columns.Add("Number", typeof(int));
        table.Columns.Add("Type");
        table.Columns.Add("Comment");
        foreach (var member in members)
        {
            table.Rows.Add(member.Name, member.Number, member.Type.ToString(), member.Comments);
        }
        return table;
    }
}
