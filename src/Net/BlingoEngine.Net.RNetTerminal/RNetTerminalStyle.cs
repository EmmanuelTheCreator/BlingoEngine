using System.Linq;
using System.Xml.Linq;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Views;

namespace BlingoEngine.Net.RNetTerminal
{
    internal class RNetTerminalStyle
    {
        public static char CharLight = '░';
        public static char CharMid = '▒';
        public static char CharDark = '▓';
        public static char CharFull = '█';

        public static Scheme DefaultScheme { get; internal set; } = null!;
        public static Scheme MenuScheme { get; internal set; } = null!;


        public static void SetMyTheme()
        {
            //ConfigurationManager.RuntimeConfig = """{ "Theme": "Light" }""";
            //ConfigurationManager.Enable(ConfigLocations.All);
            var baseScheme = new Scheme
            {
                Normal = new Attribute(Color.White, Color.BrightBlue),
                Focus = new Attribute(Color.Black, Color.White),
                HotNormal = new Attribute(Color.BrightYellow, Color.BrightBlue),
                HotFocus = new Attribute(Color.Black, Color.White),
                Disabled = new Attribute(Color.Gray, Color.BrightBlue)
            };

            //Application.Driver.SetAttribute(new Attribute(Color.White, Color.BrightBlue));
            //Colors.Base = baseScheme;
            //Colors.Dialog = baseScheme;
            //Colors.Error = baseScheme;
            var menuScheme = new Scheme
            {
                Normal = new Attribute(Color.Black, Color.Gray),
                Focus = new Attribute(Color.Black, Color.White),
                HotNormal = new Attribute(Color.Green, Color.Gray),
                HotFocus = new Attribute(Color.Green, Color.White),
                Disabled = new Attribute(Color.DarkGray, Color.Gray)
            };
            DefaultScheme = baseScheme;
            MenuScheme = menuScheme;
            //Colors.Menu = menuScheme;
            //Colors.TopLevel = baseScheme;
            //Application.Top.SetScheme(_baseScheme);
            //if (Application.Top is { } top)
            //{
            //    top.ColorScheme = baseScheme;
            //}
            Application.Force16Colors = true;
            var theme = ThemeManager.GetCurrentTheme();
            //theme.AddOrUpdate("Window", new ConfigProperty { PropertyValue = });
            // todo : set default colors

        }

        internal static void SetTileViewSchema(TileView element)
        {
            var scheme = new Scheme
            {
                Normal = new Attribute(Color.Gray, Color.Blue),
                Focus = new Attribute(Color.White, Color.Blue),
                //HotNormal = new Attribute(Color.White, Color.BrightBlue),
                //HotFocus = new Attribute(Color.White, Color.BrightBlue),
                //Disabled = new Attribute(Color.DarkGray, Color.Gray),
            };
            element.SetScheme(scheme);
            //foreach (var item in element.SubViews)
            //{
            //    item.SetScheme(scheme);
                
            //}
        }
        internal static void SetMenuSchema(MenuBarv2 menu)
        {
            menu.SetScheme(MenuScheme);
            //foreach (var item in menu.SubViews)
            //{
            //    item.SetScheme(MenuScheme);
            //    foreach (var item2 in item.SubViews)
            //    {
            //        item2.SetScheme(MenuScheme);
            //    }
            //}
        }

        internal static void SetStatusBar(Label element)
        {
            var scheme = new Scheme
            {
                Normal = new Attribute(Color.DarkGray, Color.Black),
                Focus = new Attribute(Color.Gray, Color.Black),
                //HotNormal = new Attribute(Color.White, Color.BrightBlue),
                //HotFocus = new Attribute(Color.White, Color.BrightBlue),
                //Disabled = new Attribute(Color.DarkGray, Color.Gray),
            };
            element.SetScheme(scheme);
        }

        internal static void SetForDialog(Dialog dialog)
        {
            var baseScheme = new Scheme
            {
                Normal = new Attribute(Color.Gray, Color.Blue),
                Focus = new Attribute(Color.White, Color.BrightBlue),
                HotNormal = new Attribute(Color.BrightYellow, Color.BrightBlue),
                //HotFocus = new Attribute(Color.Black, Color.White),
                //Disabled = new Attribute(Color.Gray, Color.BrightBlue)
            };
            dialog.SetScheme(baseScheme);
        }
        internal static void SetForTextField(TextField dialog)
        {
            var baseScheme = new Scheme
            {
                //Normal = new Attribute(Color.Gray, Color.Black),
                //Focus = new Attribute(Color.Black, Color.DarkGray),
                //HotNormal = new Attribute(Color.BrightYellow, Color.BrightBlue),
                //HotFocus = new Attribute(Color.Green, Color.Black),
                //Disabled = new Attribute(Color.Gray, Color.Black),
                //Active = new Attribute(Color.Cyan, Color.Black),
                //Editable = new Attribute(Color.Yellow, Color.Black),
                //Highlight = new Attribute(Color.Black, Color.Yellow)  ,
                //HotActive = new Attribute(Color.Green, Color.Yellow),
            };
            dialog.SetScheme(baseScheme);
        }
        internal static void SetForTableView(TableView element)
        {
            var baseScheme = new Scheme
            {
                Normal = new Attribute(Color.Gray, Color.Blue),
                Focus = new Attribute(Color.White, Color.BrightBlue),
                Active = new Attribute(Color.White, Color.Green),
                //HotNormal = new Attribute(Color.BrightYellow, Color.BrightBlue),
                //HotFocus = new Attribute(Color.Black, Color.White),
                //Disabled = new Attribute(Color.Gray, Color.BrightBlue)
            };
            element.SetScheme(baseScheme);
        }
        internal static void SetForTableView(Tab element)
        {
            var baseScheme = new Scheme
            {
                Normal = new Attribute(Color.Gray, Color.Blue),
                Focus = new Attribute(Color.White, Color.BrightBlue),
                Active = new Attribute(Color.White, Color.Green),
                //HotNormal = new Attribute(Color.Red, Color.BrightBlue),
                //HotFocus = new Attribute(Color.Yellow, Color.White),
                //Highlight= new Attribute(Color.Magenta, Color.White),
                //Disabled = new Attribute(Color.Gray, Color.Black)
            };
            element.SetScheme(baseScheme);
        }
    }
}

