using AbstUI.Components;
using AbstUI.Components.Texts;
using AbstUI.GfxVisualTest;
using AbstUI.Primitives;
using AbstUI.Texts;
using AbstUI.Windowing;

namespace BlingoEngine.SDL2.GfxVisualTest;

public static class GfxTestScene
{
    public static IAbstNode Build(IAbstComponentFactory factory)
    {
        var windowManager = factory.GetRequiredService<IAbstWindowManager>();


        
        //var wdw = new GfxTestWindow();

        var scroll = factory.CreateScrollContainer("scroll_root");
        //
        //var scroll = factory.CreateWrapPanel(AOrientation.Vertical,"scroll");
        //var scroll = factory.CreatePanel("scroll");
        scroll.X = 20;
        scroll.Y = 20;
        scroll.Width = 760;
        scroll.Height = 560;
        //windowManager.OpenWindow(GfxTestWindow.MyWindowCode);
       // return scroll;


        //var panel = factory.CreatePanel("rootPanel");
        var panel = factory.CreateWrapPanel(AOrientation.Vertical, "scroll");
        scroll.AddItem(panel);
        panel.Width =500;
        panel.Height = 1400;
        //panel.BackgroundColor = AColor.FromRGBA(220, 250, 250, 255);

        float y = 0; // 20f + 100;

        void Add(IAbstNode node, float height = 40)
        {
            panel.AddItem(node);
            //panel.AddItem(node, 40, y);
            y += height;
            
        }

        Add(CreateLabel(factory, "label1"), 22);
        Add(CreateLabel(factory, "Label2 center", lbl2 =>
        {
            lbl2.Width = 200;
            lbl2.TextAlignment = AbstTextAlignment.Center;
        }), 22);
        Add(CreateLabel(factory, "Label3 right", lbl3 =>
        {
            lbl3.Width = 200;
            lbl3.TextAlignment = AbstTextAlignment.Right;
        }), 22);
        Add(CreateLabel(factory, "Label4 BIG", c => c.FontSize = 30));

        var canvas2 = factory.CreateGfxCanvas("canvas1", 100, 50);
        canvas2.Clear(AColor.FromRGBA(100, 100, 100, 0));
        canvas2.DrawCircle(new APoint(50, 25), 20, AColor.FromRGBA(200, 0, 0, 255));
        canvas2.DrawRect(new ARect(10, 10, 80, 30), AColor.FromRGBA(0, 200, 0, 255));
        canvas2.DrawText(new APoint(30, 15), "Hallo", null, AColors.DarkGray);
        Add(canvas2);


        y += 50;
        var colorPicker = factory.CreateColorPicker("colorPicker");
        Add(colorPicker, 80);


        var numClicked = 0;
        var testBtnLabel = CreateLabel(factory, "Button not clicked");
        testBtnLabel.Width = 300;

        var btn1 = factory.CreateButton("button", "Button");
        btn1.Pressed += () =>
        {
            numClicked++;
            testBtnLabel.Text = $"Button clicked {numClicked} times";
            windowManager.OpenWindow(GfxTestWindow.MyWindowCode);
        };
        var btnpanel = factory.CreateWrapPanel(AOrientation.Horizontal, "scroll");
        btnpanel.Width = 500;
        btnpanel.AddItem(btn1);
        btnpanel.AddItem(testBtnLabel);
        Add(btnpanel);
        Add(btn1, 22);

        Add(testBtnLabel, 22);

        var stateButton = factory.CreateStateButton("stateButton",null, "State Button",s =>
        {
            testBtnLabel.Text = $"Button state is {s}";
        });
        //stateButton.IconTexture = factory.CreateTextureFromFile("Assets/Icons/heart.png");
        Add(stateButton);

        var txt1 = factory.CreateInputText("inputText");
        txt1.Text = "test";
        Add(txt1);
        var txtMulti = factory.CreateInputText("inputTextMulti");
        txtMulti.IsMultiLine = true;
        txtMulti.Height = 50;
        txtMulti.Width = 200;
        txtMulti.Text = "test\nother line\n other line 2";
        Add(txtMulti);


        Add(factory.CreateInputNumberInt("inputNumber", 0, 100));

        Add(factory.CreateSpinBox("spinBox", 0, 10));

        Add(factory.CreateInputCheckbox("checkbox"));

        var combo = factory.CreateInputCombobox("combobox", null);
        combo.AddItem("1", "One");
        combo.AddItem("2", "Two");
        combo.AddItem("3", "Three");
        combo.AddItem("4", "Four");
        combo.AddItem("5", "Five");
        combo.SelectedIndex = 1;
        Add(combo, 100);
        //return scroll;
        var slider = factory.CreateInputSliderFloat(AOrientation.Horizontal, "slider", 0, 1, 0.1f);
        slider.Width = 200;
        Add(slider, 50);



        var list = factory.CreateItemList("itemList");
        list.Width = 120;
        list.Height = 60;
        list.AddItem("a", "Item A");
        list.AddItem("b", "Item B B");
        list.AddItem("c", "Item C");
        list.AddItem("d", "Item D long");
        Add(list, 80);
        //return scroll;

        var tabs = factory.CreateTabContainer("tabContainer");
        tabs.Width = 300;
        tabs.Height = 120;
        var tab1 = factory.CreateTabItem("tab1", "First tab");
        var tab1Panel = factory.CreateWrapPanel(AOrientation.Vertical, "tab1panel");
        tab1Panel.AddItem(factory.CreateButton("tab1Btn1", "Button 1"));
        tab1.Content = tab1Panel;
        tabs.AddTab(tab1);
        var tab2 = factory.CreateTabItem("tab2", "Second tab");
        var tab2Panel = factory.CreateWrapPanel(AOrientation.Vertical, "tab2panel");
        tab2Panel.AddItem(factory.CreateButton("tab2Btn2", "Button 2"));
        tab2.Content = tab2Panel;
        tabs.AddTab(tab2);

        Add(tabs, 130);

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
        label.FontColor = AColors.Black;
        if (configure != null)
            configure(label);
        return label;

    }
}

