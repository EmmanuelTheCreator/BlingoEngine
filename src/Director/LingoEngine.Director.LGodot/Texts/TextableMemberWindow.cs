using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Texts;
using LingoEngine.Members;
using LingoEngine.Core;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Texts;
using LingoEngine.Director.LGodot.Icons;
using LingoEngine.Director.LGodot.UI;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Gfx;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Styles;

namespace LingoEngine.Director.LGodot.Casts;

internal partial class DirGodotTextableMemberWindow : BaseGodotWindow, IHasMemberSelectedEvent, IDirFrameworkTextEditWindow
{
    private const int NavigationBarHeight = 20;
    private const int ActionBarHeight = 22;
    private readonly TextEdit _textEdit = new TextEdit();
    private readonly MemberNavigationBar<ILingoMemberTextBase> _navBar;
    private readonly TextEditIconBar _iconBar;
    private readonly LingoGodotPanel _topBar;

    private readonly ILingoPlayer _player;
    private readonly IDirectorIconManager _iconManager;
    private readonly ILingoFontManager _lingoFontManager;
    private ILingoMemberTextBase? _member;
    private const int _topOffset = 4;
    public DirGodotTextableMemberWindow(IDirectorEventMediator mediator, ILingoPlayer player, DirectorTextEditWindow directorTextEditWindow, IDirGodotWindowManager windowManager, IDirectorIconManager iconManager, ILingoFontManager lingoFontManager)
        : base(DirectorMenuCodes.TextEditWindow, "Edit Text", windowManager)
    {
        _player = player;
        _iconManager = iconManager;
        _lingoFontManager = lingoFontManager;
        mediator.Subscribe(this);
        directorTextEditWindow.Init(this);

        Size = new Vector2(450, 200);
        CustomMinimumSize = Size;

        _navBar = new MemberNavigationBar<ILingoMemberTextBase>(mediator, player, _iconManager, NavigationBarHeight);
        AddChild(_navBar);
        _navBar.Position = new Vector2(0, TitleBarHeight);
        _navBar.CustomMinimumSize = new Vector2(Size.X, NavigationBarHeight);

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

        _topBar = _iconBar.Panel.Framework<LingoGodotPanel>();
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
        _navBar.CustomMinimumSize = new Vector2(size.X, NavigationBarHeight);
        _iconBar.Panel.Width = size.X;
        _textEdit.Size = new Vector2(size.X - 10, size.Y - (TitleBarHeight + NavigationBarHeight + ActionBarHeight + 5 + _topOffset));
    }

    private bool SetAlignment(LingoTextAlignment alignment)
    {
        if (_member != null)
            _member.Alignment = alignment;
        int val = alignment switch
        {
            LingoTextAlignment.Left => (int)HorizontalAlignment.Left,
            LingoTextAlignment.Center => (int)HorizontalAlignment.Center,
            LingoTextAlignment.Right => (int)HorizontalAlignment.Right,
            LingoTextAlignment.Justified => (int)HorizontalAlignment.Fill,
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
