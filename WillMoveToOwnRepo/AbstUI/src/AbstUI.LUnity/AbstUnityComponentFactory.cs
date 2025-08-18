using System;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.LUnity.Components;

namespace AbstUI.LUnity;

/// <summary>
/// Factory creating Unity-based AbstUI components.
/// </summary>
public class AbstUnityComponentFactory : IAbstComponentFactory
{
    public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height) => throw new NotImplementedException();

    public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name)
    {
        var panel = new AbstWrapPanel(this);
        var impl = new AbstUnityWrapPanel(orientation);
        panel.Init(impl);
        panel.Name = name;
        panel.Orientation = orientation;
        return panel;
    }

    public AbstPanel CreatePanel(string name)
    {
        var panel = new AbstPanel(this);
        var impl = new AbstUnityPanel();
        panel.Init(impl);
        panel.Name = name;
        return panel;
    }

    public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y)
    {
        var wrapper = new AbstLayoutWrapper(content);
        var impl = new AbstUnityLayoutWrapper();
        wrapper.Init(impl);
        if (x.HasValue) wrapper.X = x.Value;
        if (y.HasValue) wrapper.Y = y.Value;
        return wrapper;
    }

    public AbstTabContainer CreateTabContainer(string name)
    {
        var container = new AbstTabContainer();
        var impl = new AbstUnityTabContainer();
        container.Init(impl);
        container.Name = name;
        return container;
    }

    public AbstTabItem CreateTabItem(string name, string title)
    {
        var tab = new AbstTabItem();
        var impl = new AbstUnityTabItem();
        tab.Init(impl);
        tab.Name = name;
        tab.Title = title;
        return tab;
    }

    public AbstScrollContainer CreateScrollContainer(string name)
    {
        var scroll = new AbstScrollContainer();
        var impl = new AbstUnityScrollContainer();
        scroll.Init(impl);
        scroll.Name = name;
        return scroll;
    }

    public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null) => throw new NotImplementedException();

    public AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null) => throw new NotImplementedException();

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
        input.Name = name;
        return input;
    }

    public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null) => throw new NotImplementedException();

    public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null) => throw new NotImplementedException();

    public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null) => throw new NotImplementedException();

    public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
    {
        var input = new AbstInputCheckbox();
        var impl = new AbstUnityInputCheckbox();
        if (onChange != null)
            impl.ValueChanged += () => onChange(impl.Checked);
        input.Init(impl);
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
        input.Name = name;
        return input;
    }

    public AbstItemList CreateItemList(string name, Action<string?>? onChange = null) => throw new NotImplementedException();

    public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null) => throw new NotImplementedException();

    public AbstLabel CreateLabel(string name, string text = "")
    {
        var label = new AbstLabel();
        var impl = new AbstUnityLabel();
        label.Init(impl);
        label.Name = name;
        label.Text = text;
        return label;
    }

    public AbstButton CreateButton(string name, string text = "")
    {
        var button = new AbstButton();
        var impl = new AbstUnityButton();
        button.Init(impl);
        button.Name = name;
        button.Text = text;
        return button;
    }

    public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null) => throw new NotImplementedException();

    public AbstMenu CreateMenu(string name) => throw new NotImplementedException();

    public AbstMenuItem CreateMenuItem(string name, string? shortcut = null) => throw new NotImplementedException();

    public AbstMenu CreateContextMenu(object window) => throw new NotImplementedException();

    public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name) => throw new NotImplementedException();

    public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name) => throw new NotImplementedException();

    public AbstWindow CreateWindow(string name, string title = "") => throw new NotImplementedException();
}
