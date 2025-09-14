using System.Collections.Generic;
using System.Data;
using LingoEngine.IO.Data.DTO;
using Terminal.Gui;

namespace LingoEngine.Net.RNetTerminal;

internal sealed class CastView : View
{
    private readonly TabView _tabs;
    public event Action<LingoMemberDTO>? MemberSelected;

    public CastView(Dictionary<string, List<LingoMemberDTO>> casts)
    {
        CanFocus = true;
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
                Table = CreateTable(members)
            };
            tableView.CellActivated += e =>
            {
                if (e.Row >= 0 && e.Row < members.Count)
                {
                    MemberSelected?.Invoke(members[e.Row]);
                }
            };
            _tabs.AddTab(new TabView.Tab(cast.Key, tableView), first);
            first = false;
        }
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
