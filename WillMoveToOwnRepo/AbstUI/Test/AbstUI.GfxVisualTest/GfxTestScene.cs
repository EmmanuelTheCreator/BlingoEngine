using AbstUI.Texts;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Components.Texts;

namespace LingoEngine.SDL2.GfxVisualTest;

public static class GfxTestScene
{
    public static IAbstNode Build(IAbstComponentFactory factory)
    {
        var scroll = factory.CreateScrollContainer("scroll");
        //var scroll = factory.CreateWrapPanel(AOrientation.Vertical,"scroll");
        scroll.X = 20;
        scroll.Y = 20;
        scroll.Width = 760;
        scroll.Height = 560;

        var panel = factory.CreatePanel("rootPanel");
        //var panel = factory.CreateWrapPanel(AOrientation.Vertical, "scroll");
        scroll.AddItem(panel);
        panel.Width = 300;
        panel.Height = 1400;
        panel.BackgroundColor = new AColor(00, 50, 50, 255);

        float y = 20f;

        void Add(IAbstNode node, float height = 40)
        {
            panel.AddItem(node, 20, y);
            y += height;
        }

        Add(CreateLabel(factory, "label1"));
        Add(CreateLabel(factory, "Label2 center", lbl2 =>
        {
            lbl2.Width = 200;
            lbl2.TextAlignment = AbstTextAlignment.Center;
        }));
        Add(CreateLabel(factory, "Label3 right", lbl3 =>
        {
            lbl3.Width = 200;
            lbl3.TextAlignment = AbstTextAlignment.Right;
        }));
        Add(CreateLabel(factory, "Label4 BIG", c => c.FontSize = 30));

        var canvas2 = factory.CreateGfxCanvas("canvas1", 100, 50);
        canvas2.Clear(new AColor(100, 100, 100, 255));
        canvas2.DrawCircle(new APoint(50, 25), 20, new AColor(200, 0, 0, 255));
        canvas2.DrawRect(new ARect(10, 10, 80, 30), new AColor(0, 200, 0, 255));
        canvas2.DrawText(new APoint(5, 5), "Hallo", null, AColors.Green);
        Add(canvas2);
        Add(factory.CreateButton("button", "Button"));

        //Add(factory.CreateStateButton("stateButton", null, "State"));

        //Add(factory.CreateInputText("inputText"));

        ////Add(factory.CreateInputNumberInt("inputNumber", 0, 100));

        //Add(factory.CreateSpinBox("spinBox", 0, 10));

        //Add(factory.CreateInputCheckbox("checkbox"));

        //var combo = factory.CreateInputCombobox("combobox", null);
        //combo.AddItem("1", "One");
        //combo.AddItem("2", "Two");
        //combo.AddItem("3", "Three");
        //Add(combo);

        //var slider = factory.CreateInputSliderFloat(AOrientation.Horizontal, "slider", 0, 1, 0.1f);
        //slider.Width = 200;
        //Add(slider, 50);

        //var colorPicker = factory.CreateColorPicker("colorPicker");
        //Add(colorPicker, 80);

        //AbstGfxCanvas canvas = factory.CreateGfxCanvas("canvas", 100, 50);
        //canvas.Clear(new AColor(100, 100, 100, 255));
        //canvas.DrawRect(new ARect(10, 10, 80, 30), new AColor(200, 0, 0, 255));
        //Add(canvas, 60);

        //var list = factory.CreateItemList("itemList");
        //list.Width = 120;
        //list.Height = 60;
        //list.AddItem("a", "Item A");
        //list.AddItem("b", "Item B");
        //list.AddItem("c", "Item C");
        //Add(list, 80);

        //var wrap = factory.CreateWrapPanel(AOrientation.Horizontal, "wrapPanel");
        //wrap.Width = 300;
        //wrap.Height = 60;
        //wrap.AddItem(factory.CreateButton("wrapBtn1", "One"));
        //wrap.AddItem(factory.CreateButton("wrapBtn2", "Two"));
        //wrap.AddItem(factory.CreateButton("wrapBtn3", "Three"));
        //Add(wrap, 70);

        //var tabs = factory.CreateTabContainer("tabContainer");
        //tabs.Width = 300;
        //tabs.Height = 120;
        //var tab1 = factory.CreateTabItem("tab1", "First");
        //var tab1Panel = factory.CreateWrapPanel(AOrientation.Vertical, "tab1panel");
        //tab1Panel.AddItem(factory.CreateButton("tab1Btn1", "Button 1"));
        //tab1.Content = tab1Panel;
        //tabs.AddTab(tab1);
        //var tab2 = factory.CreateTabItem("tab2", "First");
        //var tab2Panel = factory.CreateWrapPanel(AOrientation.Vertical, "tab2panel");
        //tab2Panel.AddItem(factory.CreateButton("tab2Btn2", "Button 2"));
        //tab2.Content = tab2Panel;
        //tabs.AddTab(tab2);

        //Add(tabs, 130);

        //var menu = factory.CreateMenu("menu");
        //menu.AddItem(factory.CreateMenuItem("menuItem"));
        //var menuBtn = factory.CreateButton("menuBtn", "Menu (not rendered)");
        //menuBtn.Pressed += () =>
        //{
        //    menu.PositionPopup(menuBtn);
        //    menu.Popup();
        //};
        //Add(menuBtn);

        return scroll;
    }

    private static AbstLabel CreateLabel(IAbstComponentFactory factory, string text, Action<AbstLabel>? configure = null)
    {

        var label = factory.CreateLabel(text.Replace(" ", "_"), text);
        label.FontColor = AColors.Green;
        if (configure != null)
            configure(label);
        return label;

    }
}
