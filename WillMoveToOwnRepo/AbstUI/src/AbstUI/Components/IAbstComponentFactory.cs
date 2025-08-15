using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Factory interface for creating engine-level GFX components.
    /// </summary>
    public interface IAbstComponentFactory
    {
        AbstGfxCanvas CreateGfxCanvas(string name, int width, int height);
        AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name);
        AbstPanel CreatePanel(string name);
        AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y);
        AbstTabContainer CreateTabContainer(string name);
        AbstTabItem CreateTabItem(string name, string title);
        AbstScrollContainer CreateScrollContainer(string name);
        AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null);
        AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null);
        AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null);
        AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null);
        AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null);
        AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null);
        AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null);
        AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null);
        AbstItemList CreateItemList(string name, Action<string?>? onChange = null);
        AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null);
        AbstLabel CreateLabel(string name, string text = "");
        AbstButton CreateButton(string name, string text = "");
        AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null);
        AbstMenu CreateMenu(string name);
        AbstMenuItem CreateMenuItem(string name, string? shortcut = null);
        AbstMenu CreateContextMenu(object window);
        AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name);
        AbstVerticalLineSeparator CreateVerticalLineSeparator(string name);
        AbstWindow CreateWindow(string name, string title = "");
    }
}
