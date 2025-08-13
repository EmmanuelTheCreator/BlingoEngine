using System;
using LingoEngine.Bitmaps;
using LingoEngine.Gfx;
using LingoEngine.Inputs;
using LingoEngine.Primitives;
using LingoEngine.Styles;

namespace LingoEngine.SDL2.Gfx
{
    /// <summary>
    /// Factory responsible for creating SDL specific GFX components.
    /// </summary>
    public class SdlGfxFactory : ILingoGfxFactory
    {
        private readonly ISdlRootComponentContext _rootContext;
        private readonly ILingoFontManager _fontManager;

        public ISdlRootComponentContext RootContext => _rootContext;

        public SdlGfxFactory(ISdlRootComponentContext rootContext, ILingoFontManager fontManager)
        {
            _rootContext = rootContext;
            _fontManager = fontManager;
        }

        internal LingoSDLComponentContext CreateContext(ILingoSDLComponent component, LingoSDLComponentContext? parent = null)
            => new(_rootContext.ComponentContainer, component, parent) { Renderer = _rootContext.Renderer };

        internal LingoSDLRenderContext CreateRenderContext(ILingoSDLComponent? component = null)
            => new(_rootContext.Renderer, _rootContext.ImGuiViewPort, _rootContext.ImDrawList, _rootContext.ImGuiViewPort.WorkPos);

        public LingoGfxCanvas CreateGfxCanvas(string name, int width, int height)
        {
            var canvas = new LingoGfxCanvas();
            var impl = new SdlGfxCanvas(this, _fontManager, width, height);
            canvas.Init(impl);
            canvas.Width = width;
            canvas.Height = height;
            canvas.Name = name;
            return canvas;
        }

        public LingoGfxWrapPanel CreateWrapPanel(LingoOrientation orientation, string name)
        {
            var panel = new LingoGfxWrapPanel(this);
            var impl = new SdlGfxWrapPanel(this, orientation);
            panel.Init(impl);
            panel.Name = name;
            panel.Orientation = orientation;
            return panel;
        }

        public LingoGfxPanel CreatePanel(string name)
        {
            var panel = new LingoGfxPanel(this);
            var impl = new SdlGfxPanel(this);
            panel.Init(impl);
            panel.Name = name;
            return panel;
        }

        public LingoGfxLayoutWrapper CreateLayoutWrapper(ILingoGfxNode content, float? x, float? y)
        {
            if (content is ILingoGfxLayoutNode)
                throw new InvalidOperationException($"Content {content.Name} already supports layout — wrapping is unnecessary.");
            var panel = new LingoGfxLayoutWrapper(content);
            var impl = new LingoSdlLayoutWrapper(this,panel);
            if (x != null) panel.X = x.Value;
            if (y != null) panel.Y = y.Value;
            return panel;
        }

        public LingoGfxTabContainer CreateTabContainer(string name)
        {
            var tab = new LingoGfxTabContainer();
            var impl = new SdlGfxTabContainer(this);
            tab.Init(impl);
            tab.Name = name;
            return tab;
        }

        public LingoGfxTabItem CreateTabItem(string name, string title)
        {
            var tab = new LingoGfxTabItem();
            var impl = new SdlGfxTabItem(this, tab);
            tab.Title = title;
            tab.Name = name;
            return tab;
        }

        public LingoGfxScrollContainer CreateScrollContainer(string name)
        {
            var scroll = new LingoGfxScrollContainer();
            var impl = new SdlGfxScrollContainer(this);
            scroll.Init(impl);
            scroll.Name = name;
            return scroll;
        }

        public LingoGfxInputSlider<float> CreateInputSliderFloat(LingoOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
        {
            var slider = new LingoGfxInputSlider<float>();
            var impl = new SdlGfxInputSlider<float>(this);
            slider.Init(impl);
            slider.Name = name;
            if (min.HasValue) slider.MinValue = min.Value;
            if (max.HasValue) slider.MaxValue = max.Value;
            if (step.HasValue) slider.Step = step.Value;
            if (onChange != null)
                slider.ValueChanged += () => onChange(slider.Value);
            return slider;
        }

        public LingoGfxInputSlider<int> CreateInputSliderInt(LingoOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
        {
            var slider = new LingoGfxInputSlider<int>();
            var impl = new SdlGfxInputSlider<int>(this);
            slider.Init(impl);
            slider.Name = name;
            if (min.HasValue) slider.MinValue = min.Value;
            if (max.HasValue) slider.MaxValue = max.Value;
            if (step.HasValue) slider.Step = step.Value;
            if (onChange != null)
                slider.ValueChanged += () => onChange(slider.Value);
            return slider;
        }

        public LingoGfxInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null)
        {
            var input = new LingoGfxInputText();
            var impl = new SdlGfxInputText(this);
            input.Init(impl);
            input.Name = name;
            input.MaxLength = maxLength;
            if (onChange != null)
                input.ValueChanged += () => onChange(input.Text);
            return input;
        }

        public LingoGfxInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var minNullableNum = min.HasValue ? new NullableNum<float>(min.Value) : new NullableNum<float>();
            var maxNullableNum = max.HasValue ? new NullableNum<float>(max.Value) : new NullableNum<float>();
            return CreateInputNumber(name, minNullableNum, maxNullableNum, onChange);
        }

        public LingoGfxInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
        {
            var minNullableNum = min.HasValue ? new NullableNum<int>(min.Value) : new NullableNum<int>();
            var maxNullableNum = max.HasValue ? new NullableNum<int>(max.Value) : new NullableNum<int>();
            return CreateInputNumber(name, minNullableNum, maxNullableNum, onChange);
        }

        public LingoGfxInputNumber<TValue> CreateInputNumber<TValue>(string name, NullableNum<TValue> min, NullableNum<TValue> max, Action<TValue>? onChange = null)
            where TValue : System.Numerics.INumber<TValue>
        {
            throw new NotImplementedException();
            var input = new LingoGfxInputNumber<TValue>();
            //var impl = new SdlGfxInputNumber<float>(_rootContext.Renderer);
            //input.Init(impl);
            //input.Name = name;
            //if (min.HasValue) input.Min = min.Value;
            //if (max.HasValue) input.Max = max.Value;
            return input;
        }

        public LingoGfxSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var spin = new LingoGfxSpinBox();
            var impl = new SdlGfxSpinBox(this);
            spin.Init(impl);
            spin.Name = name;
            if (min.HasValue) spin.Min = min.Value;
            if (max.HasValue) spin.Max = max.Value;
            if (onChange != null)
                spin.ValueChanged += () => onChange(spin.Value);
            return spin;
        }

        public LingoGfxInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
        {
            var input = new LingoGfxInputCheckbox();
            var impl = new SdlGfxInputCheckbox(this);
            input.Init(impl);
            input.Name = name;
            input.ValueChanged += () => onChange?.Invoke(input.Checked);
            return input;
        }

        public LingoGfxInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
        {
            var input = new LingoGfxInputCombobox();
            var impl = new SdlGfxInputCombobox(this);
            input.Init(impl);
            input.Name = name;
            input.ValueChanged += () => onChange?.Invoke(input.SelectedKey);
            return input;
        }

        public LingoGfxItemList CreateItemList(string name, Action<string?>? onChange = null)
        {
            var list = new LingoGfxItemList();
            var impl = new SdlGfxItemList(this);
            list.Init(impl);
            list.Name = name;
            if (onChange != null)
                list.ValueChanged += () => onChange(list.SelectedKey);
            return list;
        }

        public LingoGfxColorPicker CreateColorPicker(string name, Action<LingoColor>? onChange = null)
        {
            var picker = new LingoGfxColorPicker();
            var impl = new SdlGfxColorPicker(this);
            picker.Init(impl);
            picker.Name = name;
            if (onChange != null)
                picker.ValueChanged += () => onChange(picker.Color);
            return picker;
        }

        public LingoGfxLabel CreateLabel(string name, string text = "")
        {
            var label = new LingoGfxLabel();
            var impl = new SdlGfxLabel(this);
            label.Init(impl);
            label.Name = name;
            label.Text = text;
            return label;
        }

        public LingoGfxButton CreateButton(string name, string text = "")
        {
            var button = new LingoGfxButton();
            var impl = new SdlGfxButton(this);
            button.Init(impl);
            button.Name = name;
            button.Text = text;
            return button;
        }

        public LingoGfxStateButton CreateStateButton(string name, ILingoImageTexture? texture = null, string text = "", Action<bool>? onChange = null)
        {
            var button = new LingoGfxStateButton();
            var impl = new SdlGfxStateButton(this);
            if (onChange != null)
                button.ValueChanged += () => onChange(button.IsOn);
            button.Init(impl);
            button.Name = name;
            button.Text = text;
            if (texture != null)
                button.TextureOn = texture;
            return button;
        }

        public LingoGfxMenu CreateMenu(string name)
        {
            var menu = new LingoGfxMenu();
            var impl = new SdlGfxMenu(this, name);
            menu.Init(impl);
            return menu;
        }


        public LingoGfxWindow CreateWindow(string name, string title = "")
        {
            
            var win = new LingoGfxWindow();
            var impl = new SdlGfxWindow(win, this);
            win.Name = name;
            win.Title = title;
            return win;
        }


        public LingoGfxMenuItem CreateMenuItem(string name, string? shortcut = null)
        {
            var item = new LingoGfxMenuItem();
            var impl = new SdlGfxMenuItem(this, name, shortcut);
            item.Init(impl);
            return item;
        }

        public LingoGfxMenu CreateContextMenu(object window)
        {
            var menu = CreateMenu("ContextMenu");
            return menu;
        }

        public LingoGfxHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
        {
            throw new NotImplementedException();
        }

        public LingoGfxVerticalLineSeparator CreateVerticalLineSeparator(string name)
        {
            throw new NotImplementedException();
        }

      
    }
}
