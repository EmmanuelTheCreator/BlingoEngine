using System.Collections.Generic;
using System.Data;
using LingoEngine.IO.Data.DTO;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace LingoEngine.Net.RNetTerminal;

internal sealed class CastView : View
{
    private TabView _tabs;
    public event System.Action<LingoMemberDTO>? MemberSelected;

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
                Table = new DataTableSource(CreateTable(members)),
                FullRowSelect = true
            };
            tableView.SelectedColumn = 0;
            tableView.SelectedCellChanged += (_,_) => tableView.SelectedColumn = 0;
            tableView.KeyDown += (_,e) =>
            {
                
                if (e.KeyCode == Key.CursorLeft || e.KeyCode == Key.CursorRight)
                {
                    e.Handled = true;
                }
            };
            tableView.CellActivated += (_,e) =>
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
                        var ok = RUI.NewButton("OK",true,() =>
                        {
                            member.Comments = text.Text.ToString() ?? string.Empty;
                            TerminalDataStore.Instance.UpdateMember(member);
                            Application.RequestStop();
                        });
                        var dialog = new Dialog();
                        dialog.Text = $"Edit {member.Name}";
                        dialog.Width = 60;
                        dialog.Height = 20;
                        dialog.AddButton(ok);
                        dialog.Add(text);
                        text.SetFocus();
                        Application.Run(dialog);
                    }
                }
            };
            var tab = new Tab();
            tab.Text = cast.Key;
            tab.View = tableView;
            _tabs.AddTab(tab, first);
            first = false;
        }
        _tabs.SetNeedsDraw();
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
