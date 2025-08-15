using System;
using LingoEngine.AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Inputs;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Factory interface for creating engine-level GFX components.
    /// </summary>
    public interface ILingoGfxFactory
    {
        LingoGfxCanvas CreateGfxCanvas(string name, int width, int height);
        LingoGfxWrapPanel CreateWrapPanel(AOrientation orientation, string name);
        LingoGfxPanel CreatePanel(string name);
        LingoGfxLayoutWrapper CreateLayoutWrapper(ILingoGfxNode content, float? x, float? y);
        LingoGfxTabContainer CreateTabContainer(string name);
        LingoGfxTabItem CreateTabItem(string name, string title);
        LingoGfxScrollContainer CreateScrollContainer(string name);
        LingoGfxInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null);
        LingoGfxInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null);
        LingoGfxInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null);
        LingoGfxInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null);
        LingoGfxInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null);
        LingoGfxSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null);
        LingoGfxInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null);
        LingoGfxInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null);
        LingoGfxItemList CreateItemList(string name, Action<string?>? onChange = null);
        LingoGfxColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null);
        LingoGfxLabel CreateLabel(string name, string text = "");
        LingoGfxButton CreateButton(string name, string text = "");
        LingoGfxStateButton CreateStateButton(string name, ILingoTexture2D? texture = null, string text = "", Action<bool>? onChange = null);
        LingoGfxMenu CreateMenu(string name);
        LingoGfxMenuItem CreateMenuItem(string name, string? shortcut = null);
        LingoGfxMenu CreateContextMenu(object window);
        LingoGfxHorizontalLineSeparator CreateHorizontalLineSeparator(string name);
        LingoGfxVerticalLineSeparator CreateVerticalLineSeparator(string name);
        LingoGfxWindow CreateWindow(string name, string title = "");
    }
}
