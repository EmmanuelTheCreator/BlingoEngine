using System;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using Microsoft.Extensions.DependencyInjection;
using AbstUI.LGodot;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Styles;

namespace AbstUI.GfxVisualTest.LGodot;

/// <summary>
/// Minimal Godot implementation of <see cref="IAbstComponentFactory"/> used for visual tests.
/// </summary>
public class AbstGodotComponentFactory : IAbstComponentFactory
{
    private readonly IAbstFontManager _fontManager;
    private readonly IAbstGodotStyleManager _styleManager;

    public AbstGodotComponentFactory()
    {
        var services = new ServiceCollection();
        services.WithAbstUIGodot();
        var provider = services.BuildServiceProvider();
        _fontManager = provider.GetRequiredService<IAbstFontManager>();
        _styleManager = provider.GetRequiredService<IAbstGodotStyleManager>();
    }

    public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height)
    {
        var canvas = new AbstGfxCanvas();
        var impl = new AbstGodotGfxCanvas(canvas, _fontManager, width, height);
        canvas.Init(impl);
        canvas.Name = name;
        canvas.Width = width;
        canvas.Height = height;
        return canvas;
    }

    public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name)
    {
        var panel = new AbstWrapPanel(this);
        var impl = new AbstGodotWrapPanel(panel, orientation);
        panel.Init(impl);
        panel.Name = name;
        panel.Orientation = orientation;
        return panel;
    }

    public AbstPanel CreatePanel(string name)
    {
        var panel = new AbstPanel(this);
        var impl = new AbstGodotPanel(panel);
        panel.Init(impl);
        panel.Name = name;
        return panel;
    }

    public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y)
    {
        var wrapper = new AbstLayoutWrapper(content);
        var impl = new AbstGodotLayoutWrapper(wrapper);
        wrapper.Init(impl);
        if (x.HasValue) wrapper.X = x.Value;
        if (y.HasValue) wrapper.Y = y.Value;
        return wrapper;
    }

    public AbstTabContainer CreateTabContainer(string name)
    {
        var tab = new AbstTabContainer();
        var impl = new AbstGodotTabContainer(tab, _styleManager);
        tab.Init(impl);
        tab.Name = name;
        return tab;
    }

    public AbstTabItem CreateTabItem(string name, string title)
    {
        var tab = new AbstTabItem();
        var impl = new AbstGodotTabItem(tab);
        tab.Init(impl);
        tab.Name = name;
        tab.Title = title;
        return tab;
    }

    public AbstScrollContainer CreateScrollContainer(string name)
    {
        var scroll = new AbstScrollContainer();
        var impl = new AbstGodotScrollContainer(scroll);
        scroll.Init(impl);
        scroll.Name = name;
        return scroll;
    }

    public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
    {
        var slider = new AbstInputSlider<float>();
        var impl = new AbstGodotInputSlider<float>(slider, orientation, v => onChange?.Invoke(v));
        slider.Init(impl);
        slider.Name = name;
        if (min.HasValue) slider.MinValue = min.Value;
        if (max.HasValue) slider.MaxValue = max.Value;
        if (step.HasValue) slider.Step = step.Value;
        if (onChange != null) slider.ValueChanged += () => onChange(slider.Value);
        return slider;
    }

    public AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
    {
        var slider = new AbstInputSlider<int>();
        var impl = new AbstGodotInputSlider<int>(slider, orientation, v => onChange?.Invoke(v));
        slider.Init(impl);
        slider.Name = name;
        if (min.HasValue) slider.MinValue = min.Value;
        if (max.HasValue) slider.MaxValue = max.Value;
        if (step.HasValue) slider.Step = step.Value;
        if (onChange != null) slider.ValueChanged += () => onChange(slider.Value);
        return slider;
    }

    public AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null, bool multiLine = false)
    {
        var input = new AbstInputText();
        var impl = new AbstGodotInputText(input, _fontManager, onChange, multiLine);
        input.Init(impl);
        input.Name = name;
        input.MaxLength = maxLength;
        if (onChange != null) input.ValueChanged += () => onChange(input.Text);
        return input;
    }

    public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
    {
        var input = new AbstInputNumber<float>();
        var impl = new AbstGodotInputNumber<float>(input, _fontManager, onChange);
        input.Init(impl);
        input.Name = name;
        if (min.HasValue) input.Min = min.Value;
        if (max.HasValue) input.Max = max.Value;
        if (onChange != null) input.ValueChanged += () => onChange(input.Value);
        return input;
    }

    public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
    {
        var input = new AbstInputNumber<int>();
        var impl = new AbstGodotInputNumber<int>(input, _fontManager, onChange);
        input.Init(impl);
        input.Name = name;
        if (min.HasValue) input.Min = min.Value;
        if (max.HasValue) input.Max = max.Value;
        if (onChange != null) input.ValueChanged += () => onChange(input.Value);
        return input;
    }

    public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
    {
        var spin = new AbstInputSpinBox();
        var impl = new AbstGodotSpinBox(spin, _fontManager, onChange);
        spin.Init(impl);
        spin.Name = name;
        if (min.HasValue) spin.Min = min.Value;
        if (max.HasValue) spin.Max = max.Value;
        if (onChange != null) spin.ValueChanged += () => onChange(spin.Value);
        return spin;
    }

    public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
    {
        var input = new AbstInputCheckbox();
        var impl = new AbstGodotInputCheckbox(input, onChange);
        input.Init(impl);
        input.Name = name;
        if (onChange != null) input.ValueChanged += () => onChange(input.Checked);
        return input;
    }

    public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
    {
        var input = new AbstInputCombobox();
        var impl = new AbstGodotInputCombobox(input, _fontManager, onChange);
        input.Init(impl);
        input.Name = name;
        if (onChange != null) input.ValueChanged += () => onChange(input.SelectedKey);
        return input;
    }

    public AbstItemList CreateItemList(string name, Action<string?>? onChange = null)
    {
        var list = new AbstItemList();
        var impl = new AbstGodotItemList(list, onChange);
        list.Init(impl);
        list.Name = name;
        if (onChange != null) list.ValueChanged += () => onChange(list.SelectedKey);
        return list;
    }

    public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null)
    {
        var picker = new AbstColorPicker();
        var impl = new AbstGodotColorPicker(picker, onChange);
        picker.Init(impl);
        picker.Name = name;
        if (onChange != null) picker.ValueChanged += () => onChange(picker.Color);
        return picker;
    }

    public AbstLabel CreateLabel(string name, string text = "")
    {
        var label = new AbstLabel();
        var impl = new AbstGodotLabel(label, _fontManager);
        label.Init(impl);
        label.Name = name;
        label.Text = text;
        return label;
    }

    public AbstButton CreateButton(string name, string text = "")
    {
        var button = new AbstButton();
        var impl = new AbstGodotButton(button, _fontManager);
        button.Init(impl);
        button.Name = name;
        button.Text = text;
        return button;
    }

    public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null)
    {
        var button = new AbstStateButton();
        var impl = new AbstGodotStateButton(button, onChange);
        button.Init(impl);
        button.Name = name;
        button.Text = text;
        if (texture != null) button.TextureOn = texture;
        if (onChange != null) button.ValueChanged += () => onChange(button.IsOn);
        return button;
    }

    public AbstMenu CreateMenu(string name)
    {
        var menu = new AbstMenu();
        var impl = new AbstGodotMenu(menu, name);
        menu.Init(impl);
        return menu;
    }

    public AbstMenuItem CreateMenuItem(string name, string? shortcut = null)
    {
        var item = new AbstMenuItem();
        var impl = new AbstGodotMenuItem(item, name, shortcut);
        item.Init(impl);
        return item;
    }

    public AbstMenu CreateContextMenu(object window)
    {
        var menu = new AbstMenu();
        var impl = new AbstGodotMenu(menu, "ContextMenu");
        menu.Init(impl);
        return menu;
    }

    public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
    {
        var sep = new AbstHorizontalLineSeparator();
        var impl = new AbstGodotHorizontalLineSeparator(sep);
        sep.Init(impl);
        sep.Name = name;
        return sep;
    }

    public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name)
    {
        var sep = new AbstVerticalLineSeparator();
        var impl = new AbstGodotVerticalLineSeparator(sep);
        sep.Init(impl);
        sep.Name = name;
        return sep;
    }

    public AbstWindow CreateWindow(string name, string title = "")
    {
        throw new NotImplementedException();
    }
}

