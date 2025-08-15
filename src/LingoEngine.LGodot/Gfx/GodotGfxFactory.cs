using System;
using LingoEngine.Bitmaps;
using LingoEngine.Styles;
using LingoEngine.LGodot.Styles;
using LingoEngine.Inputs;
using AbstUI.Primitives;
using AbstUI.Components;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Factory responsible for creating Godot specific GFX components.
    /// </summary>
    public class GodotGfxFactory : IAbstUIGfxFactory
    {
        private readonly ILingoFontManager _fontManager;
        private readonly ILingoGodotStyleManager _styleManager;
        private readonly LingoGodotRootNode _rootNode;

        public GodotGfxFactory(ILingoFontManager fontManager, ILingoGodotStyleManager styleManager, LingoGodotRootNode rootNode)
        {
            _fontManager = fontManager;
            _styleManager = styleManager;
            _rootNode = rootNode;
        }

        public AbstUIGfxCanvas CreateGfxCanvas(string name, int width, int height)
        {
            var canvas = new AbstUIGfxCanvas();
            var impl = new LingoGodotGfxCanvas(canvas, _fontManager, width, height);
            canvas.Width = width;
            canvas.Height = height;
            canvas.Name = name;
            return canvas;
        }

        public AbstUIGfxWrapPanel CreateWrapPanel(AOrientation orientation, string name)
        {
            var panel = new AbstUIGfxWrapPanel(this);
            var impl = new LingoGodotWrapPanel(panel, orientation);
            panel.Name = name;
            panel.Orientation = orientation;
            return panel;
        }

        public AbstUIGfxPanel CreatePanel(string name)
        {
            var panel = new AbstUIGfxPanel(this);
            var impl = new LingoGodotPanel(panel);
            panel.Name = name;
            return panel;
        }

        public AbstUIGfxLayoutWrapper CreateLayoutWrapper(IAbstUIGfxNode content, float? x, float? y)
        {
            if (content is IAbstUIGfxLayoutNode)
                throw new InvalidOperationException($"Content {content.Name} already supports layout — wrapping is unnecessary.");
            var panel = new AbstUIGfxLayoutWrapper(content);
            var impl = new LingoGodotLayoutWrapper(panel);
            if (x != null) panel.X = x.Value;
            if (y != null) panel.Y = y.Value;
            return panel;
        }

        public AbstUIGfxTabContainer CreateTabContainer(string name)
        {
            var tab = new AbstUIGfxTabContainer();
            var impl = new LingoGodotTabContainer(tab, _styleManager);
            tab.Name = name;
            return tab;
        }

        public AbstUIGfxTabItem CreateTabItem(string name, string title)
        {
            var tab = new AbstUIGfxTabItem();
            var impl = new LingoGodotTabItem(tab);
            tab.Title = title;
            tab.Name = name;
            return tab;
        }

        public AbstUIGfxScrollContainer CreateScrollContainer(string name)
        {
            var scroll = new AbstUIGfxScrollContainer();
            var impl = new LingoGodotScrollContainer(scroll);
            scroll.Name = name;
            return scroll;
        }

        public AbstUIGfxInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
        {
            var minNum = min.HasValue ? new NullableNum<float>(min.Value) : new NullableNum<float>();
            var maxNum = max.HasValue ? new NullableNum<float>(max.Value) : new NullableNum<float>();
            var stepNum = step.HasValue ? new NullableNum<float>(step.Value) : new NullableNum<float>();
            return CreateInputSlider(name, orientation, minNum, maxNum, stepNum, onChange);
        }

        public AbstUIGfxInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
        {
            var minNum = min.HasValue ? new NullableNum<int>(min.Value) : new NullableNum<int>();
            var maxNum = max.HasValue ? new NullableNum<int>(max.Value) : new NullableNum<int>();
            var stepNum = step.HasValue ? new NullableNum<int>(step.Value) : new NullableNum<int>();
            return CreateInputSlider(name, orientation, minNum, maxNum, stepNum, onChange);
        }

        public AbstUIGfxInputSlider<TValue> CreateInputSlider<TValue>(string name, AOrientation orientation, NullableNum<TValue> min, NullableNum<TValue> max, NullableNum<TValue> step, Action<TValue>? onChange = null)
            where TValue : struct, System.Numerics.INumber<TValue>, IConvertible
        {
            var slider = new AbstUIGfxInputSlider<TValue>();
            var impl = new LingoGodotInputSlider<TValue>(slider, orientation, onChange);
            if (min.HasValue) slider.MinValue = min.Value!;
            if (max.HasValue) slider.MaxValue = max.Value!;
            if (step.HasValue) slider.Step = step.Value!;
            slider.Name = name;
            return slider;
        }

        public AbstUIGfxInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null)
        {
            var input = new AbstUIGfxInputText();
            var impl = new LingoGodotInputText(input, _fontManager, onChange);
            input.MaxLength = maxLength;
            input.Name = name;
            return input;
        }

        public AbstUIGfxInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var minNullableNum = min.HasValue ? new NullableNum<float>(min.Value) : new NullableNum<float>();
            var maxNullableNum = max.HasValue ? new NullableNum<float>(max.Value) : new NullableNum<float>();
            return CreateInputNumber(name, minNullableNum, maxNullableNum, onChange);
        }

        public AbstUIGfxInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
        {
            var minNullableNum = min.HasValue ? new NullableNum<int>(min.Value) : new NullableNum<int>();
            var maxNullableNum = max.HasValue ? new NullableNum<int>(max.Value) : new NullableNum<int>();
            return CreateInputNumber(name, minNullableNum, maxNullableNum, onChange);
        }

        public AbstUIGfxInputNumber<TValue> CreateInputNumber<TValue>(string name, NullableNum<TValue> min, NullableNum<TValue> max, Action<TValue>? onChange = null)
             where TValue : System.Numerics.INumber<TValue>
        {
            var input = new AbstUIGfxInputNumber<TValue>();
            var impl = new LingoGodotInputNumber<TValue>(input, _fontManager, onChange);
            if (min.HasValue) input.Min = min.Value!;
            if (max.HasValue) input.Max = max.Value!;
            input.Name = name;
            return input;
        }

        public AbstUIGfxSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var spin = new AbstUIGfxSpinBox();
            var impl = new LingoGodotSpinBox(spin, _fontManager, onChange);
            spin.Name = name;
            if (min.HasValue) spin.Min = min.Value;
            if (max.HasValue) spin.Max = max.Value;
            return spin;
        }

        public AbstUIGfxInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
        {
            var input = new AbstUIGfxInputCheckbox();
            var impl = new LingoGodotInputCheckbox(input, onChange);
            input.Name = name;
            return input;
        }

        public AbstUIGfxInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
        {
            var input = new AbstUIGfxInputCombobox();
            var impl = new LingoGodotInputCombobox(input, _fontManager, onChange);
            input.Name = name;
            return input;
        }

        public AbstUIGfxItemList CreateItemList(string name, Action<string?>? onChange = null)
        {
            var list = new AbstUIGfxItemList();
            var impl = new LingoGodotItemList(list, onChange);
            list.Name = name;
            return list;
        }

        public AbstUIGfxColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null)
        {
            var picker = new AbstUIGfxColorPicker();
            var impl = new LingoGodotColorPicker(picker, onChange);
            picker.Name = name;
            return picker;
        }

        public AbstUIGfxLabel CreateLabel(string name, string text = "")
        {
            var label = new AbstUIGfxLabel();
            var impl = new LingoGodotLabel(label, _fontManager);
            label.Text = text;
            label.Name = name;
            return label;
        }

        public AbstUIGfxButton CreateButton(string name, string text = "")
        {
            var button = new AbstUIGfxButton();
            var impl = new LingoGodotButton(button, _fontManager);
            button.Name = name;
            if (!string.IsNullOrWhiteSpace(text))
                button.Text = text;
            return button;
        }

        public AbstUIGfxStateButton CreateStateButton(string name, IAbstUITexture2D? texture = null, string text = "", Action<bool>? onChange = null)
        {
            var button = new AbstUIGfxStateButton();
            var impl = new LingoGodotStateButton(button, onChange);
            button.Name = name;
            if (!string.IsNullOrWhiteSpace(text))
                button.Text = text;
            if (texture != null)
                button.TextureOn = texture;
            return button;
        }

        public AbstUIGfxMenu CreateMenu(string name)
        {
            var menu = new AbstUIGfxMenu();
            var impl = new LingoGodotMenu(menu, name);
            return menu;
        }

        public AbstUIGfxMenuItem CreateMenuItem(string name, string? shortcut = null)
        {
            var item = new AbstUIGfxMenuItem();
            var impl = new LingoGodotMenuItem(item, name, shortcut);
            return item;
        }

        public AbstUIGfxMenu CreateContextMenu(object window)
        {
            var menu = CreateMenu("ContextMenu");
            if (window is Godot.Node node)
                node.AddChild(menu.Framework<LingoGodotMenu>());
            return menu;
        }

        public AbstUIGfxHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
        {
            var sep = new AbstUIGfxHorizontalLineSeparator();
            var impl = new LingoGodotHorizontalLineSeparator(sep);
            sep.Name = name;
            return sep;
        }

        public AbstUIGfxVerticalLineSeparator CreateVerticalLineSeparator(string name)
        {
            var sep = new AbstUIGfxVerticalLineSeparator();
            var impl = new LingoGodotVerticalLineSeparator(sep);
            sep.Name = name;
            return sep;
        }

        public AbstUIGfxWindow CreateWindow(string name, string title= "")
        {
            var win = new AbstUIGfxWindow();
            var impl = new LingoGodotWindow(win, _styleManager, _rootNode);
            win.Name = name;
            if (!string.IsNullOrWhiteSpace(title))
                win.Title = title;
            return win;
        }
    }
}
