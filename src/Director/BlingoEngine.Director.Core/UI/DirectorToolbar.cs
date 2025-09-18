using AbstUI.Primitives;
using AbstUI.Commands;
using BlingoEngine.Director.Core.Bitmaps;
using BlingoEngine.Director.Core.Bitmaps.Commands;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.FrameworkCommunication;
using AbstUI.Components.Containers;
using AbstUI.Components.Buttons;

namespace BlingoEngine.Director.Core.UI;

public abstract class DirectorToolbar<TToolEnumType>
    where TToolEnumType : Enum
{
    protected readonly IDirectorIconManager _iconManager;
    protected readonly IAbstCommandManager _commandManager;
    protected readonly IBlingoFrameworkFactory _factory;
    protected readonly AbstPanel _panel;
    protected readonly AbstWrapPanel _container;
    protected AbstStateButton? _selectedButton;

    public event Action<TToolEnumType>? ToolSelected;
    public AbstPanel Panel => _panel;
    public TToolEnumType SelectedTool { get; protected set; }

    public DirectorToolbar(string name, IDirectorIconManager iconManager, IAbstCommandManager commandManager, IBlingoFrameworkFactory factory)
    {
        _iconManager = iconManager;
        _commandManager = commandManager;
        _factory = factory;

        _panel = factory.CreatePanel(name);
        _panel.BackgroundColor = DirectorColors.BG_WhiteMenus;
        _panel.Width = 52;   // fallback size similar to Godot implementation
        _panel.Height = 200;

        _container = factory.CreateWrapPanel(AOrientation.Horizontal, "PaintToolbarContainer");
        _container.Width = _panel.Width - 2;
        _container.Height = _panel.Height - 2;
        _container.ItemMargin = new APoint(1,1);
        // TODO: custom minimum size (48,100) when supported
        // TODO: size flags ExpandFill/ShrinkBegin when supported

        /*
        _container = new HFlowContainer();
            _container.CustomMinimumSize = new Vector2(48, 100);
            _container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _container.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            _container.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
            _container.SizeFlagsVertical = SizeFlags.ShrinkBegin;
            _container.AddThemeConstantOverride("margin_left", 2);
            _container.AddThemeConstantOverride("margin_top", 2);
            AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = BGColor }); 
        */

        //_container.ItemMargin = new BlingoMargin(2, 2, 0, 0); // margin_left/top
        _panel.AddItem(_container);

        ToolSelected?.Invoke(SelectedTool);
    }
   
    protected void AddToolButton(DirectorIcon icon, Func<TToolEnumType, IAbstCommand> toCommand)
    {
        var btn = _factory.CreateStateButton(icon.ToString(), _iconManager.Get(icon));
        btn.Width = 20; // approximate size
        btn.Height = 20;
        btn.ValueChanged += () =>
        {
            if (!btn.IsOn) return;
            SelectButton(btn);
            TToolEnumType tool = ConvertToTool(icon);

            SelectedTool = tool;
            _commandManager.Handle(toCommand(tool));
            ToolSelected?.Invoke(tool);
        };

        _container.AddItem(btn);
    }

    protected abstract TToolEnumType ConvertToTool(DirectorIcon icon);
   

    public void SelectTool(TToolEnumType tool)
    {
        if (SelectedTool.Equals(tool)) return;
        SelectedTool = tool;
        ToolSelected?.Invoke(tool);
    }

    private void SelectButton(AbstStateButton btn)
    {
        if (_selectedButton == btn) return;
        if (_selectedButton != null)
            _selectedButton.IsOn = false;

        btn.IsOn = true;
        _selectedButton = btn;
    }


}

