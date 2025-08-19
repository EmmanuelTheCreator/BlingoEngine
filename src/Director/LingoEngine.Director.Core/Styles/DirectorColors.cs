using AbstUI.Primitives;
using AbstUI.Styles;

namespace LingoEngine.Director.Core.Styles
{
    public class DirectorColors
    {

        public static AColor BlueSelectColor = AbstDefaultColors.BlueSelectColor ;
        public static AColor ListHoverColor = AbstDefaultColors.ListHoverColor ;
        public static AColor BlueLightSelectColor = AbstDefaultColors.BlueLightSelectColor ;
        public static AColor BitmapSelectionFill = AbstDefaultColors.BitmapSelectionFill ;
        public static AColor BlueSelectColorSemiTransparent = AbstDefaultColors.BlueSelectColorSemiTransparent;

        public static AColor BG_PropWindowBar = new AColor(178, 180, 191);    // Top bar of panels
        public static AColor BG_WhiteMenus = new AColor(240, 240, 240);       // Common window background

        // Windows
        public static AColor Window_Title_Line_Under = new AColor(178, 180, 191); // The line just beneath the title of the window
        public static AColor Window_Title_BG = AColor.FromHex("#d2e0ed"); // Base color of the window title bar
        public static AColor Window_Title_BG_Active = Window_Title_BG.Darken(0.2f); // Darker when window is active
        public static AColor Window_Title_BG_Inactive = Window_Title_BG.Lighten(0.2f); // Lighter when window is inactive
        public static AColor Window_Border = new AColor(200, 200, 200);       // borders for the windows


        // Thumbnail of a member
        public static AColor Bg_Thumb = new AColor(255, 255, 255);
        public static AColor Border_Thumb = new AColor(64, 64, 64);


        // Text
        public static AColor TextColorLabels = new AColor(30, 30, 30);        // Property labels
        public static AColor TextColorDisabled = new AColor(130, 130, 130);   // Grayed-out text
        public static AColor TextColorFocused = new AColor(20, 20, 20);       // Active focus color


        // Inputs
        public static AColor InputText = new AColor(30, 30, 30);
        public static AColor InputBorder = new AColor(30, 30, 30);
        public static AColor InputSelection = new AColor(0, 120, 215);        // Windows blue selection
        public static AColor InputSelectionText = new AColor(255, 255, 255);  // Text over selection
        public static AColor Input_Border = new AColor(50, 50, 50);           // Border for text inputs
        public static AColor Input_Bg = new AColor(255, 255, 255);            // Background for text inputs


        // Tabs
        public static AColor BG_Tabs = new AColor(157, 172, 191);             // Inactive tabs
        public static AColor BG_Tabs_Hover = new AColor(120, 133, 150);             // Inactive tabs
        public static AColor TabActiveBorder = new AColor(130, 130, 130);     // Top/side borders
        public static AColor Border_Tabs = new AColor(100, 100, 100);         // Tab outline
        public static AColor Tab_Selected_TextColor = new AColor(0, 0, 0);         // text of the selected tab
        public static AColor Tab_Deselected_TextColor = new AColor(255, 255, 255); // text of the deselected tab

        // Dividers and lines
        public static AColor DividerLines = new AColor(190, 190, 190);        // Light panel separators

        public static AColor LineDarker = AColor.FromHex("a0a0a0");

        // Score grid
        public static AColor ScoreGridLineLight = AColor.FromHex("f9f9f9");
        public static AColor ScoreGridLineDark = AColor.FromHex("f0f0f0");

        public static AColor LineLight = AColor.FromHex("f9f9f9");
        public static AColor LineDark = AColor.FromHex("d0d0d0");

        // Buttons
        public static AColor Button_Bg_Normal = new AColor(240, 240, 240);      // Classic light gray
        public static AColor Button_Bg_Hover = BlueLightSelectColor;                // Hover highlight
        public static AColor Button_Bg_Pressed = new AColor(204, 204, 204);     // Pressed darker gray
        public static AColor Button_Bg_Disabled = new AColor(230, 230, 230);    // Disabled gray

        public static AColor Button_Border_Normal = new AColor(50, 50, 50);     // Standard border
        public static AColor Button_Border_Pressed = new AColor(100, 100, 100); // Slightly darker on press
        public static AColor Button_Border_Disabled = new AColor(190, 190, 190);// Faded border
        public static AColor Button_Border_Hover = BlueSelectColor;                 // Border hover

        public static AColor Button_Text_Normal = new AColor(0, 0, 0);
        public static AColor Button_Text_Disabled = new AColor(130, 130, 130);



        // Notifications
        public static AColor Notification_Warning_Bg = new AColor(255, 255, 204);
        public static AColor Notification_Warning_Border = new AColor(255, 255, 0);
        public static AColor Notification_Error_Bg = new AColor(255, 204, 204);
        public static AColor Notification_Error_Border = new AColor(255, 0, 0);
        public static AColor Notification_Info_Bg = new AColor(204, 204, 255);
        public static AColor Notification_Info_Border = new AColor(0, 0, 255);



        // Popup Window
        public static AColor PopupWindow_Background = new AColor(255, 255, 255);   // Main background
        public static AColor PopupWindow_Border = new AColor(160, 160, 160);       // Frame border

        public static AColor PopupWindow_Header_BG = new AColor(216, 216, 216);    // Header bar (light gray)
        public static AColor PopupWindow_Header_Text = new AColor(0, 0, 0);        // Title text

        public static AColor PopupWindow_CloseButton_BG = new AColor(221, 221, 221);
        public static AColor PopupWindow_CloseButton_Border = new AColor(130, 130, 130);
        public static AColor PopupWindow_CloseButton_Hover = new AColor(255, 0, 0);

        // Menu
        public static AColor BG_TopMenu = new AColor(240, 240, 240); // Background color of the top menu bar


        // Code Highlighting
        public static AColor LingoCodeKeyword = new AColor(0, 0, 200); // Blue
        public static AColor LingoCodeLiteral = new AColor(80, 80, 80); // Dark gray
        public static AColor LingoCodeComment = new AColor(200, 0, 0); // Dark red
        public static AColor LingoCodeBuiltIn = new AColor(0, 128, 0); // Green

        public static AColor CCharpCodeTypes = AColor.FromHex("#2B91AF"); // Teal
        public static AColor CCharpCodeBuiltIn = AColor.FromHex("#0000FF"); // Blue
        public static AColor CCharpCodeString = AColor.FromHex("#A31515"); // Maroon
        public static AColor CCharpCodeComment = AColor.FromHex("#008000"); // Green
        public static AColor CCharpCodeNumber = AColor.FromHex("#098658"); // Dark green

    }
}
