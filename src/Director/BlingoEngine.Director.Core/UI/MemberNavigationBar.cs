using AbstUI.Components.Buttons;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Texts;
using AbstUI.Primitives;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Members;
using BlingoEngine.Movies;

namespace BlingoEngine.Director.Core.UI;

public class MemberNavigationBar<T> : MemberNavigationBar where T : class, IBlingoMember
{
    public MemberNavigationBar(IDirectorEventMediator mediator, IBlingoPlayer player, IDirectorIconManager iconManager, IBlingoFrameworkFactory factory, int barHeight = 20) : base(mediator, player, iconManager, factory, barHeight)
    {
    }
    protected override void Navigate(int offset)
    {
        if (_member == null) return;
        if (_player.ActiveMovie is not IBlingoMovie movie) return;
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

public class MemberNavigationBar
{
    private string _memberName = "";
    private readonly IDirectorIconManager _iconManager;
    private readonly AbstWrapPanel _panel;
    private AbstButton _typeIcon;
    private AbstLabel _numberLabel;
    private AbstLabel _castLibLabel;
    private AbstInputText _nameEdit;
    protected readonly IBlingoPlayer _player;

    public int Height { get; private set; }

    protected readonly IDirectorEventMediator _mediator;
    protected IBlingoMember? _member;

    public AbstWrapPanel Panel => _panel;
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
    public MemberNavigationBar(IDirectorEventMediator mediator, IBlingoPlayer player, IDirectorIconManager iconManager, IBlingoFrameworkFactory factory, int barHeight = 20)
#pragma warning restore CS8618 
    {
        Height = barHeight;
        _mediator = mediator;
        _player = player;
        _iconManager = iconManager;
        _panel = factory.CreateWrapPanel(AOrientation.Horizontal, "MemberNavigationBar");
        _panel.Height = barHeight;
        _panel.Width = 400;

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

    public void SetMember(IBlingoMember member)
    {
        if (member == _member) return;
        _member = member;
        _memberName = member.Name;
        _nameEdit.Text = _memberName;
        _numberLabel.Text = member.NumberInCast.ToString();
        _castLibLabel.Text = GetCastName(member);
        var icon = BlingoMemberTypeIcons.GetIconType(member);
        _typeIcon.IconTexture = icon.HasValue ? _iconManager.Get(icon.Value) : null;
    }

    private string GetCastName(IBlingoMember m)
    {
        if (_player.ActiveMovie is IBlingoMovie movie)
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

    protected virtual void Navigate(int offset)
    {
        if (_member == null) return;
        if (_player.ActiveMovie is not IBlingoMovie movie) return;
        var cast = movie.CastLib.GetCast(_member.CastLibNum);
        var items = cast.GetAll().ToList();
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


