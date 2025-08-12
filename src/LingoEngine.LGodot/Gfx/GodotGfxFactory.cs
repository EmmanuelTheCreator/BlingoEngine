using System;
using LingoEngine.Bitmaps;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Styles;
using LingoEngine.LGodot.Styles;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Factory responsible for creating Godot specific GFX components.
    /// </summary>
    public class GodotGfxFactory : ILingoGfxFactory
    {
        private readonly ILingoFontManager _fontManager;
        private readonly ILingoGodotStyleManager _styleManager;

        public GodotGfxFactory(ILingoFontManager fontManager, ILingoGodotStyleManager styleManager)
        {
            _fontManager = fontManager;
            _styleManager = styleManager;
        }

        public LingoGfxCanvas CreateGfxCanvas(string name, int width, int height)
        {
            var canvas = new LingoGfxCanvas();
            var impl = new LingoGodotGfxCanvas(canvas, _fontManager, width, height);
            canvas.Width = width;
            canvas.Height = height;
            canvas.Name = name;
            return canvas;
        }

        public LingoGfxWrapPanel CreateWrapPanel(LingoOrientation orientation, string name)
        {
            var panel = new LingoGfxWrapPanel(this);
            var impl = new LingoGodotWrapPanel(panel, orientation);
            panel.Name = name;
            panel.Orientation = orientation;
            return panel;
        }

        public LingoGfxPanel CreatePanel(string name)
        {
            var panel = new LingoGfxPanel(this);
            var impl = new LingoGodotPanel(panel);
            panel.Name = name;
            return panel;
        }

        public LingoGfxLayoutWrapper CreateLayoutWrapper(ILingoGfxNode content, float? x, float? y)
        {
            if (content is ILingoGfxLayoutNode)
                throw new InvalidOperationException($"Content {content.Name} already supports layout — wrapping is unnecessary.");
            var panel = new LingoGfxLayoutWrapper(content);
            var impl = new LingoGodotLayoutWrapper(panel);
            if (x != null) panel.X = x.Value;
            if (y != null) panel.Y = y.Value;
            return panel;
        }

        public LingoGfxTabContainer CreateTabContainer(string name)
        {
            var tab = new LingoGfxTabContainer();
            var impl = new LingoGodotTabContainer(tab, _styleManager);
            tab.Name = name;
            return tab;
        }

        public LingoGfxTabItem CreateTabItem(string name, string title)
        {
            var tab = new LingoGfxTabItem();
            var impl = new LingoGodotTabItem(tab);
            tab.Title = title;
            tab.Name = name;
            return tab;
        }

        public LingoGfxScrollContainer CreateScrollContainer(string name)
        {
            var scroll = new LingoGfxScrollContainer();
            var impl = new LingoGodotScrollContainer(scroll);
            scroll.Name = name;
            return scroll;
        }

        public LingoGfxInputSlider<float> CreateInputSliderFloat(LingoOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
        {
            var minNum = min.HasValue ? new NullableNum<float>(min.Value) : new NullableNum<float>();
            var maxNum = max.HasValue ? new NullableNum<float>(max.Value) : new NullableNum<float>();
            var stepNum = step.HasValue ? new NullableNum<float>(step.Value) : new NullableNum<float>();
            return CreateInputSlider(name, orientation, minNum, maxNum, stepNum, onChange);
        }

        public LingoGfxInputSlider<int> CreateInputSliderInt(LingoOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
        {
            var minNum = min.HasValue ? new NullableNum<int>(min.Value) : new NullableNum<int>();
            var maxNum = max.HasValue ? new NullableNum<int>(max.Value) : new NullableNum<int>();
            var stepNum = step.HasValue ? new NullableNum<int>(step.Value) : new NullableNum<int>();
            return CreateInputSlider(name, orientation, minNum, maxNum, stepNum, onChange);
        }

        public LingoGfxInputSlider<TValue> CreateInputSlider<TValue>(string name, LingoOrientation orientation, NullableNum<TValue> min, NullableNum<TValue> max, NullableNum<TValue> step, Action<TValue>? onChange = null)
            where TValue : struct, System.Numerics.INumber<TValue>, IConvertible
        {
            var slider = new LingoGfxInputSlider<TValue>();
            var impl = new LingoGodotInputSlider<TValue>(slider, orientation, onChange);
            if (min.HasValue) slider.MinValue = min.Value!;
            if (max.HasValue) slider.MaxValue = max.Value!;
            if (step.HasValue) slider.Step = step.Value!;
            slider.Name = name;
            return slider;
        }

        public LingoGfxInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null)
        {
            var input = new LingoGfxInputText();
            var impl = new LingoGodotInputText(input, _fontManager, onChange);
            input.MaxLength = maxLength;
            input.Name = name;
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
            var input = new LingoGfxInputNumber<TValue>();
            var impl = new LingoGodotInputNumber<TValue>(input, _fontManager, onChange);
            if (min.HasValue) input.Min = min.Value!;
            if (max.HasValue) input.Max = max.Value!;
            input.Name = name;
            return input;
        }

        public LingoGfxSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var spin = new LingoGfxSpinBox();
            var impl = new LingoGodotSpinBox(spin, _fontManager, onChange);
            spin.Name = name;
            if (min.HasValue) spin.Min = min.Value;
            if (max.HasValue) spin.Max = max.Value;
            return spin;
        }

        public LingoGfxInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
        {
            var input = new LingoGfxInputCheckbox();
            var impl = new LingoGodotInputCheckbox(input, onChange);
            input.Name = name;
            return input;
        }

        public LingoGfxInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
        {
            var input = new LingoGfxInputCombobox();
            var impl = new LingoGodotInputCombobox(input, _fontManager, onChange);
            input.Name = name;
            return input;
        }

        public LingoGfxItemList CreateItemList(string name, Action<string?>? onChange = null)
        {
            var list = new LingoGfxItemList();
            var impl = new LingoGodotItemList(list, onChange);
            list.Name = name;
            return list;
        }

        public LingoGfxColorPicker CreateColorPicker(string name, Action<LingoColor>? onChange = null)
        {
            var picker = new LingoGfxColorPicker();
            var impl = new LingoGodotColorPicker(picker, onChange);
            picker.Name = name;
            return picker;
        }

        public LingoGfxLabel CreateLabel(string name, string text = "")
        {
            var label = new LingoGfxLabel();
            var impl = new LingoGodotLabel(label, _fontManager);
            label.Text = text;
            label.Name = name;
            return label;
        }

        public LingoGfxButton CreateButton(string name, string text = "")
        {
            var button = new LingoGfxButton();
            var impl = new LingoGodotButton(button, _fontManager);
            button.Name = name;
            if (!string.IsNullOrWhiteSpace(text))
                button.Text = text;
            return button;
        }

        public LingoGfxStateButton CreateStateButton(string name, ILingoImageTexture? texture = null, string text = "", Action<bool>? onChange = null)
        {
            var button = new LingoGfxStateButton();
            var impl = new LingoGodotStateButton(button, onChange);
            button.Name = name;
            if (!string.IsNullOrWhiteSpace(text))
                button.Text = text;
            if (texture != null)
                button.TextureOn = texture;
            return button;
        }

        public LingoGfxMenu CreateMenu(string name)
        {
            var menu = new LingoGfxMenu();
            var impl = new LingoGodotMenu(menu, name);
            return menu;
        }

        public LingoGfxMenuItem CreateMenuItem(string name, string? shortcut = null)
        {
            var item = new LingoGfxMenuItem();
            var impl = new LingoGodotMenuItem(item, name, shortcut);
            return item;
        }

        public LingoGfxMenu CreateContextMenu(object window)
        {
            var menu = CreateMenu("ContextMenu");
            if (window is Godot.Node node)
                node.AddChild(menu.Framework<LingoGodotMenu>());
            return menu;
        }

        public LingoGfxHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
        {
            var sep = new LingoGfxHorizontalLineSeparator();
            var impl = new LingoGodotHorizontalLineSeparator(sep);
            sep.Name = name;
            return sep;
        }

        public LingoGfxVerticalLineSeparator CreateVerticalLineSeparator(string name)
        {
            var sep = new LingoGfxVerticalLineSeparator();
            var impl = new LingoGodotVerticalLineSeparator(sep);
            sep.Name = name;
            return sep;
        }

        public LingoGfxWindow CreateWindow(string name, string title = "")
        {
            var win = new LingoGfxWindow();
            var impl = new LingoGodotWindow(win, _styleManager);
            win.Name = name;
            if (!string.IsNullOrWhiteSpace(title))
                win.Title = title;
            return win;
        }
    }
}
