using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Styles;

namespace AbstUI.SDL2
{
    /// <summary>
    /// Factory responsible for creating SDL specific GFX components.
    /// </summary>
    public class AbstSdlComponentFactory : IAbstComponentFactory
    {
        private readonly ISdlRootComponentContext _rootContext;
        private readonly SdlFontManager _fontManager;

        public ISdlRootComponentContext RootContext => _rootContext;
        public SdlFocusManager FocusManager => _rootContext.FocusManager;

        public AbstSdlComponentFactory(ISdlRootComponentContext rootContext, SdlFontManager fontManager)
        {
            _rootContext = rootContext;
            _fontManager = fontManager;
        }

        public AbstSDLComponentContext CreateContext(IAbstSDLComponent component, AbstSDLComponentContext? parent = null)
            => new(_rootContext.ComponentContainer, component, parent) { Renderer = _rootContext.Renderer };

        public AbstSDLRenderContext CreateRenderContext(IAbstSDLComponent? component = null)
            => new(_rootContext.Renderer, System.Numerics.Vector2.Zero, _fontManager);

        public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height)
        {
            var canvas = new AbstGfxCanvas();
            var impl = new AbstSdlGfxCanvas(this, _fontManager, width, height);
            canvas.Init(impl);
            canvas.Width = width;
            canvas.Height = height;
            canvas.Name = name;
            return canvas;
        }

        public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name)
        {
            var panel = new AbstWrapPanel(this);
            var impl = new AbstSdlWrapPanel(this, orientation);
            panel.Init(impl);
            panel.Name = name;
            panel.Orientation = orientation;
            return panel;
        }

        public AbstPanel CreatePanel(string name)
        {
            var panel = new AbstPanel(this);
            var impl = new AbstSdlPanel(this);
            panel.Init(impl);
            panel.Name = name;
            return panel;
        }

        public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y)
        {
            if (content is IAbstLayoutNode)
                throw new InvalidOperationException($"Content {content.Name} already supports layout — wrapping is unnecessary.");
            var panel = new AbstLayoutWrapper(content);
            var impl = new AbstSdlLayoutWrapper(this, panel);
            if (x != null) panel.X = x.Value;
            if (y != null) panel.Y = y.Value;
            return panel;
        }

        public AbstTabContainer CreateTabContainer(string name)
        {
            var tab = new AbstTabContainer();
            var impl = new AbstSdlTabContainer(this);
            tab.Init(impl);
            tab.Name = name;
            return tab;
        }

        public AbstTabItem CreateTabItem(string name, string title)
        {
            var tab = new AbstTabItem();
            var impl = new AbstSdlTabItem(this, tab);
            tab.Title = title;
            tab.Name = name;
            return tab;
        }

        public AbstScrollContainer CreateScrollContainer(string name)
        {
            var scroll = new AbstScrollContainer();
            var impl = new AbstSdlScrollContainer(this);
            scroll.Init(impl);
            scroll.Name = name;
            return scroll;
        }

        public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
        {
            var slider = new AbstInputSlider<float>();
            var impl = new AbstSdlInputSlider<float>(this);
            slider.Init(impl);
            slider.Name = name;
            if (min.HasValue) slider.MinValue = min.Value;
            if (max.HasValue) slider.MaxValue = max.Value;
            if (step.HasValue) slider.Step = step.Value;
            if (onChange != null)
                slider.ValueChanged += () => onChange(slider.Value);
            return slider;
        }

        public AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
        {
            var slider = new AbstInputSlider<int>();
            var impl = new AbstSdlInputSlider<int>(this);
            slider.Init(impl);
            slider.Name = name;
            if (min.HasValue) slider.MinValue = min.Value;
            if (max.HasValue) slider.MaxValue = max.Value;
            if (step.HasValue) slider.Step = step.Value;
            if (onChange != null)
                slider.ValueChanged += () => onChange(slider.Value);
            return slider;
        }

        public AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null, bool multiLine = false)
        {
            var input = new AbstInputText();
            var impl = new AbstSdlInputText(this, multiLine);
            input.Init(impl);
            input.Name = name;
            input.MaxLength = maxLength;
            if (onChange != null)
                input.ValueChanged += () => onChange(input.Text);
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
            throw new NotImplementedException();
            var input = new AbstInputNumber<TValue>();
            //var impl = new AbstSdlInputNumber<float>(_rootContext.Renderer);
            //input.Init(impl);
            //input.Name = name;
            //if (min.HasValue) input.Min = min.Value;
            //if (max.HasValue) input.Max = max.Value;
            return input;
        }

        public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var spin = new AbstInputSpinBox();
            var impl = new AbstSdlSpinBox(this);
            spin.Init(impl);
            spin.Name = name;
            if (min.HasValue) spin.Min = min.Value;
            if (max.HasValue) spin.Max = max.Value;
            if (onChange != null)
                spin.ValueChanged += () => onChange(spin.Value);
            return spin;
        }

        public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
        {
            var input = new AbstInputCheckbox();
            var impl = new AbstSdlInputCheckbox(this);
            input.Init(impl);
            input.Name = name;
            input.ValueChanged += () => onChange?.Invoke(input.Checked);
            return input;
        }

        public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
        {
            var input = new AbstInputCombobox();
            var impl = new AbstSdlInputCombobox(this);
            input.Init(impl);
            input.Name = name;
            input.ValueChanged += () => onChange?.Invoke(input.SelectedKey);
            return input;
        }

        public AbstItemList CreateItemList(string name, Action<string?>? onChange = null)
        {
            var list = new AbstItemList();
            var impl = new AbstSdInputltemList(this);
            list.Init(impl);
            list.Name = name;
            if (onChange != null)
                list.ValueChanged += () => onChange(list.SelectedKey);
            return list;
        }

        public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null)
        {
            var picker = new AbstColorPicker();
            var impl = new AbstSdlColorPicker(this);
            picker.Init(impl);
            picker.Name = name;
            if (onChange != null)
                picker.ValueChanged += () => onChange(picker.Color);
            return picker;
        }

        public AbstLabel CreateLabel(string name, string text = "")
        {
            var label = new AbstLabel();
            var impl = new AbstSdlLabel(this);
            label.Init(impl);
            label.Name = name;
            label.Text = text;
            return label;
        }

        public AbstButton CreateButton(string name, string text = "")
        {
            var button = new AbstButton();
            var impl = new AbstSdlButton(this);
            button.Init(impl);
            button.Name = name;
            button.Text = text;
            return button;
        }

        public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null)
        {
            var button = new AbstStateButton();
            var impl = new AbstSdlStateButton(this);
            if (onChange != null)
                button.ValueChanged += () => onChange(button.IsOn);
            button.Init(impl);
            button.Name = name;
            button.Text = text;
            if (texture != null)
                button.TextureOn = texture;
            return button;
        }

        public AbstMenu CreateMenu(string name)
        {
            var menu = new AbstMenu();
            var impl = new AbstSdlMenu(this, name);
            menu.Init(impl);
            return menu;
        }


        public AbstWindow CreateWindow(string name, string title = "")
        {

            var win = new AbstWindow();
            var impl = new AbstSdlWindow(win, this);
            win.Name = name;
            win.Title = title;
            return win;
        }


        public AbstMenuItem CreateMenuItem(string name, string? shortcut = null)
        {
            var item = new AbstMenuItem();
            var impl = new AbstSdlMenuItem(this, name, shortcut);
            item.Init(impl);
            return item;
        }

        public AbstMenu CreateContextMenu(object window)
        {
            var menu = CreateMenu("ContextMenu");
            return menu;
        }

        public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
        {
            throw new NotImplementedException();
        }

        public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name)
        {
            throw new NotImplementedException();
        }


    }
}
