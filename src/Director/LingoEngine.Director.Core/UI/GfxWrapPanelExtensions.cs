using LingoEngine.AbstUI.Primitives;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;

namespace LingoEngine.Director.Core.UI
{
    public static class GfxWrapPanelExtensions
    {
        public static GfxWrapPanelBuilder Compose(this LingoGfxWrapPanel panel, GfxWrapPanelBuilder? parent = null)
        {
            var builder = new GfxWrapPanelBuilder(panel, parent);
            return builder;
        }
        public static GfxWrapPanelBuilderForToolBar ComposeForToolBar(this LingoGfxWrapPanel panel, GfxWrapPanelBuilder? parent = null)
        {
            var builder = new GfxWrapPanelBuilderForToolBar(panel, parent);
            return builder;
        }

        public static LingoGfxWrapPanel AddHLine(this LingoGfxWrapPanel panel, string name, float width =0, float paddingLeft = 0)
        {
            var line = panel.Factory.CreateHorizontalLineSeparator(name);
            if (width > 0) line.Width = width;
            if (paddingLeft > 0) line.Margin = new AMargin(paddingLeft, 0, 0, 0);
            panel.AddItem(line);
            return panel;
        }
        public static LingoGfxWrapPanel AddVLine(this LingoGfxWrapPanel panel, string name, float height =0, float paddingTop = 0)
        {
            var line = panel.Factory.CreateVerticalLineSeparator(name);
            if (height > 0) line.Height = height;
            if (paddingTop > 0) line.Margin = new AMargin(0, paddingTop, 0, 0);
            panel.AddItem(line);
            return panel;
        }

        public static LingoGfxItemList AddItemList(this LingoGfxWrapPanel panel, string name, IEnumerable<KeyValuePair<string,string>> items, int width = 100)
        {
            var list = panel.Factory.CreateItemList(name);
            foreach (var item in items)
                list.AddItem(item.Key, item.Value);
            list.Width = width;
            panel.AddItem(list);
            return list;
        }

        public static LingoGfxInputSlider<float> AddSliderFloat(this LingoGfxWrapPanel panel, string name, AOrientation orientation, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
        {
            var slider = panel.Factory.CreateInputSliderFloat(orientation, name, min, max, step, onChange);
            panel.AddItem(slider);
            return slider;
        }

        public static LingoGfxInputSlider<int> AddSliderInt(this LingoGfxWrapPanel panel, string name, AOrientation orientation, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
        {
            var slider = panel.Factory.CreateInputSliderInt(orientation, name, min, max, step, onChange);
            panel.AddItem(slider);
            return slider;
        }
       
    }
}
