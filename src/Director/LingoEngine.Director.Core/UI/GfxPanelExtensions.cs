using LingoEngine.Director.Core.Styles;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using System.Linq.Expressions;
using LingoEngine.Tools;
using LingoEngine.Bitmaps;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Texts;
using LingoEngine.AbstUI.Primitives;


namespace LingoEngine.Director.Core.UI
{
    public static class GfxPanelExtensions
    {
       
        public static GfxPanelBuilder Compose(this LingoGfxPanel panel, ILingoFrameworkFactory factory)
        {
            var builder = new GfxPanelBuilder(panel, factory);
            return builder;
        }

        public static LingoGfxCanvas SetGfxCanvasAt(this LingoGfxPanel container, string name, float x, float y, int width, int height)
        {
            var canvas = container.Factory.CreateGfxCanvas(name, width, height);
            container.AddItem(canvas, x, y);
            return canvas;
        }

        public static LingoGfxLabel SetLabelAt(this LingoGfxPanel container, string name, float x, float y, string? text = null, int fontSize = 11, int? labelWidth = null, LingoTextAlignment lingoTextAlignment = LingoTextAlignment.Left)
        {
            LingoGfxLabel lbl = container.Factory.CreateLabel(name,text ??"");
            lbl.FontColor = DirectorColors.TextColorLabels;
            lbl.FontSize = fontSize;
            if(labelWidth.HasValue)
                lbl.Width = labelWidth.Value;
            lbl.TextAlignment = lingoTextAlignment;
            container.AddItem(lbl, x, y);
            return lbl;
        }
        public static LingoGfxInputText SetInputTextAt<T>(this LingoGfxPanel container, T element, string name, float x, float y, int width, Expression<Func<T,string?>> property, int maxLength = 0)
        {
            Action<T, string?> setter = property.CompileSetter();
            var control = container.Factory.CreateInputText(name, maxLength,x => setter(element,x));
            control.Text = property.CompileGetter()(element)?.ToString() ?? string.Empty;
            control.Width = width;
            container.AddItem(control, x, y);
            return control;
        }
        public static LingoGfxInputNumber<float> SetInputNumberAt<T>(this LingoGfxPanel container, T element, string name, float x, float y, int width, Expression<Func<T,float>> property, float? min = null, float? max = null)
        {
            Action<T, float> setter = property.CompileSetter();
            var control = container.Factory.CreateInputNumberFloat(name, min, max, x => setter(element, x));
            control.Value = property.CompileGetter()(element);
            control.Width = width;
            container.AddItem(control, x, y);
            return control;
        }
        public static LingoGfxInputNumber<int> SetInputNumberAt<T>(this LingoGfxPanel container, T element, string name, float x, float y, int width, Expression<Func<T,int>> property, int? min = null, int? max = null)
        {
            Action<T, int> setter = property.CompileSetter();
            var control = container.Factory.CreateInputNumberInt(name, min, max, x => setter(element, x));
            control.Value = property.CompileGetter()(element);
            control.Width = width;
            container.AddItem(control, x, y);
            return control;
        }
        public static LingoGfxInputCheckbox SetCheckboxAt<T>(this LingoGfxPanel container, T element, string name, float x, float y, Expression<Func<T,bool>> property)
        {
            Action<T, bool> setter = property.CompileSetter();
            LingoGfxInputCheckbox control = container.Factory.CreateInputCheckbox(name,x => setter(element, x));
            control.Checked = property.CompileGetter()(element);
            container.AddItem(control, x, y);
            return control;
        } 
        public static LingoGfxInputCombobox SetComboBoxAt(this LingoGfxPanel container, IEnumerable<KeyValuePair<string, string>> items, string name, float x, float y, int width = 100, string? selectedKey = null, Action<string?>? onChange = null)
        {
            var list = container.Factory.CreateInputCombobox(name, onChange);
            foreach (var item in items)
                list.AddItem(item.Key, item.Value);
            if (selectedKey != null)
                list.SelectedKey = selectedKey;
            list.Width = width;
            container.AddItem(list, x, y);
            return list;
        }
        public static LingoGfxItemList SetInputListAt(this LingoGfxPanel container, IEnumerable<KeyValuePair<string, string>> items, string name, float x, float y, int width = 100, string? selectedKey = null, Action<string?>? onChange = null)
        {
            var list = container.Factory.CreateItemList(name, onChange);
            foreach (var item in items)
                list.AddItem(item.Key, item.Value);
            if (selectedKey != null)
                list.SelectedKey = selectedKey;
            list.Width = width;
            container.AddItem(list, x, y);
            return list;
        }

        public static (LingoGfxButton Button, ILingoGfxLayoutNode Layout) SetButtonAt(this LingoGfxPanel container, string name, string text, float x, float y, Action onClick, int width = 80)
        {
            var control = container.Factory.CreateButton(name, text);
            control.Width = width;
            control.Pressed += onClick;
            ILingoGfxLayoutNode layout = (ILingoGfxLayoutNode)container.AddItem(control, x, y);
            return (control, layout);
        }
        public static LingoGfxInputSlider<float> SetSliderAt<T>(this LingoGfxPanel container, T element, string name, float x, float y, int width, AOrientation orientation, Expression<Func<T, float>> property, float? min = null, float? max = null, float? step = null)
        {
            Action<T, float> setter = property.CompileSetter();
            var slider = container.Factory.CreateInputSliderFloat(orientation, name, min, max, step, v => setter(element, v));
            slider.Value = property.CompileGetter()(element);
            slider.Width = width;
            container.AddItem(slider, x, y);
            return slider;
        }

        public static LingoGfxInputSlider<int> SetSliderAt<T>(this LingoGfxPanel container, T element, string name, float x, float y, int width, AOrientation orientation, Expression<Func<T, int>> property, int? min = null, int? max = null, int? step = null)
        {
            Action<T, int> setter = property.CompileSetter();
            var slider = container.Factory.CreateInputSliderInt(orientation, name, min, max, step, v => setter(element, v));
            slider.Value = property.CompileGetter()(element);
            slider.Width = width;
            container.AddItem(slider, x, y);
            return slider;
        }
        public static LingoGfxStateButton SetStateButtonAt<T>(this LingoGfxPanel container, T element, string name, float x, float y, Expression<Func<T,bool>> property, ILingoTexture2D? texture = null, string? label = null)
        {
            Action<T, bool> setter = property.CompileSetter();
            LingoGfxStateButton control = container.Factory.CreateStateButton(name, texture, label ??"", onChange: val => setter(element, val));
            control.IsOn = property.CompileGetter()(element);
            container.AddItem(control, x, y);
            return control;
        }
        public static LingoGfxCanvas AddVLine(this LingoGfxPanel container, string name, float x, float y, float height)
        {
            var paintPanel = container.Factory.CreateGfxCanvas(name, 2, (int)height);
            paintPanel.DrawLine(new APoint(0, 0), new APoint(0, height), DirectorColors.LineLight, 1);
            paintPanel.DrawLine(new APoint(1, 0), new APoint(1, height), DirectorColors.LineDark, 1);
            container.AddItem(paintPanel, x, y);
            return paintPanel;
        } 
        public static LingoGfxCanvas AddHLine(this LingoGfxPanel container, string name, float x, float y, float width)
        {
            var paintPanel = container.Factory.CreateGfxCanvas(name, (int)width, 2);
            paintPanel.DrawLine(new APoint(0, 0), new APoint(width, 0), DirectorColors.LineLight, 1);
            paintPanel.DrawLine(new APoint(0, 1), new APoint(width, 1), DirectorColors.LineDark, 1);
            container.AddItem(paintPanel, x, y);
            return paintPanel;
        }
        public static void AddPopupButtons(this LingoGfxPanel container, Action okAction, Action onClose)
        {
            container.AddVLine("VLineBtns", container.Width - 100, 10, container.Height - 10 - 10);

            container.SetButtonAt("OKBtn", "OK", container.Width - 90, 10, () => { okAction(); onClose(); },80);
            container.SetButtonAt("CancelBtn", "Cancel", container.Width - 90, 40, onClose,80);
        }

    }
}
