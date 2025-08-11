using LingoEngine.Bitmaps;
using LingoEngine.Director.Core.Styles;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Tools;
using System.Linq.Expressions;

namespace LingoEngine.Director.Core.UI
{
    public class GfxWrapPanelBuilder
    {
        private LingoGfxWrapPanel _panel;
        private LingoGfxWrapPanel _rootPanel;
        private ILingoFrameworkFactory _factory;
        private readonly GfxWrapPanelBuilder? _parent;

        public GfxWrapPanelBuilder(LingoGfxWrapPanel panel, GfxWrapPanelBuilder? parent)
        {
            _rootPanel = panel;
            _panel = panel;
            _factory = panel.Factory;
            _parent = parent;
        }
        public LingoGfxWrapPanel Finalize()
        {
            return _parent != null? _parent.Finalize() : _rootPanel;
        }

        public GfxWrapPanelBuilder NewLine(string name)
        {
            if(_parent != null)
                return _parent.NewLine(name);
            _panel = _factory.CreateWrapPanel(_rootPanel.Orientation == LingoOrientation.Vertical? LingoOrientation.Horizontal : LingoOrientation.Vertical, name);
            _panel.Width = _rootPanel.Width;
            _rootPanel.AddItem(_panel);
            return _panel.Compose(this);
        }
        public GfxWrapPanelBuilder Configure(Action<LingoGfxWrapPanel> configure)
        {
            configure(_panel);
            return this;
        }


        public GfxWrapPanelBuilder AddLabel(string name, string text, int fontSize = 11, int? width = null, Action<LingoGfxLabel>? configure = null)
        {
            LingoGfxLabel label = _factory.CreateLabel(name, text);
            label.FontSize = fontSize;
            label.FontColor = DirectorColors.TextColorLabels;
            if (width.HasValue)
                label.Width = width.Value;
            _panel.AddItem(label);
            if (configure != null)
                configure(label);
            return this;
        }

        public GfxWrapPanelBuilder AddTextInput<T>(string name, T target, Expression<Func<T, string?>> property, int width = 100, Action<LingoGfxInputText>? configure = null)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var input = _factory.CreateInputText(name, 0, value => setter(target, value));
            input.Text = getter(target) ?? string.Empty;
            input.Width = width;
            _panel.AddItem(input);
            if (configure != null)
                configure(input);
            return this;
        }

        public GfxWrapPanelBuilder AddNumericInputFloat<T>(string name, T target, Expression<Func<T, float>> property, int width = 40, float? min = null, float? max = null)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var input = _factory.CreateInputNumberFloat(name, min, max, value => setter(target, value));
            input.Value = getter(target);
            input.Width = width;
            _panel.AddItem(input);
            return this;
        } 
        public GfxWrapPanelBuilder AddNumericInputInt<T>(string name, T target, Expression<Func<T, int>> property, int width = 40, int? min = null, int? max = null)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var input = _factory.CreateInputNumberInt(name, min, max, value => setter(target, value));
            input.Value = getter(target);
            input.Width = width;
            _panel.AddItem(input);
            return this;
        }

        public GfxWrapPanelBuilder AddCheckbox<T>(string name, T target, Expression<Func<T, bool>> property)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var checkbox = _factory.CreateInputCheckbox(name, value => setter(target, value));
            checkbox.Checked = getter(target);
            _panel.AddItem(checkbox);
            return this;
        }

        public GfxWrapPanelBuilder AddColorPicker<T>(string name, T target, Expression<Func<T, LingoColor>> property, int width = 20)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var colorPicker = _factory.CreateColorPicker(name, value => setter(target, value));
            colorPicker.Color = getter(target);
            colorPicker.Width = width;
            _panel.AddItem(colorPicker);
            return this;
        }

        public GfxWrapPanelBuilder AddItemList(string name, IEnumerable<KeyValuePair<string,string>> items, int width = 100, string? selectedKey = null, Action<string?>? onChange = null)
        {
            var list = _factory.CreateItemList(name, onChange);
            foreach (var item in items)
                list.AddItem(item.Key, item.Value);
            if (selectedKey != null)
                list.SelectedKey = selectedKey;
            list.Width = width;
            _panel.AddItem(list);
            return this;
        } 
        public GfxWrapPanelBuilder AddCombobox(string name, IEnumerable<KeyValuePair<string,string>> items, int width = 100, string? selectedKey = null, Action<string?>? onChange = null)
        {
            var list = _factory.CreateInputCombobox(name, onChange);
            foreach (var item in items)
                list.AddItem(item.Key, item.Value);
            if (selectedKey != null)
                list.SelectedKey = selectedKey;
            list.Width = width;
            _panel.AddItem(list);
            return this;
        }

        public GfxWrapPanelBuilder AddStateButton<T>(string name, T target, ILingoImageTexture texture, Expression<Func<T, bool>> property, string text = "", Action<LingoGfxStateButton>? configure = null)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();
            LingoGfxStateButton button = _factory.CreateStateButton(name, texture, text, value => setter(target, value));
            button.IsOn = getter(target);
            _panel.AddItem(button);
            configure?.Invoke(button);
            return this;
        } 
        public GfxWrapPanelBuilder AddButton(string name, string text, Action click, Action<LingoGfxButton>? configure = null)
        {
            var button = _factory.CreateButton(name, text);
            _panel.AddItem(button);
            button.Pressed += () => click();
            configure?.Invoke(button);
            return this;
        }

        public GfxWrapPanelBuilder AddSliderFloat(string name, LingoOrientation orientation, Action<float>? onChange = null, float? min = null, float? max = null, float? step = null, Action<LingoGfxInputSlider<float>>? configure = null)
        {
            var slider = _factory.CreateInputSliderFloat(orientation, name, min, max, step, onChange);
            _panel.AddItem(slider);
            configure?.Invoke(slider);
            return this;
        }

        public GfxWrapPanelBuilder AddSliderInt(string name, LingoOrientation orientation, Action<int>? onChange = null, int? min = null, int? max = null, int? step = null, Action<LingoGfxInputSlider<int>>? configure = null)
        {
            var slider = _factory.CreateInputSliderInt(orientation, name, min, max, step, onChange);
            _panel.AddItem(slider);
            configure?.Invoke(slider);
            return this;
        }
        public GfxWrapPanelBuilder AddVLine(string name, float height = 0, float paddingTop = 0)
        {
            _panel.AddVLine(name, height, paddingTop);
            return this;
        }
        public GfxWrapPanelBuilder AddHLine(string name, float width = 0, float paddingLeft = 0)
        {
            _panel.AddHLine(name, width, paddingLeft);
            return this;
        } public GfxWrapPanelBuilder AddItem(ILingoGfxNode element)
        {
            _panel.AddItem(element);
            return this;
        }



    }
}
