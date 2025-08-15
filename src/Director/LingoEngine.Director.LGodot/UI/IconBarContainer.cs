using AbstUI.LGodot.Primitives;
using Godot;
using LingoEngine.Director.Core.Styles;

namespace LingoEngine.Director.LGodot.UI
{
    public partial class IconBarContainer : MarginContainer
    {
        private HBoxContainer _container = new HBoxContainer();
        [Export] public Color BackgroundColor { get; set; } = DirectorColors.BG_WhiteMenus.ToGodotColor();
        [Export] public int PaddingLeft { get; set; } = 6;
        [Export] public int PaddingTop { get; set; } = 2;
        [Export] public int PaddingRight { get; set; } = 6;
        [Export] public int PaddingBottom { get; set; } = 2;


        public IconBarContainer()
        {
            base.AddChild(_container);
        }
        public override void _Ready()
        {
            AddThemeConstantOverride("margin_left", 5);
            AddThemeConstantOverride("margin_right", 5);
            AddThemeConstantOverride("margin_top", 2);
            AddThemeConstantOverride("margin_bottom", 2);
        }

        public override void _Draw()
        {
            DrawRect(new Rect2(Vector2.Zero, Size), BackgroundColor, filled: true);
        }

        public override void _Notification(int what)
        {
            if (what == NotificationThemeChanged || what == NotificationResized)
                QueueRedraw();
        }

        public new void AddChild(Node node, bool forceReadableName = false, Node.InternalMode @internal = 0)
        {
            _container.AddChild(node, forceReadableName, @internal);
        }

    }
}
