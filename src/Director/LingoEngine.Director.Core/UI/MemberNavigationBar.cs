using AbstUI.Components;
using AbstUI.Primitives;
using LingoEngine.Core;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Members;
using LingoEngine.Movies;

namespace LingoEngine.Director.Core.UI;

public class MemberNavigationBar<T> where T : class, ILingoMember
{
    private string _memberName = "";
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoPlayer _player;
    private readonly IDirectorIconManager _iconManager;
    private readonly AbstUIGfxWrapPanel _panel;
    private AbstUIGfxButton _typeIcon;
    private AbstUIGfxLabel _numberLabel;
    private AbstUIGfxLabel _castLibLabel;
    private AbstUIGfxInputText _nameEdit;

    private T? _member;

    public AbstUIGfxWrapPanel Panel => _panel;
    public string MemberName
    {
        get => _memberName; 
        set
        {
            _memberName = value;
            if (_member != null) _member.Name = value.Trim();
        }
    }

#pragma warning disable CS8618 
    public MemberNavigationBar(IDirectorEventMediator mediator, ILingoPlayer player, IDirectorIconManager iconManager, ILingoFrameworkFactory factory, int barHeight = 20)
#pragma warning restore CS8618 
    {
        _mediator = mediator;
        _player = player;
        _iconManager = iconManager;
        _panel = factory.CreateWrapPanel(AOrientation.Horizontal, "MemberNavigationBar");
        _panel.Height = barHeight;

        _panel.Compose()
            .AddButton("PrevButton", string.Empty, () => Navigate(-1), b =>
            {
                b.IconTexture = iconManager.Get(DirectorIcon.Previous);
                b.Width = barHeight;
                b.Height = barHeight;
            })
            .AddButton("NextButton", string.Empty, () => Navigate(1), b =>
            {
                b.IconTexture = iconManager.Get(DirectorIcon.Next);
                b.Width = barHeight;
                b.Height = barHeight;
            })
            .AddButton("TypeIcon", string.Empty, () => { }, c =>
            {
                _typeIcon = c;
                _typeIcon.Width = barHeight;
                _typeIcon.Height = barHeight;
                _typeIcon.Enabled = false;
            })
            .AddTextInput("NameEdit", this, x => x.MemberName, 100,c => _nameEdit = c)
            .AddLabel("MemberNameLabel", "0",11,40,c => _numberLabel = c)
            .AddButton("InfoButton", string.Empty, OnInfo, b =>
            {
                b.IconTexture = iconManager.Get(DirectorIcon.Info);
                b.Width = barHeight;
                b.Height = barHeight;
            })
            .AddLabel("CastLibLabel", "0",11,80,c => _castLibLabel = c)
            ;

    }

    public void SetMember(T member)
    {
        _member = member;
        _memberName = member.Name;
        _nameEdit.Text = _memberName;
        _numberLabel.Text = member.NumberInCast.ToString();
        _castLibLabel.Text = GetCastName(member);
        var icon = LingoMemberTypeIcons.GetIconType(member);
        _typeIcon.IconTexture = icon.HasValue ? _iconManager.Get(icon.Value) : null;
    }

    private string GetCastName(ILingoMember m)
    {
        if (_player.ActiveMovie is ILingoMovie movie)
        {
            return movie.CastLib.GetCast(m.CastLibNum).Name;
        }
        return string.Empty;
    }

    private void OnInfo()
    {
        if (_member == null) return;
        _mediator.RaiseFindMember(_member);
        _mediator.RaiseMemberSelected(_member);
    }

    private void Navigate(int offset)
    {
        if (_member == null) return;
        if (_player.ActiveMovie is not ILingoMovie movie) return;
        var cast = movie.CastLib.GetCast(_member.CastLibNum);
        var items = cast.GetAll().OfType<T>().OrderBy(m => m.NumberInCast).ToList();
        int index = items.FindIndex(m => m == _member);
        if (index < 0) return;
        int target = index + offset;
        if (target < 0 || target >= items.Count) return;
        var next = items[target];
        _mediator.RaiseFindMember(next);
        _mediator.RaiseMemberSelected(next);
        SetMember(next);
    }
}

