namespace AbstUI.SDL2.Inputs
{
    //public class GlobalSDLAbstMouse : AbstMouse, IAbstGlobalMouse
    //{
    //    public GlobalSDLAbstMouse(IAbstGodotRootNode rootNode)
    //        : base(CreateFrameworkMouse(rootNode, out var framework))
    //    {
    //        framework.ReplaceMouseObj(this);
    //    }

    //    private static IAbstFrameworkMouse CreateFrameworkMouse(IAbstGodotRootNode rootNode, out AbstGodotGlobalMouse<GlobalGodotAbstMouse, AbstMouseEvent> framework)
    //    {
    //        framework = new AbstGodotGlobalMouse<GlobalGodotAbstMouse, AbstMouseEvent>(rootNode);
    //        return framework;
    //    }
    //}

    //public partial class AbstGodotGlobalMouse<TAbstMouseType, TAbstUIMouseEvent> : Node, IAbstFrameworkMouse
    //    where TAbstMouseType : AbstMouse<TAbstUIMouseEvent>
    //    where TAbstUIMouseEvent : AbstMouseEvent 
    //{
    //    private readonly AbstUIGodotMouse<TAbstMouseType, TAbstUIMouseEvent> _handler;
    //    private AbstMouse<TAbstUIMouseEvent>? _mouse;

    //    public AbstGodotGlobalMouse(IAbstGodotRootNode rootNode)
    //    {
    //        _handler = new AbstUIGodotMouse<TAbstMouseType,TAbstUIMouseEvent>(new Lazy<TAbstMouseType>(() => (TAbstMouseType)_mouse!));
    //        rootNode.RootNode.AddChild(this);
    //    }

    //    public override void _Input(InputEvent inputEvent)
    //    {
    //        var pos = GetViewport().GetMousePosition();
    //        if (inputEvent is InputEventMouseButton mouseButton)
    //        {
    //            _handler.HandleMouseButtonEvent(mouseButton, true, pos.X, pos.Y);
    //        }
    //        else if (inputEvent is InputEventMouseMotion mouseMotion)
    //        {
    //            _handler.HandleMouseMoveEvent(mouseMotion, true, pos.X, pos.Y);
    //        }
    //    }

    //    public void ReplaceMouseObj(IAbstMouse lingoMouse)
    //    {
    //        _mouse = (AbstMouse<TAbstUIMouseEvent>)lingoMouse;
    //        _handler.ReplaceMouseObj(lingoMouse);
    //    }
    //    public void Release() => GetParent()?.RemoveChild(this);
    //    public void HideMouse(bool state) => _handler.HideMouse(state);
    //    public void SetCursor(AMouseCursor value) => _handler.SetCursor(value);
    //}
}
