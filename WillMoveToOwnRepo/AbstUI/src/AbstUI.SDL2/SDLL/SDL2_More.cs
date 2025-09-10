#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable IDE1006 // Naming Styles
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AbstUI.SDL2.SDLL
{
    public static partial class SDL_gfx
    {
        [DllImport("SDL2_gfx")]
        public static extern int stringColor(IntPtr renderer, int x, int y, string text, uint color);
    }
}
