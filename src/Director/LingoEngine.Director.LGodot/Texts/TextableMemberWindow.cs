using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using Godot;
using LingoEngine.Core;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.Texts;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.UI;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Members;
using LingoEngine.Texts;

namespace LingoEngine.Director.LGodot.Casts;

internal partial class DirGodotTextableMemberWindow : BaseGodotWindow, IHasMemberSelectedEvent, IDirFrameworkTextEditWindow, IFrameworkFor<DirectorTextEditWindow>
{
    private const int NavigationBarHeight = 20;
    private const int ActionBarHeight = 22;
    private readonly TextEdit _textEdit = new TextEdit();
    private readonly MemberNavigationBar<ILingoMemberTextBase> _navBar;
    private readonly TextEditIconBar _iconBar;
    private readonly AbstGodotPanel _topBar;

    private readonly ILingoPlayer _player;
    private readonly IDirectorIconManager _iconManager;
    private readonly IAbstFontManager _lingoFontManager;
    private ILingoMemberTextBase? _member;
    private const int _topOffset = 4;
    public DirGodotTextableMemberWindow(IDirectorEventMediator mediator, ILingoPlayer player, DirectorTextEditWindow directorTextEditWindow, IServiceProvider serviceProvider, IDirectorIconManager iconManager, IAbstFontManager lingoFontManager, ILingoFrameworkFactory factory)
        : base( "Edit Text", serviceProvider)
    {
        _player = player;
        _iconManager = iconManager;
        _lingoFontManager = lingoFontManager;
        mediator.Subscribe(this);
        Init(directorTextEditWindow);

        Size = new Vector2(450, 200);
        CustomMinimumSize = Size;

        _navBar = new MemberNavigationBar<ILingoMemberTextBase>(mediator, player, _iconManager, factory, NavigationBarHeight);
        AddChild(_navBar.Panel.Framework<AbstGodotWrapPanel>());
        _navBar.Panel.X = 0;
        _navBar.Panel.Y = TitleBarHeight;
        _navBar.Panel.Width = Size.X;
        _navBar.Panel.Height = NavigationBarHeight;

        _iconBar = directorTextEditWindow.IconBar;
        _iconBar.AlignmentChanged += a => SetAlignment(a);
        _iconBar.BoldChanged += v => ToggleStyle(LingoTextStyle.Bold, v);
        _iconBar.ItalicChanged += v => ToggleStyle(LingoTextStyle.Italic, v);
        _iconBar.UnderlineChanged += v => ToggleStyle(LingoTextStyle.Underline, v);
        _iconBar.FontSizeChanged += v =>
        {
            if (_member != null)
                _member.FontSize = v;
            _textEdit.AddThemeConstantOverride("font_size", v);
            ApplyStyleToEditor();
        };
        _iconBar.FontChanged += fontName =>
        {
            if (_member != null)
                _member.Font = fontName;
            var f = _lingoFontManager.Get<Font>(fontName);
            if (f != null)
                _textEdit.AddThemeFontOverride("font", f);
            ApplyStyleToEditor();
        };
        _iconBar.ColorChanged += c =>
        {
            _textEdit.AddThemeColorOverride("font_color", c.ToGodotColor());
            if (_member != null)
                _member.TextColor = c;
        };

        _iconBar.SetFonts(_lingoFontManager.GetAllNames());

        _topBar = _iconBar.Panel.Framework<AbstGodotPanel>();
        _topBar.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight + 2);
        AddChild(_topBar);

        _textEdit.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight + ActionBarHeight + _topOffset);
        _textEdit.Size = new Vector2(Size.X - 10, Size.Y - (TitleBarHeight + NavigationBarHeight + ActionBarHeight + 5));
        _textEdit.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _textEdit.SizeFlagsVertical = SizeFlags.ExpandFill;
        _textEdit.TextChanged += () =>
        {
            if (_member != null)
                _member.Text = _textEdit.Text;
        };
        AddChild(_textEdit);
        ApplyStyleToEditor();
    }

    public void MemberSelected(ILingoMember member)
    {
        if (member is ILingoMemberTextBase textMember)
            SetMemberValues(textMember);
    }

    private void SetMemberValues(ILingoMemberTextBase textMember)
    {
        _member = textMember;
        _textEdit.Text = textMember.Text.Replace("\r", "\r\n");
        _textEdit.AddThemeColorOverride("font_color", textMember.TextColor.ToGodotColor());
        _textEdit.AddThemeConstantOverride("font_size", textMember.FontSize);
        var font = _lingoFontManager.Get<Font>(textMember.Font);
        if (font != null)
            _textEdit.AddThemeFontOverride("font", font);

        _iconBar.SetMemberValues(textMember);
        _navBar.SetMember(textMember);
        ApplyStyleToEditor();
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _iconBar.OnResizing(size.X, size.Y);
        _navBar.Panel.Width = size.X;
        _navBar.Panel.Height = NavigationBarHeight;
        _textEdit.Size = new Vector2(size.X - 10, size.Y - (TitleBarHeight + NavigationBarHeight + ActionBarHeight + 5 + _topOffset));
    }

    private bool SetAlignment(AbstTextAlignment alignment)
    {
        if (_member != null)
            _member.Alignment = alignment;
        int val = alignment switch
        {
            AbstTextAlignment.Left => (int)HorizontalAlignment.Left,
            AbstTextAlignment.Center => (int)HorizontalAlignment.Center,
            AbstTextAlignment.Right => (int)HorizontalAlignment.Right,
            AbstTextAlignment.Justified => (int)HorizontalAlignment.Fill,
            _ => (int)HorizontalAlignment.Left
        };
        _textEdit.Set("alignment", val);
        return true;
    }

    private void ToggleStyle(LingoTextStyle style, bool on)
    {
        if (_member == null) return;
        switch (style)
        {
            case LingoTextStyle.Bold:
                _member.Bold = on; break;
            case LingoTextStyle.Italic:
                _member.Italic = on; break;
            case LingoTextStyle.Underline:
                _member.Underline = on; break;
        }
        ApplyStyleToEditor();
    }

    private void ApplyStyleToEditor()
    {
        var fontName = _iconBar.SelectedFont;
        var font = _lingoFontManager.Get<Font>(fontName);
        if (font != null)
        {
            var variation = new FontVariation { BaseFont = font };
            TextServer.FontStyle fs = 0;
            if (_iconBar.IsBold)
                fs |= TextServer.FontStyle.Bold;
            if (_iconBar.IsItalic)
                fs |= TextServer.FontStyle.Italic;
            // variation.FontStyle = fs;
            //_textEdit.AddThemeFontOverride("font", variation);
        }
        //_textEdit.UnderlineMode = _iconBar.IsUnderline ? UnderlineMode.Always : UnderlineMode.Disabled;
    }



}
