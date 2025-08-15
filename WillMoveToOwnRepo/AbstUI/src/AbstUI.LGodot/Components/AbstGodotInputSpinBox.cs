using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;

namespace AbstUI.LGodot.Components
{
    public partial class AbstGodotSpinBox : SpinBox, IAbstFrameworkSpinBox, IDisposable
    {
        private AMargin _margin = AMargin.Zero;
        private Action<float>? _onChange;

        public AbstGodotSpinBox(AbstInputSpinBox spin, IAbstFontManager lingoFontManager, Action<float>? onChange)
        {
            _onChange = onChange;
            spin.Init(this);
            ValueChanged += _ => _onValueChanged?.Invoke();
            if (_onChange != null) ValueChanged += _ => _onChange(Value);
        }
        private event Action? _onValueChanged;

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }

        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                AddThemeConstantOverride("margin_left", (int)_margin.Left);
                AddThemeConstantOverride("margin_right", (int)_margin.Right);
                AddThemeConstantOverride("margin_top", (int)_margin.Top);
                AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }

        public bool Enabled { get => Editable; set => Editable = value; }
        public new float Value { get => (float)base.Value; set => base.Value = value; }
        public float Min { get => (float)MinValue; set => MinValue = value; }
        public float Max { get => (float)MaxValue; set => MaxValue = value; }

        public object FrameworkNode => this;


        event Action? IAbstFrameworkNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        public new void Dispose()
        {
            ValueChanged -= _ => _onValueChanged?.Invoke();
            if (_onChange != null) ValueChanged -= _ => _onChange(Value);
            QueueFree();
            base.Dispose();
        }
    }
}
