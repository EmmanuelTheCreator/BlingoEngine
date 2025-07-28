﻿using LingoEngine.Director.Core.Inspector;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using System.Collections.Generic;

namespace LingoEngine.Director.Core.UI
{
    public static class GfxWrapPanelExtensions
    {
        public static GfxWrapPanelBuilder Compose(this LingoGfxWrapPanel panel, ILingoFrameworkFactory factory)
        {
            var builder = new GfxWrapPanelBuilder(panel, factory);
            return builder;
        }

        public static LingoGfxWrapPanel AddHLine(this LingoGfxWrapPanel panel, ILingoFrameworkFactory factory, string name, float width =0, float paddingLeft = 0)
        {
            var line = factory.CreateHorizontalLineSeparator(name);
            if (width > 0) line.Width = width;
            if (paddingLeft > 0) line.Margin = new LingoMargin(paddingLeft, 0, 0, 0);
            panel.AddItem(line);
            return panel;
        }
        public static LingoGfxWrapPanel AddVLine(this LingoGfxWrapPanel panel, ILingoFrameworkFactory factory, string name, float height =0, float paddingTop = 0)
        {
            var line = factory.CreateVerticalLineSeparator(name);
            if (height > 0) line.Height = height;
            if (paddingTop > 0) line.Margin = new LingoMargin(0, paddingTop, 0, 0);
            panel.AddItem(line);
            return panel;
        }

        public static LingoGfxItemList AddItemList(this LingoGfxWrapPanel panel, ILingoFrameworkFactory factory, string name, IEnumerable<KeyValuePair<string,string>> items, int width = 100)
        {
            var list = factory.CreateItemList(name);
            foreach (var item in items)
                list.AddItem(item.Key, item.Value);
            list.Width = width;
            panel.AddItem(list);
            return list;
        }
       
    }
}
