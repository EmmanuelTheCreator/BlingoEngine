using Godot;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Core;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.UI;
using AbstUI.LGodot.Components;

namespace LingoEngine.Director.LGodot.Inspector;

public partial class DirGodotPropertyInspector : BaseGodotWindow, IDirFrameworkPropertyInspectorWindow
{

    private readonly DirectorPropertyInspectorWindow _inspectorWindow;
    private AbstGodotPanel _headerPanel;

    public DirGodotPropertyInspector(DirectorPropertyInspectorWindow inspectorWindow, ILingoPlayer player, IDirGodotWindowManager windowManager, IDirectorIconManager iconManager)
        : base(DirectorMenuCodes.PropertyInspector, "Property Inspector", windowManager)
    {
        _inspectorWindow = inspectorWindow;

        Size = new Vector2(260, 450);
        _inspectorWindow.Init(this, Size.X, Size.Y, TitleBarHeight);
        CustomMinimumSize = Size;

        _headerPanel = _inspectorWindow.HeaderPanel.Framework<AbstGodotPanel>();
        _headerPanel.Position = new Vector2(0, TitleBarHeight);
        _headerPanel.Width = Size.X - 10;
        AddChild(_headerPanel);


        var tabs = _inspectorWindow.Tabs.Framework<AbstGodotTabContainer>();
        tabs.Position = new Vector2(0, TitleBarHeight + DirectorPropertyInspectorWindow.HeaderHeight);
        tabs.Size = new Vector2(Size.X, Size.Y - 30 - DirectorPropertyInspectorWindow.HeaderHeight);
        AddChild(tabs);

        //var behaviorPanel = _inspectorWindow.BehaviorPanel.Framework<LingoGodotPanel>();
        //behaviorPanel.Visibility = false;
        //behaviorPanel.Position = new Vector2(0, TitleBarHeight + DirectorPropertyInspectorWindow.HeaderHeight);
        //behaviorPanel.Size = new Vector2(Size.X - 10, 0);
        //AddChild(behaviorPanel);

    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);

        _inspectorWindow.OnResizing(size.X, size.Y);

    }

}
