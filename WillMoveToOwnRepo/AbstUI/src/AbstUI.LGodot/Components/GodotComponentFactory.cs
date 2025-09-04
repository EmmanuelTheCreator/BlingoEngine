using AbstUI.Components;
using AbstUI.Components.Buttons;
using AbstUI.Components.Containers;
using AbstUI.Components.Graphics;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Texts;
using AbstUI.LGodot.Components.Graphics;
using AbstUI.LGodot.Components.Inputs;
using AbstUI.LGodot.Components.Menus;
using AbstUI.LGodot.Styles;
using AbstUI.Primitives;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Factory responsible for creating Godot specific GFX components.
    /// </summary>
    public class GodotComponentFactory : AbstComponentFactoryBase, IAbstComponentFactory
    {
        private readonly IAbstGodotStyleManager _godotStyleManager;
        private readonly IAbstGodotRootNode _rootNode;

        public GodotComponentFactory(IServiceProvider serviceProvider, IAbstGodotStyleManager godotStyleManager, IAbstGodotRootNode rootNode)
            : base(serviceProvider)
        {
            _godotStyleManager = godotStyleManager;
            _rootNode = rootNode;
        }
        public IAbstImagePainter CreateImagePainter(int width = 0, int height = 0)
           => new GodotImagePainter((AbstGodotFontManager)FontManager, width, height);
        public IAbstImagePainter CreateImagePainterToTexture(int width = 0, int height = 0)
           => new GodotImagePainterToTexture((AbstGodotFontManager)FontManager, width, height);
           
        public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height)
        {
            var canvas = new AbstGfxCanvas();
            var impl = new AbstGodotGfxCanvas(canvas, FontManager, width, height);
            InitComponent(canvas);
            canvas.Width = width;
            canvas.Height = height;
            canvas.Name = name;
            return canvas;
        }

        public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name)
        {
            var panel = new AbstWrapPanel(this);
            var impl = new AbstGodotWrapPanel(panel, orientation);
            InitComponent(panel);
            panel.Name = name;
            panel.Orientation = orientation;
            return panel;
        }

        public AbstPanel CreatePanel(string name)
        {
            var panel = new AbstPanel(this);
            var impl = new AbstGodotPanel(panel);
            InitComponent(panel);
            panel.Name = name;
            return panel;
        }

        public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y)
        {
            if (content is IAbstLayoutNode)
                throw new InvalidOperationException($"Content {content.Name} already supports layout — wrapping is unnecessary.");
            var panel = new AbstLayoutWrapper(content);
            var impl = new AbstGodotLayoutWrapper(panel);
            InitComponent(panel);
            if (x != null) panel.X = x.Value;
            if (y != null) panel.Y = y.Value;
            return panel;
        }

        public AbstTabContainer CreateTabContainer(string name)
        {
            var tab = new AbstTabContainer();
            var impl = new AbstGodotTabContainer(tab, _godotStyleManager);
            InitComponent(tab);
            tab.Name = name;
            return tab;
        }

        public AbstTabItem CreateTabItem(string name, string title)
        {
            var tab = new AbstTabItem();
            var impl = new AbstGodotTabItem(tab);
            InitComponent(tab);
            tab.Title = title;
            tab.Name = name;
            return tab;
        }

        public AbstScrollContainer CreateScrollContainer(string name)
        {
            var scroll = new AbstScrollContainer();
            var impl = new AbstGodotScrollContainer(scroll);
            InitComponent(scroll);
            scroll.Name = name;
            return scroll;
        }

        public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
        {
            var minNum = min.HasValue ? new NullableNum<float>(min.Value) : new NullableNum<float>();
            var maxNum = max.HasValue ? new NullableNum<float>(max.Value) : new NullableNum<float>();
            var stepNum = step.HasValue ? new NullableNum<float>(step.Value) : new NullableNum<float>();
            return CreateInputSlider(name, orientation, minNum, maxNum, stepNum, onChange);
        }

        public AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
        {
            var minNum = min.HasValue ? new NullableNum<int>(min.Value) : new NullableNum<int>();
            var maxNum = max.HasValue ? new NullableNum<int>(max.Value) : new NullableNum<int>();
            var stepNum = step.HasValue ? new NullableNum<int>(step.Value) : new NullableNum<int>();
            return CreateInputSlider(name, orientation, minNum, maxNum, stepNum, onChange);
        }

        public AbstInputSlider<TValue> CreateInputSlider<TValue>(string name, AOrientation orientation, NullableNum<TValue> min, NullableNum<TValue> max, NullableNum<TValue> step, Action<TValue>? onChange = null)
            where TValue : struct, System.Numerics.INumber<TValue>, IConvertible
        {
            var slider = new AbstInputSlider<TValue>();
            var impl = new AbstGodotInputSlider<TValue>(slider, orientation, onChange);
            InitComponent(slider);
            if (min.HasValue) slider.MinValue = min.Value!;
            if (max.HasValue) slider.MaxValue = max.Value!;
            if (step.HasValue) slider.Step = step.Value!;
            slider.Name = name;
            return slider;
        }

        public AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null, bool multiLine = false)
        {
            var input = new AbstInputText();
            var impl = new AbstGodotInputText(input, FontManager, onChange, multiLine);
            InitComponent(input);
            input.MaxLength = maxLength;
            input.Name = name;
            return input;
        }

        public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var minNullableNum = min.HasValue ? new NullableNum<float>(min.Value) : new NullableNum<float>();
            var maxNullableNum = max.HasValue ? new NullableNum<float>(max.Value) : new NullableNum<float>();
            return CreateInputNumber(name, minNullableNum, maxNullableNum, onChange);
        }

        public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
        {
            var minNullableNum = min.HasValue ? new NullableNum<int>(min.Value) : new NullableNum<int>();
            var maxNullableNum = max.HasValue ? new NullableNum<int>(max.Value) : new NullableNum<int>();
            return CreateInputNumber(name, minNullableNum, maxNullableNum, onChange);
        }

        public AbstInputNumber<TValue> CreateInputNumber<TValue>(string name, NullableNum<TValue> min, NullableNum<TValue> max, Action<TValue>? onChange = null)
             where TValue : System.Numerics.INumber<TValue>
        {
            var input = new AbstInputNumber<TValue>();
            var impl = new AbstGodotInputNumber<TValue>(input, FontManager, onChange);
            InitComponent(input);
            if (min.HasValue) input.Min = min.Value!;
            if (max.HasValue) input.Max = max.Value!;
            input.Name = name;
            return input;
        }

        public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var spin = new AbstInputSpinBox();
            var impl = new AbstGodotSpinBox(spin, FontManager, onChange);
            InitComponent(spin);
            spin.Name = name;
            if (min.HasValue) spin.Min = min.Value;
            if (max.HasValue) spin.Max = max.Value;
            return spin;
        }

        public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
        {
            var input = new AbstInputCheckbox();
            var impl = new AbstGodotInputCheckbox(input, onChange);
            InitComponent(input);
            input.Name = name;
            return input;
        }

        public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
        {
            var input = new AbstInputCombobox();
            var impl = new AbstGodotInputCombobox(input, FontManager, onChange);
            InitComponent(input);
            input.Name = name;
            return input;
        }

        public AbstItemList CreateItemList(string name, Action<string?>? onChange = null)
        {
            var list = new AbstItemList();
            var impl = new AbstGodotItemList(list, onChange);
            InitComponent(list);
            list.Name = name;
            return list;
        }

        public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null)
        {
            var picker = new AbstColorPicker();
            var impl = new AbstGodotColorPicker(picker, onChange);
            InitComponent(picker);
            picker.Name = name;
            return picker;
        }

        public AbstLabel CreateLabel(string name, string text = "")
        {
            var label = new AbstLabel();
            var impl = new AbstGodotLabel(label, FontManager);
            InitComponent(label);
            label.Text = text;
            label.Name = name;
            return label;
        }

        public AbstButton CreateButton(string name, string text = "")
        {
            var button = new AbstButton();
            var impl = new AbstGodotButton(button, FontManager);
            InitComponent(button);
            button.Name = name;
            if (!string.IsNullOrWhiteSpace(text))
                button.Text = text;
            return button;
        }

        public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null)
        {
            var button = new AbstStateButton();
            var impl = new AbstGodotStateButton(button, onChange);
            InitComponent(button);
            button.Name = name;
            if (!string.IsNullOrWhiteSpace(text))
                button.Text = text;
            if (texture != null)
                button.TextureOn = texture;
            return button;
        }

        public AbstMenu CreateMenu(string name)
        {
            var menu = new AbstMenu();
            var impl = new AbstGodotMenu(menu, name);
            InitComponent(menu);
            return menu;
        }

        public AbstMenuItem CreateMenuItem(string name, string? shortcut = null)
        {
            var item = new AbstMenuItem();
            var impl = new AbstGodotMenuItem(item, name, shortcut);
            return item;
        }

        public AbstMenu CreateContextMenu(object window)
        {
            var menu = CreateMenu("ContextMenu");
            if (window is Godot.Node node)
                node.AddChild(menu.Framework<AbstGodotMenu>());
            return menu;
        }

        public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
        {
            var sep = new AbstHorizontalLineSeparator();
            var impl = new AbstGodotHorizontalLineSeparator(sep);
            InitComponent(sep);
            sep.Name = name;
            return sep;
        }

        public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name)
        {
            var sep = new AbstVerticalLineSeparator();
            var impl = new AbstGodotVerticalLineSeparator(sep);
            InitComponent(sep);
            sep.Name = name;
            return sep;
        }

        //public AbstWindow CreateWindow(string name, string title = "")
        //{
        //    var win = new AbstWindow();
        //    var impl = new GodotWindow(win, _godotStyleManager, _rootNode);
        //    InitComponent(win);
        //    win.Name = name;
        //    if (!string.IsNullOrWhiteSpace(title))
        //        win.Title = title;
        //    return win;
        //}
    }
}
