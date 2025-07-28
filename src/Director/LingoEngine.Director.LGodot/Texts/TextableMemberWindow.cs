using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Texts;
using LingoEngine.Members;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Core;
using LingoEngine.Movies;
using System.Linq;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.LGodot.Icons;
using LingoEngine.Director.Core.Texts;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.UI;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Styles;
using LingoEngine.Director.LGodot.UI;
using LingoEngine.LGodot.Gfx;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using static Godot.LinkButton;

namespace LingoEngine.Director.LGodot.Casts;

internal partial class DirGodotTextableMemberWindow : BaseGodotWindow, IHasMemberSelectedEvent, IDirFrameworkTextEditWindow
{
    private const int NavigationBarHeight = 20;
    private const int ActionBarHeight = 22;
    private readonly TextEdit _textEdit = new TextEdit();
    private readonly MemberNavigationBar<ILingoMemberTextBase> _navBar;
    private readonly LingoGfxStateButton _alignLeft;
    private readonly LingoGfxStateButton _alignCenter;
    private readonly LingoGfxStateButton _alignRight;
    private readonly LingoGfxStateButton _alignJustified;
    private readonly LingoGfxStateButton _boldButton;
    private readonly LingoGfxStateButton _italicButton;
    private readonly LingoGfxStateButton _underlineButton;
    private readonly ColorRect _colorDisplay = new ColorRect();
    private readonly ColorPickerButton _colorPicker = new ColorPickerButton();
    private readonly SpinBox _fontSize = new SpinBox();
    private readonly OptionButton _fontsCombo = new OptionButton();
    private readonly IconBarContainer _topBar = new IconBarContainer();

    private readonly ILingoPlayer _player;
    private readonly IDirectorIconManager _iconManager;
    private readonly ILingoFontManager _lingoFontManager;
    private ILingoMemberTextBase? _member;
    private List<string> _allFonts;
    private const int _topOffset = 4;
    public DirGodotTextableMemberWindow(IDirectorEventMediator mediator, ILingoPlayer player, DirectorTextEditWindow directorTextEditWindow, IDirGodotWindowManager windowManager, IDirectorIconManager iconManager, ILingoFontManager lingoFontManager, ILingoFrameworkFactory frameworkFactory)
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

        _topBar.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight+2);
        AddChild(_topBar);

        _alignLeft = frameworkFactory.CreateStateButton(Name + "_alignLeft", null,"L", (s) => SetAlignment(LingoTextAlignment.Left));
        _alignCenter = frameworkFactory.CreateStateButton(Name + "_alignCenter", null, "C", (s) => SetAlignment(LingoTextAlignment.Center));
        _alignRight = frameworkFactory.CreateStateButton(Name + "_alignRight", null, "R", (s) => SetAlignment(LingoTextAlignment.Right));
        _alignJustified = frameworkFactory.CreateStateButton(Name + "_alignJustified", null, "J", (s) => SetAlignment(LingoTextAlignment.Justified));
        _topBar.AddChild(_alignLeft.Framework<LingoGodotStateButton>());
        _topBar.AddChild(_alignCenter.Framework<LingoGodotStateButton>());
        _topBar.AddChild(_alignRight.Framework<LingoGodotStateButton>());
        _topBar.AddChild(_alignJustified.Framework<LingoGodotStateButton>());

        _boldButton = frameworkFactory.CreateStateButton(Name + "_bold", null, "B", v => ToggleStyle(LingoTextStyle.Bold, v));
        _italicButton = frameworkFactory.CreateStateButton(Name + "_italic", null, "I", v => ToggleStyle(LingoTextStyle.Italic, v));
        _underlineButton = frameworkFactory.CreateStateButton(Name + "_underline", null, "U", v => ToggleStyle(LingoTextStyle.Underline, v));
        _topBar.AddChild(_boldButton.Framework<LingoGodotStateButton>());
        _topBar.AddChild(_italicButton.Framework<LingoGodotStateButton>());
        _topBar.AddChild(_underlineButton.Framework<LingoGodotStateButton>());

        _fontSize.MinValue = 1;
        _fontSize.MaxValue = 200;
        _fontSize.CustomMinimumSize = new Vector2(50, 16);
        _fontSize.ValueChanged += v =>
        {
            if (_member != null)
                _member.FontSize = (int)v;
            _textEdit.AddThemeConstantOverride("font_size", (int)v);
            ApplyStyleToEditor();
        };
        _topBar.AddChild(_fontSize);

        _allFonts = _lingoFontManager.GetAllNames().ToList();
        foreach (var font in _allFonts)
            _fontsCombo.AddItem(font);
        _fontsCombo.ItemSelected += id =>
        {
            var fontName = _fontsCombo.GetItemText((int)id);
            if (_member != null)
                _member.Font = fontName;
            var f = _lingoFontManager.Get<Font>(fontName);
            if (f != null)
                _textEdit.AddThemeFontOverride("font", f);
            ApplyStyleToEditor();
        };
        _topBar.AddChild(_fontsCombo);

        _colorDisplay.CustomMinimumSize = new Vector2(ActionBarHeight, ActionBarHeight);
        _topBar.AddChild(_colorDisplay);
        _colorPicker.CustomMinimumSize = new Vector2(ActionBarHeight, ActionBarHeight);
        _colorPicker.ColorChanged += c => OnColorChanged(c);
        _topBar.AddChild(_colorPicker);

        _textEdit.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight + ActionBarHeight+ _topOffset);
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
        if (member is not ILingoMemberTextBase textMember)
            return;
        
        _member = textMember;
        _textEdit.Text = textMember.Text.Replace("\r", "\r\n");
        _textEdit.AddThemeColorOverride("font_color", textMember.TextColor.ToGodotColor());
        _textEdit.AddThemeConstantOverride("font_size", textMember.FontSize);
        var font = _lingoFontManager.Get<Font>(textMember.Font);
        if (font != null)
        {
            _textEdit.AddThemeFontOverride("font", font);
            _fontsCombo.Selected = _allFonts.IndexOf(textMember.Font);
        }
        _colorDisplay.Color = textMember.TextColor.ToGodotColor();
        _colorPicker.Color = textMember.TextColor.ToGodotColor();
        _fontSize.Value = textMember.FontSize;
        _navBar.SetMember(textMember);
        _alignLeft.IsOn = false;
        _alignCenter.IsOn = false;
        _alignRight.IsOn = false;
        _alignJustified.IsOn = false;
        _boldButton.IsOn = textMember.Bold;
        _italicButton.IsOn = textMember.Italic;
        _underlineButton.IsOn = textMember.Underline;
        switch (textMember.Alignment)
        {
            case LingoTextAlignment.Left:_alignLeft.IsOn = true;break;
            case LingoTextAlignment.Center: _alignCenter.IsOn = true; break;
            case LingoTextAlignment.Right: _alignRight.IsOn = true; break;
            case LingoTextAlignment.Justified: _alignJustified.IsOn = true; break;
            default:
                break;
        }
        ApplyStyleToEditor();
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _navBar.CustomMinimumSize = new Vector2(size.X, NavigationBarHeight);
        _textEdit.Size = new Vector2(size.X - 10, size.Y - (TitleBarHeight + NavigationBarHeight + ActionBarHeight + 5+ _topOffset));
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

    private void OnColorChanged(Color color)
    {
        _colorDisplay.Color = color;
        _textEdit.AddThemeColorOverride("font_color", color);
        if (_member != null)
            _member.TextColor = color.ToLingoColor();
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
        var fontName = _fontsCombo.GetItemText(_fontsCombo.Selected);
        var font = _lingoFontManager.Get<Font>(fontName);
        if (font != null)
        {
            var variation = new FontVariation { BaseFont = font };
            TextServer.FontStyle fs = 0;
            if (_boldButton.IsOn)
                fs |= TextServer.FontStyle.Bold;
            if (_italicButton.IsOn)
                fs |= TextServer.FontStyle.Italic;
           // variation.FontStyle = fs;
            //_textEdit.AddThemeFontOverride("font", variation);
        }
        //_textEdit.UnderlineMode = _underlineButton.IsOn ? UnderlineMode.Always : UnderlineMode.Disabled;
    }


   
}
