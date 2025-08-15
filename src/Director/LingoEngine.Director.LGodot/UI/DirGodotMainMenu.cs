using Godot;
using LingoEngine.Core;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Tools;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Commands;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.Core.Casts.Commands;
using LingoEngine.Movies;
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Director.LGodot.Casts;
using LingoEngine.Director.Core.UI;
using LingoEngine.Inputs;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.Director.LGodot;

/// <summary>
/// Godot wrapper for <see cref="DirectorMainMenu"/>.
/// </summary>
internal partial class DirGodotMainMenu : Control, IDirFrameworkMainMenuWindow
{
    private readonly LingoGodotWrapPanel _menuBar;
    private readonly LingoGodotWrapPanel _iconBar;
    private Panel _bgColorPanel;
    private readonly IDirGodotWindowManager _windowManager;
    private readonly ILingoCommandManager _commandManager;
    private readonly LingoPlayer _player;

    protected StyleBoxFlat Style = new StyleBoxFlat();

    public bool IsOpen => true;
    public bool IsActiveWindow => true;
    public ILingoMouse Mouse { get; private set; }

    public DirGodotMainMenu(
        DirectorProjectManager projectManager,
        LingoPlayer player,
        IDirectorShortCutManager shortCutManager,
        IHistoryManager historyManager,
        IDirGodotWindowManager windowManager,
        ILingoCommandManager commandManager,
        DirectorMainMenu directorMainMenu, ILingoFrameworkFactory factory)
    {
        _windowManager = windowManager;
        _commandManager = commandManager;
        _player = player;
        var mouseFrameworkObj = new LingoGodotMouse(new Lazy<LingoMouse>(() => (LingoMouse)Mouse!));
        Mouse = new LingoMouse(mouseFrameworkObj);
        directorMainMenu.Init(this);

        _menuBar = directorMainMenu.MenuBar.Framework<LingoGodotWrapPanel>();
        _iconBar = directorMainMenu.IconBar.Framework<LingoGodotWrapPanel>();
        CreateBgColor();

        AddChild(_bgColorPanel);
        AddChild(_menuBar);
        AddChild(_iconBar);
        _menuBar.Position = new Vector2(10, 1);
        _iconBar.Position = new Vector2(300, 1);

        directorMainMenu.CallOnAllTopMenus(btn =>
        {
            AddChild(btn.Framework<LingoGodotMenu>());
        });

        StyleTopMenu(directorMainMenu);
        foreach (var childItem in _iconBar.GetChild(0).GetChildren())
        {
            if (childItem is Button btn)
            {
                StyleIconButton(btn);
            }
        }
       
    }

    private static void StyleIconButton(Button btn)
    {
        var topMenuBtnStyle = new StyleBoxFlat
        {
            BorderWidthLeft = 0,
            BorderWidthRight = 0,
            BorderWidthTop = 0,
            BorderWidthBottom = 0,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            BgColor = DirectorColors.BG_TopMenu.ToGodotColor(),
            ContentMarginLeft = 2,
            ContentMarginRight = 2,
        };
        btn.AddThemeStyleboxOverride("normal", topMenuBtnStyle);
        btn.Size = new Vector2(18, 18);
    }
    private static void StyleTopMenu(DirectorMainMenu directorMainMenu)
    {
        var topMenuBtnStyle = new StyleBoxFlat
        {
            BorderWidthLeft = 0,
            BorderWidthRight = 0,
            BorderWidthTop = 0,
            BorderWidthBottom = 0,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            BgColor = DirectorColors.BG_TopMenu.ToGodotColor(),
            ContentMarginLeft = 5,
            ContentMarginRight = 5,
        };
        var topMenuBtnStyle_hover = new StyleBoxFlat
        {
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            BorderWidthBottom = 0,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusTopLeft = 5,
            CornerRadiusTopRight = 5,
            BgColor = DirectorColors.BG_TopMenu.ToGodotColor(),
            ContentMarginLeft = 5,
            ContentMarginRight = 5,
        };
        
        directorMainMenu.CallOnAllTopMenuButtons(btn =>
        {
            var btnG = (Button)btn.Framework<LingoGodotButton>().FrameworkNode;
            btnG.AddThemeStyleboxOverride("normal", topMenuBtnStyle);
            btnG.AddThemeStyleboxOverride("hover", topMenuBtnStyle_hover);
            btnG.CustomMinimumSize = new Vector2(30, 18);
        });
    }

    private void CreateBgColor()
    {
        StyleBoxFlat panelStyle = new StyleBoxFlat();
        _bgColorPanel = new Panel();
        _bgColorPanel.Size = new Vector2(3000, 20);
        //_bgColorPanel.CustomMinimumSize = new Vector2(3000, 20);
        _bgColorPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _bgColorPanel.GrowHorizontal = GrowDirection.End;
        _bgColorPanel.Name = "MainMenuBackgroundColorPanel";
        panelStyle.BgColor = DirectorColors.BG_TopMenu.ToGodotColor(); ;
        _bgColorPanel.AddThemeStyleboxOverride("panel", panelStyle);
    }


    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Delete)
        {
            var active = _windowManager.ActiveWindow;
            if (active is DirGodotCastWindow castWin && _player.ActiveMovie is LingoMovie movie2)
            {
                var member = castWin.SelectedMember as LingoMember;
                if (member != null)
                {
                    var cast = (LingoCast)movie2.CastLib.GetCast(member.CastLibNum);
                    _commandManager.Handle(new RemoveMemberCommand(cast, member));
                }
            }
        }
    }
   


    public void OpenWindow()
    {
        // not allowed
    }
    public void CloseWindow()
    {
        // not allowed
    }
    public void MoveWindow(int x, int y)
    {
        // not allowed
    }

    public void SetPositionAndSize(int x, int y, int width, int height)
    {
        // not allowed
    }
    public void SetSize(int width, int height)
    {
        // not allowed
    }

    APoint IDirFrameworkWindow.GetPosition() => Position.ToLingoPoint();

    APoint IDirFrameworkWindow.GetSize() => Size.ToLingoPoint();

    
}
