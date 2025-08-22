using UnityEngine;
using System;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Windowing;
using AbstUI.Components.Graphics;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Buttons;
using AbstUI.Components.Texts;
using AbstUI.LUnity.Components.Containers;
using AbstUI.LUnity.Components.Inputs;
using AbstUI.LUnity.Components.Texts;
using AbstUI.LUnity.Components.Buttons;
using AbstUI.LUnity.Components.Graphics;

using AbstUI.LUnity.Components.Menus;
namespace AbstUI.LUnity.Components;

/// <summary>
/// Factory creating Unity-based AbstUI components.
/// </summary>
public class AbstUnityComponentFactory : AbstComponentFactoryBase, IAbstComponentFactory
{

    public AbstUnityComponentFactory(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height)
    {
        var canvas = new AbstGfxCanvas();
        var impl = new AbstUnityGfxCanvas(width, height);
        canvas.Init(impl);
        InitComponent(canvas);
        canvas.Name = name;
        canvas.Width = width;
        canvas.Height = height;
        return canvas;
    }

    public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name)
    {
        var panel = new AbstWrapPanel(this);
        var impl = new AbstUnityWrapPanel(orientation);
        panel.Init(impl);
        InitComponent(panel);
        panel.Name = name;
        panel.Orientation = orientation;
        return panel;
    }

    public AbstPanel CreatePanel(string name)
    {
        var panel = new AbstPanel(this);
        var impl = new AbstUnityPanel();
        panel.Init(impl);
        InitComponent(panel);
        panel.Name = name;
        return panel;
    }

    public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y)
    {
        var wrapper = new AbstLayoutWrapper(content);
        var impl = new AbstUnityLayoutWrapper(wrapper);
        wrapper.Init(impl);
        InitComponent(wrapper);
        if (x.HasValue) wrapper.X = x.Value;
        if (y.HasValue) wrapper.Y = y.Value;
        return wrapper;
    }

    public AbstTabContainer CreateTabContainer(string name)
    {
        var container = new AbstTabContainer();
        var impl = new AbstUnityTabContainer();
        container.Init(impl);
        InitComponent(container);
        container.Name = name;
        return container;
    }

    public AbstTabItem CreateTabItem(string name, string title)
    {
        var tab = new AbstTabItem();
        var impl = new AbstUnityTabItem();
        tab.Init(impl);
        InitComponent(tab);
        tab.Name = name;
        tab.Title = title;
        return tab;
    }

    public AbstScrollContainer CreateScrollContainer(string name)
    {
        var scroll = new AbstScrollContainer();
        var impl = new AbstUnityScrollContainer();
        scroll.Init(impl);
        InitComponent(scroll);
        scroll.Name = name;
        return scroll;
    }

    public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
    {
        var slider = new AbstInputSlider<float>();
        var impl = new AbstUnityInputSlider<float>(orientation);
        slider.Init(impl);
        InitComponent(slider);
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
        var impl = new AbstUnityInputSlider<int>(orientation);
        slider.Init(impl);
        InitComponent(slider);
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
        var impl = new AbstUnityInputText
        {
            MaxLength = maxLength,
            IsMultiLine = multiLine
        };
        if (onChange != null)
            impl.ValueChanged += () => onChange(impl.Text);
        input.Init(impl);
        InitComponent(input);
        input.Name = name;
        return input;
    }

    public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
    {
        var input = new AbstInputNumber<float>();
        var impl = new AbstUnityInputNumber<float>();
        input.Init(impl);
        InitComponent(input);
        input.Name = name;
        input.NumberType = ANumberType.Float;
        if (min.HasValue) input.Min = min.Value;
        if (max.HasValue) input.Max = max.Value;
        if (onChange != null)
            input.ValueChanged += () => onChange(input.Value);
        return input;
    }

    public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
    {
        var input = new AbstInputNumber<int>();
        var impl = new AbstUnityInputNumber<int>();
        input.Init(impl);
        InitComponent(input);
        input.Name = name;
        input.NumberType = ANumberType.Integer;
        if (min.HasValue) input.Min = min.Value;
        if (max.HasValue) input.Max = max.Value;
        if (onChange != null)
            input.ValueChanged += () => onChange(input.Value);
        return input;
    }

    public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
    {
        var spin = new AbstInputSpinBox();
        var impl = new AbstUnityInputSpinBox();
        if (onChange != null)
            impl.ValueChanged += () => onChange(spin.Value);
        spin.Init(impl);
        InitComponent(spin);
        spin.Name = name;
        if (min.HasValue) spin.Min = min.Value;
        if (max.HasValue) spin.Max = max.Value;
        return spin;
    }

    public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
    {
        var input = new AbstInputCheckbox();
        var impl = new AbstUnityInputCheckbox();
        if (onChange != null)
            impl.ValueChanged += () => onChange(impl.Checked);
        input.Init(impl);
        InitComponent(input);
        input.Name = name;
        return input;
    }

    public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
    {
        var input = new AbstInputCombobox();
        var impl = new AbstUnityInputCombobox();
        if (onChange != null)
            impl.ValueChanged += () => onChange(impl.SelectedValue);
        input.Init(impl);
        InitComponent(input);
        input.Name = name;
        return input;
    }

    public AbstItemList CreateItemList(string name, Action<string?>? onChange = null)
    {
        var list = new AbstItemList();
        var impl = new AbstUnityItemList();
        if (onChange != null)
            impl.ValueChanged += () => onChange(impl.SelectedValue);
        list.Init(impl);
        InitComponent(list);
        list.Name = name;
        return list;
    }

    public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null)
    {
        var picker = new AbstColorPicker();
        var impl = new AbstUnityColorPicker();
        if (onChange != null)
            impl.ValueChanged += () => onChange(impl.Color);
        picker.Init(impl);
        InitComponent(picker);
        picker.Name = name;
        return picker;
    }

    public AbstLabel CreateLabel(string name, string text = "")
    {
        var label = new AbstLabel();
        var impl = new AbstUnityLabel();
        label.Init(impl);
        InitComponent(label);
        label.Name = name;
        label.Text = text;
        return label;
    }

    public AbstButton CreateButton(string name, string text = "")
    {
        var button = new AbstButton();
        var impl = new AbstUnityButton();
        button.Init(impl);
        InitComponent(button);
        button.Name = name;
        button.Text = text;
        return button;
    }

    public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null)
    {
        var button = new AbstStateButton();
        var impl = new AbstUnityStateButton();
        if (onChange != null)
            impl.ValueChanged += () => onChange(impl.IsOn);
        button.Init(impl);
        InitComponent(button);
        button.Name = name;
        button.Text = text;
        if (texture != null)
            button.TextureOn = texture;
        return button;
    }

    public AbstMenu CreateMenu(string name)
    {
        var menu = new AbstMenu();
        var impl = new AbstUnityMenu(name);
        menu.Init(impl);
        InitComponent(menu);
        menu.Name = name;
        return menu;
    }

    public AbstMenuItem CreateMenuItem(string name, string? shortcut = null)
    {
        var item = new AbstMenuItem();
        var impl = new AbstUnityMenuItem(name, shortcut);
        item.Init(impl);
        return item;
    }

    public AbstMenu CreateContextMenu(object window)
    {
        var menu = CreateMenu("ContextMenu");
        if (window is GameObject go)
            ((GameObject)menu.Framework<AbstUnityMenu>().FrameworkNode).transform.SetParent(go.transform);
        return menu;
    }

    public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
    {
        var sep = new AbstHorizontalLineSeparator();
        var impl = new AbstUnityHorizontalLineSeparator();
        sep.Init(impl);
        InitComponent(sep);
        sep.Name = name;
        return sep;
    }

    public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name)
    {
        var sep = new AbstVerticalLineSeparator();
        var impl = new AbstUnityVerticalLineSeparator();
        sep.Init(impl);
        InitComponent(sep);
        sep.Name = name;
        return sep;
    }

}
