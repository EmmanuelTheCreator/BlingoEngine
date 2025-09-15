
using Terminal.Gui;

namespace LingoEngine.Net.RNetTerminal
{
    public static class BackgroundHelper
    {
        /// <summary>
        /// Clears the entire screen with the given rune and color scheme.
        /// </summary>
        public static void ClearWith(Rect bounds,Rune rune, Terminal.Gui.Attribute attr)
        {
            var driver = Application.Driver;
            driver.SetAttribute(attr);

            for (int y = bounds.Top; y < bounds.Height; y++)
            {
                driver.Move(0, y);
                for (int x = bounds.Left; x < bounds.Width; x++)
                    driver.AddRune(rune);
            }
        }
    }
}
