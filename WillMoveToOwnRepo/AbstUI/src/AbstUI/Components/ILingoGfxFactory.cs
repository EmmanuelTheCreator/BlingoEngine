using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Factory interface for creating engine-level GFX components.
    /// </summary>
    public interface IAbstUIGfxFactory
    {
        AbstUIGfxCanvas CreateGfxCanvas(string name, int width, int height);
        AbstUIGfxWrapPanel CreateWrapPanel(AOrientation orientation, string name);
        AbstUIGfxPanel CreatePanel(string name);
        AbstUIGfxLayoutWrapper CreateLayoutWrapper(IAbstUIGfxNode content, float? x, float? y);
        AbstUIGfxTabContainer CreateTabContainer(string name);
        AbstUIGfxTabItem CreateTabItem(string name, string title);
        AbstUIGfxScrollContainer CreateScrollContainer(string name);
        AbstUIGfxInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null);
        AbstUIGfxInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null);
        AbstUIGfxInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null);
        AbstUIGfxInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null);
        AbstUIGfxInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null);
        AbstUIGfxSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null);
        AbstUIGfxInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null);
        AbstUIGfxInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null);
        AbstUIGfxItemList CreateItemList(string name, Action<string?>? onChange = null);
        AbstUIGfxColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null);
        AbstUIGfxLabel CreateLabel(string name, string text = "");
        AbstUIGfxButton CreateButton(string name, string text = "");
        AbstUIGfxStateButton CreateStateButton(string name, IAbstUITexture2D? texture = null, string text = "", Action<bool>? onChange = null);
        AbstUIGfxMenu CreateMenu(string name);
        AbstUIGfxMenuItem CreateMenuItem(string name, string? shortcut = null);
        AbstUIGfxMenu CreateContextMenu(object window);
        AbstUIGfxHorizontalLineSeparator CreateHorizontalLineSeparator(string name);
        AbstUIGfxVerticalLineSeparator CreateVerticalLineSeparator(string name);
        AbstUIGfxWindow CreateWindow(string name, string title = "");
    }
}
