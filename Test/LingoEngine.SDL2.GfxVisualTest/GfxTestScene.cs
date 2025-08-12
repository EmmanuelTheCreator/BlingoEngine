using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.GfxVisualTest;

public static class GfxTestScene
{
    public static LingoGfxScrollContainer Build(ILingoGfxFactory factory)
    {
        var scroll = factory.CreateScrollContainer("scroll");
        scroll.X = 20;
        scroll.Y = 20;
        scroll.Width = 760;
        scroll.Height = 560;

        var panel = factory.CreatePanel("rootPanel");
        panel.Width = 720;
        panel.Height = 1400;

        float y = 10f;

        void Add(ILingoGfxNode node, float height = 40)
        {
            panel.AddItem(node, 10, y);
            y += height;
        }

        Add(factory.CreateLabel("label", "Label"));

        Add(factory.CreateButton("button", "Button"));

        Add(factory.CreateStateButton("stateButton", null, "State"));

        Add(factory.CreateInputText("inputText"));

        Add(factory.CreateInputNumberInt("inputNumber", 0, 100));

        Add(factory.CreateSpinBox("spinBox", 0, 10));

        Add(factory.CreateInputCheckbox("checkbox"));

        var combo = factory.CreateInputCombobox("combobox", null);
        combo.AddItem("1", "One");
        combo.AddItem("2", "Two");
        combo.AddItem("3", "Three");
        Add(combo);

        var slider = factory.CreateInputSliderFloat(LingoOrientation.Horizontal, "slider", 0, 1, 0.1f);
        slider.Width = 200;
        Add(slider, 50);

        var colorPicker = factory.CreateColorPicker("colorPicker");
        Add(colorPicker, 80);

        var canvas = factory.CreateGfxCanvas("canvas", 100, 50);
        canvas.Clear(new LingoColor(100, 100, 100, 255));
        canvas.DrawRect(new LingoRect(10, 10, 80, 30), new LingoColor(200, 0, 0, 255));
        Add(canvas, 60);

        var list = factory.CreateItemList("itemList");
        list.Width = 120;
        list.Height = 60;
        list.AddItem("a", "Item A");
        list.AddItem("b", "Item B");
        list.AddItem("c", "Item C");
        Add(list, 80);

        var wrap = factory.CreateWrapPanel(LingoOrientation.Horizontal, "wrapPanel");
        wrap.Width = 300;
        wrap.Height = 60;
        wrap.AddItem(factory.CreateButton("wrapBtn1", "One"));
        wrap.AddItem(factory.CreateButton("wrapBtn2", "Two"));
        wrap.AddItem(factory.CreateButton("wrapBtn3", "Three"));
        Add(wrap, 70);

        var tabs = factory.CreateTabContainer("tabContainer");
        tabs.Width = 300;
        tabs.Height = 120;
        var tab1 = factory.CreateTabItem("tab1", "First");
        tab1.Content = factory.CreateLabel("tab1Label", "Tab 1");
        tabs.AddTab(tab1);
        var tab2 = factory.CreateTabItem("tab2", "Second");
        tab2.Content = factory.CreateLabel("tab2Label", "Tab 2");
        tabs.AddTab(tab2);
        Add(tabs, 130);

        var menu = factory.CreateMenu("menu");
        menu.AddItem(factory.CreateMenuItem("menuItem"));
        var menuBtn = factory.CreateButton("menuBtn", "Menu (not rendered)");
        menuBtn.Pressed += () =>
        {
            menu.PositionPopup(menuBtn);
            menu.Popup();
        };
        Add(menuBtn);

        scroll.AddItem(panel);

        return scroll;
    }
}
