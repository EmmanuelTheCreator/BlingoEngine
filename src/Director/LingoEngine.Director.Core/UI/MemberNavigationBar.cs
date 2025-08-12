using LingoEngine.Core;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Members;
using LingoEngine.Movies;
using System;
using System.Linq;

namespace LingoEngine.Director.Core.UI;

public class MemberNavigationBar<T> where T : class, ILingoMember
{
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoPlayer _player;
    private readonly IDirectorIconManager _iconManager;
    private readonly LingoGfxWrapPanel _panel;
    private readonly LingoGfxButton _typeIcon;
    private readonly LingoGfxInputText _nameEdit;
    private readonly LingoGfxLabel _numberLabel;
    private readonly LingoGfxLabel _castLibLabel;

    private T? _member;

    public LingoGfxWrapPanel Panel => _panel;

    public MemberNavigationBar(IDirectorEventMediator mediator, ILingoPlayer player, IDirectorIconManager iconManager, ILingoFrameworkFactory factory, int barHeight = 20)
    {
        _mediator = mediator;
        _player = player;
        _iconManager = iconManager;
        _panel = factory.CreateWrapPanel(LingoOrientation.Horizontal, "MemberNavigationBar");
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
            });

        _typeIcon = factory.CreateButton("TypeIcon", string.Empty);
        _typeIcon.Width = barHeight;
        _typeIcon.Height = barHeight;
        _typeIcon.Enabled = false;
        _panel.AddItem(_typeIcon);

        _nameEdit = factory.CreateInputText("NameEdit", 0, t => { if (_member != null) _member.Name = t; });
        _nameEdit.Width = 100;
        _panel.AddItem(_nameEdit);

        _numberLabel = factory.CreateLabel("NumberLabel", string.Empty);
        _numberLabel.Width = 40;
        _panel.AddItem(_numberLabel);

        _panel.Compose().AddButton("InfoButton", string.Empty, OnInfo, b =>
        {
            b.IconTexture = iconManager.Get(DirectorIcon.Info);
            b.Width = barHeight;
            b.Height = barHeight;
        });

        _castLibLabel = factory.CreateLabel("CastLibLabel", string.Empty);
        _castLibLabel.Width = 80;
        _panel.AddItem(_castLibLabel);
    }

    public void SetMember(T member)
    {
        _member = member;
        _nameEdit.Text = member.Name;
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

