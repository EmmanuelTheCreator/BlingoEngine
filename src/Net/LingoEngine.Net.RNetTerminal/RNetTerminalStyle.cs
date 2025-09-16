using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;

namespace LingoEngine.Net.RNetTerminal
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
    }
}
