using System;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Blazor.Components;
using AbstUI.Styles;
using AbstUI.Windowing;

namespace AbstUI.Blazor;

public class AbstBlazorComponentFactory : AbstComponentFactoryBase, IAbstComponentFactory
{

    private readonly IAbstFontManager _fontManager;
      private readonly AbstBlazorComponentMapper _mapper;

    public AbstBlazorComponentFactory(IServiceProvider serviceProvider, AbstBlazorComponentMapper mapper) : base(serviceProvider)
    {
        _fontManager = FontManager;
         _mapper = mapper;
        _mapper.Map<AbstBlazorWrapPanelComponent>(typeof(Components.AbstBlazorWrapPanel));
        _mapper.Map<AbstBlazorPanelComponent>(typeof(Components.AbstBlazorPanel));
        _mapper.Map<AbstBlazorTabContainerComponent>(typeof(Components.AbstBlazorTabContainer));
        _mapper.Map<AbstBlazorScrollContainerComponent>(typeof(Components.AbstBlazorScrollContainer));
        _mapper.Map<AbstBlazorInputCheckboxComponent>(typeof(Components.AbstBlazorInputCheckbox));
        _mapper.Map<AbstBlazorButtonComponent>(typeof(Components.AbstBlazorButton));
        _mapper.Map<AbstBlazorLabelComponent>(typeof(Components.AbstBlazorLabel));
        _mapper.Map<AbstBlazorInputTextComponent>(typeof(Components.AbstBlazorInputText));
        _mapper.Map<AbstBlazorLayoutWrapper>(typeof(Components.AbstBlazorLayoutWrapper));
    }

    public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height)
    {
        var canvas = new AbstGfxCanvas();
        var impl = new AbstBlazorGfxCanvas();
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
        var impl = new AbstBlazorWrapPanelComponent();
        panel.Init(impl);
        InitComponent(panel);
        panel.Name = name;
        panel.Orientation = orientation;
        return panel;
    }

    public AbstPanel CreatePanel(string name)
    {
        var panel = new AbstPanel(this);
        var impl = new AbstBlazorPanelComponent();
        panel.Init(impl);
        InitComponent(panel);
        panel.Name = name;
        return panel;
    }
    public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y)
    {
        if (content is IAbstLayoutNode)
            throw new InvalidOperationException($"Content {content.Name} already supports layout wrapping is unnecessary.");

        var wrapper = new AbstLayoutWrapper(content);
        var impl = new AbstBlazorLayoutWrapper(wrapper);
        wrapper.Init(impl);
        InitComponent(wrapper);
        if (x.HasValue) wrapper.X = x.Value;
        if (y.HasValue) wrapper.Y = y.Value;
        return wrapper;
    }

    public AbstTabContainer CreateTabContainer(string name)
    {
        var tab = new AbstTabContainer();
        var impl = new AbstBlazorTabContainerComponent();
        tab.Init(impl);
        InitComponent(tab);
        tab.Name = name;
        return tab;
    }

    public AbstTabItem CreateTabItem(string name, string title)
    {
        var tab = new AbstTabItem();
        var impl = new AbstBlazorTabItemComponent();
        tab.Init(impl);
        InitComponent(tab);
        tab.Name = name;
        tab.Title = title;
        return tab;
    }

    public AbstScrollContainer CreateScrollContainer(string name)
    {
        var scroll = new AbstScrollContainer();
        var impl = new AbstBlazorScrollContainerComponent();
        scroll.Init(impl);
        InitComponent(scroll);
        scroll.Name = name;
        return scroll;
    }
    public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
    {
        var slider = new AbstInputSlider<float>();
        var impl = new AbstBlazorInputSlider<float>();
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
        var impl = new AbstBlazorInputSlider<int>();
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

    public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
    {
        var input = new AbstInputNumber<float>();
        var impl = new AbstBlazorInputNumber<float>();
        input.Init(impl);
        InitComponent(input);
        input.Name = name;
        if (min.HasValue) input.Min = min.Value;
        if (max.HasValue) input.Max = max.Value;
        if (onChange != null)
            input.ValueChanged += () => onChange(input.Value);
        return input;
    }

    public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
    {
        var input = new AbstInputNumber<int>();
        var impl = new AbstBlazorInputNumber<int>();
        input.Init(impl);
        InitComponent(input);
        input.Name = name;
        if (min.HasValue) input.Min = min.Value;
        if (max.HasValue) input.Max = max.Value;
        if (onChange != null)
            input.ValueChanged += () => onChange(input.Value);
        return input;
    }

    public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
    {
        var spin = new AbstInputSpinBox();
        var impl = new AbstBlazorSpinBox();
        spin.Init(impl);
            InitComponent(spin);
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
        var impl = new AbstBlazorInputCheckboxComponent();
        impl.Name = name;
        input.Init(impl);
            InitComponent(input);
        input.Name = name;
        if (onChange != null)
            input.ValueChanged += () => onChange(input.Checked);
        return input;
    }

    public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
    {
        var input = new AbstInputCombobox();
        var impl = new AbstBlazorInputCombobox();
        input.Init(impl);
            InitComponent(input);
        input.Name = name;
        if (onChange != null)
            input.ValueChanged += () => onChange(input.SelectedKey);
        return input;
    }

    public AbstItemList CreateItemList(string name, Action<string?>? onChange = null)
    {
        var list = new AbstItemList();
        var impl = new AbstBlazorItemList();
        list.Init(impl);
            InitComponent(list);
        list.Name = name;
        if (onChange != null)
            list.ValueChanged += () => onChange(list.SelectedKey);
        return list;
    }

    public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null)
    {
        var picker = new AbstColorPicker();
        var impl = new AbstBlazorColorPicker();
        picker.Init(impl);
            InitComponent(picker);
        picker.Name = name;
        if (onChange != null)
            picker.ValueChanged += () => onChange(picker.Color);
        return picker;
    }

    public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null)
    {
        var button = new AbstStateButton();
        var impl = new AbstBlazorStateButton();
        button.Init(impl);
            InitComponent(button);
        button.Name = name;
        button.Text = text;
        if (texture != null) button.TextureOn = texture;
        if (onChange != null)
            button.ValueChanged += () => onChange(button.IsOn);
        return button;
    }
    public AbstMenu CreateMenu(string name)
    {
        var menu = new AbstMenu();
        var impl = new AbstBlazorMenu(this, name);
        menu.Init(impl);
            InitComponent(menu);
        menu.Name = name;
        return menu;
    }

    public AbstMenuItem CreateMenuItem(string name, string? shortcut = null)
    {
        var item = new AbstMenuItem();
        var impl = new AbstBlazorMenuItem(this, name, shortcut);
        item.Init(impl);
        item.Name = name;
        item.Shortcut = shortcut;
        return item;
    }

    public AbstMenu CreateContextMenu(object window)
    {
        var menu = new AbstMenu();
        InitComponent(menu);
        menu.Name = "ContextMenu";
        return menu;
    }

    public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
    {
        var sep = new AbstHorizontalLineSeparator();
        InitComponent(sep);
        sep.Name = name;
        return sep;
    }

    public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name)
    {
        var sep = new AbstVerticalLineSeparator();
        InitComponent(sep);
        sep.Name = name;
        return sep;
    }

   

    public AbstButton CreateButton(string name, string text = "")
    {
        var button = new AbstButton();
        InitComponent(button);

        button.Name = name;
        button.Text = text;
        return button;
    }

    public AbstLabel CreateLabel(string name, string text = "")
    {
        var label = new AbstLabel();
        InitComponent(label);

        label.Name = name;
        label.Text = text;
        return label;
    }

    public AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null, bool multiLine = false)
    {
        var input = new AbstInputText();
        var impl = new AbstBlazorInputTextComponent { IsMultiLine = multiLine, MaxLength = maxLength };
        input.Init(impl);
        InitComponent(input);
        input.Name = name;
        if (onChange != null)
            input.ValueChanged += () => onChange(input.Text);
        return input;
    }
}
